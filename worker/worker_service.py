#!/usr/bin/env python3
"""
Clinical Intelligence Worker Service

This service continuously listens to RabbitMQ for document processing jobs
and processes them asynchronously.
"""

import json
import os
import sys
import time
import signal
from typing import Dict, Any, Optional
import pika
from pika.exceptions import AMQPConnectionError, AMQPChannelError

# Add parent directory to path for imports
sys.path.insert(0, os.path.dirname(os.path.dirname(__file__)))
sys.path.insert(0, os.path.dirname(__file__))

try:
    from jsonschema import Draft7Validator
    from worker.config import load_config
    from worker.main import validate_job_payload, extract_text_from_job, run_entity_extraction_pipeline
    from worker.entity_extraction.patient_extractor import extract_patient_from_text
    from worker.database.patient_manager import PatientManager
    from worker.entity_extraction.response_parser import parse_and_validate_response
    from worker.entity_extraction.models import ExtractionInput, ChunkWithProvenance
except ImportError as e:
    print(f"Import error: {e}")
    print("Make sure all dependencies are installed: pip install -r requirements.txt")
    sys.exit(1)


class WorkerService:
    """Continuous worker service that processes document jobs from RabbitMQ"""
    
    def __init__(self):
        self.config = load_config()
        self.connection = None
        self.channel = None
        self.queue_name = "document_processing_jobs"
        self.running = False
        
        # Initialize patient manager - use DATABASE_URL (Python PostgreSQL URI format)
        db_conn_string = os.getenv('DATABASE_URL', 'postgresql://postgres:admin@localhost:5432/ClinicalIntelligence')
        self.patient_manager = PatientManager(db_conn_string)
        
    def connect_to_rabbitmq(self) -> bool:
        """Connect to RabbitMQ server"""
        try:
            # Connection parameters
            credentials = pika.PlainCredentials('guest', 'guest')
            parameters = pika.ConnectionParameters(
                host='localhost',
                port=5672,
                virtual_host='/',
                credentials=credentials,
                heartbeat=600,
                blocked_connection_timeout=300
            )
            
            print("Connecting to RabbitMQ...")
            self.connection = pika.BlockingConnection(parameters)
            self.channel = self.connection.channel()
            
            # Declare queue (durable)
            self.channel.queue_declare(
                queue=self.queue_name,
                durable=True,
                arguments={'x-message-ttl': 3600000}  # 1 hour TTL
            )
            
            # Set QoS to process one message at a time
            self.channel.basic_qos(prefetch_count=1)
            
            print(f"✅ Connected to RabbitMQ - Queue: {self.queue_name}")
            return True
            
        except (AMQPConnectionError, AMQPChannelError) as e:
            print(f"❌ Failed to connect to RabbitMQ: {e}")
            return False
        except Exception as e:
            print(f"❌ Unexpected error connecting to RabbitMQ: {e}")
            return False
    
    def process_job(self, channel, method, properties, body):
        """Process a single job from the queue"""
        try:
            # Parse job
            raw_body = body.decode('utf-8')
            print(f"\n{'='*80}")
            print(f"📨 RAW MESSAGE RECEIVED:")
            print(f"{'='*80}")
            print(raw_body)
            print(f"{'='*80}\n")
            
            job_data = json.loads(raw_body)
            job_id = job_data.get('job_id', 'unknown')
            document_id = job_data.get('payload', {}).get('document_id')
            
            print(f"📋 Processing job: {job_id}")
            print(f"📦 Job data keys: {list(job_data.keys())}")
            
            # Validate job payload
            validate_job_payload(job_data)
            
            # Update document status to Processing
            if document_id:
                self.patient_manager.update_document_status(document_id, 'Processing')
            
            # Extract text from document
            try:
                # Pass full job_data, not just payload - extract_text_from_job expects the full structure
                text_result = extract_text_from_job(job_data)
                if text_result is None:
                    print(f"⚠️ No text extraction result - skipping")
                    channel.basic_ack(delivery_tag=method.delivery_tag)
                    return
                
                # Extract text from segments - text_result has structure: {segments: [{text: "..."}, ...]}
                segments = text_result.get('segments', [])
                extracted_text = ' '.join(seg.get('text', '') for seg in segments)
                print(f"📄 Text extraction completed: {len(segments)} segments, {len(extracted_text)} characters")
                
                # Extract patient demographics from text
                print(f"🔍 Extracting patient demographics...")
                demographics = extract_patient_from_text(extracted_text)
                
                # Run full entity extraction pipeline
                print(f"🧠 Running entity extraction pipeline...")
                entities_result = self._run_entity_extraction(job_data, text_result)
                
                if demographics.get('mrn') or demographics.get('name'):
                    print(f"✅ Patient demographics extracted: MRN={demographics.get('mrn')}, Name={demographics.get('name')}")
                    
                    # Create or find patient in database (even if validation has minor issues)
                    patient_id = self.patient_manager.find_or_create_patient(demographics)
                    
                    if patient_id and document_id:
                        # Link document to patient
                        self.patient_manager.link_document_to_patient(document_id, patient_id)
                        print(f"🔗 Document {document_id} linked to patient {patient_id}")
                        
                        # Store extracted entities in database
                        if entities_result:
                            entities = entities_result.get('extracted_entities', [])
                            if entities:
                                entity_count = self._store_extracted_entities(str(patient_id), document_id, entities)
                                print(f"💾 Stored {entity_count} extracted entities in database")
                            else:
                                print(f"⚠️ No entities extracted from document {document_id}")
                        else:
                            print(f"⚠️ Entity extraction returned None for document {document_id}")
                        
                        # Update document status to Completed
                        self.patient_manager.update_document_status(document_id, 'Completed')
                        entities_count = len(entities_result.get('extracted_entities', [])) if entities_result else 0
                        validation_status = "valid" if demographics.get('is_valid') else "partial"
                        result = {"status": "completed", "patient_id": str(patient_id), "entities_extracted": entities_count, "validation_status": validation_status}
                    else:
                        print(f"⚠️ Failed to create/find patient")
                        if document_id:
                            self.patient_manager.update_document_status(document_id, 'Failed')
                        result = {"status": "failed", "error": "Patient creation failed"}
                else:
                    print(f"⚠️ No patient identifiers found (MRN or name missing)")
                    
                    # Create a generic patient record for unlinked documents
                    generic_demographics = {
                        'mrn': f"DOC-{document_id[:8].upper()}",
                        'name': "Unknown Patient",
                        'is_valid': True,
                        'validation_errors': ["Generic patient created from document"]
                    }
                    
                    patient_id = self.patient_manager.find_or_create_patient(generic_demographics)
                    
                    if patient_id and document_id:
                        self.patient_manager.link_document_to_patient(document_id, patient_id)
                        print(f"🔗 Document {document_id} linked to generic patient {patient_id}")
                        
                        # Store extracted entities if available
                        if entities_result:
                            entities = entities_result.get('extracted_entities', [])
                            if entities:
                                entity_count = self._store_extracted_entities(str(patient_id), document_id, entities)
                                print(f"💾 Stored {entity_count} extracted entities in database")
                        
                        result = {"status": "completed", "patient_id": str(patient_id), "entities_extracted": len(entities_result.get('extracted_entities', [])) if entities_result else 0, "validation_status": "generic"}
                    else:
                        print(f"⚠️ Failed to create generic patient")
                        if document_id:
                            self.patient_manager.update_document_status(document_id, 'Failed')
                        result = {"status": "failed", "error": "Generic patient creation failed"}
                    
                    if document_id:
                        self.patient_manager.update_document_status(document_id, 'Completed')
                    
            except Exception as e:
                print(f"⚠️ Processing failed: {e}")
                if document_id:
                    self.patient_manager.update_document_status(document_id, 'Failed')
                result = {"status": "failed", "error": str(e)}
            
            print(f"✅ Job completed: {job_id}")
            
            # Acknowledge message
            channel.basic_ack(delivery_tag=method.delivery_tag)
            
        except json.JSONDecodeError as e:
            print(f"❌ Invalid JSON in job: {e}")
            channel.basic_nack(delivery_tag=method.delivery_tag, requeue=False)
            
        except Exception as e:
            print(f"❌ Error processing job: {e}")
            # Negative acknowledgment, but requeue for retry
            channel.basic_nack(delivery_tag=method.delivery_tag, requeue=True)
    
    def _run_entity_extraction(self, job_data: Dict[str, Any], text_result: Dict[str, Any]) -> Optional[Dict[str, Any]]:
        """Run the complete entity extraction pipeline"""
        try:
            # Create chunks from text segments
            chunks = []
            for i, segment in enumerate(text_result.get('segments', [])):
                chunk = ChunkWithProvenance(
                    text=segment.get('text', ''),
                    document_id=job_data.get('payload', {}).get('document_id', ''),
                    page=segment.get('page'),
                    section=segment.get('section'),
                    rank=i
                )
                chunks.append(chunk)
            
            if not chunks:
                print("⚠️ No chunks available for entity extraction")
                return None
            
            # Run entity extraction pipeline
            extraction_result = run_entity_extraction_pipeline(
                job_payload=job_data,
                max_retries=3
            )
            
            if extraction_result:
                print(f"✅ Entity extraction completed: {len(extraction_result.get('extracted_entities', []))} entities")
                return extraction_result
            else:
                print("⚠️ Entity extraction returned no results")
                return None
                
        except Exception as e:
            print(f"⚠️ Entity extraction failed: {e}")
            return None
    
    def _store_extracted_entities(self, patient_id: str, document_id: str, entities: list) -> int:
        """Store extracted entities in the database via API"""
        try:
            import requests
            
            # Category mapping from worker format to frontend display format
            CATEGORY_MAPPING = {
                'patient_demographics': 'Patient Demographics',
                'allergies': 'Allergies',
                'medications': 'Medications',
                'diagnoses': 'Diagnoses',
                'procedures': 'Procedures',
                'lab_results': 'Lab Results',
                'vital_signs': 'Vital Signs',
                'social_history': 'Social History',
                'clinical_notes': 'Clinical Notes',
                'document_metadata': 'Document Metadata'
            }
            
            # Prepare entities for API with mapped categories
            entity_dtos = []
            for entity in entities:
                # Map category from worker format to frontend format
                original_category = entity.get('entity_group_name', '')
                mapped_category = CATEGORY_MAPPING.get(original_category, original_category.title())
                
                dto = {
                    'entityGroupName': original_category,  # Keep original for storage
                    'entityName': entity.get('entity_name', ''),
                    'entityValue': entity.get('entity_value', ''),
                    'rationale': entity.get('rationale'),
                    'sourceText': entity.get('source_text'),
                    'confidence': entity.get('confidence'),
                    'documentLocation': entity.get('document_location', {}),
                    'mappedCategory': mapped_category  # Add mapped category for frontend
                }
                entity_dtos.append(dto)
            
            # Call backend API to store entities with authentication
            api_url = f"http://localhost:5000/api/v1/documents/{document_id}/entities"
            headers = {
                'Content-Type': 'application/json',
                'X-API-Key': self.config.worker_api_key  # Add API key for authentication
            }
            
            print(f"🔑 Sending API request with key: {self.config.worker_api_key}")
            print(f"🌐 API URL: {api_url}")
            
            response = requests.post(api_url, json={
                'patientId': patient_id,
                'documentId': document_id,
                'entities': entity_dtos
            }, headers=headers)
            
            if response.status_code == 200:
                return len(entity_dtos)
            else:
                print(f"⚠️ Failed to store entities: {response.status_code} - {response.text}")
                return 0
                
        except Exception as e:
            print(f"⚠️ Error storing entities: {e}")
            return 0
    
    def start_consuming(self):
        """Start consuming messages from RabbitMQ"""
        try:
            # Set up consumer
            self.channel.basic_consume(
                queue=self.queue_name,
                on_message_callback=self.process_job,
                auto_ack=False
            )
            
            print("🚀 Worker service started - waiting for jobs...")
            print("Press Ctrl+C to stop")
            
            # Start consuming (blocking)
            self.running = True
            while self.running:
                try:
                    self.connection.process_data_events(time_limit=1)
                except KeyboardInterrupt:
                    break
                except Exception as e:
                    print(f"⚠️ Connection error: {e}")
                    time.sleep(5)  # Wait before retrying
                    if not self.connect_to_rabbitmq():
                        print("❌ Failed to reconnect, exiting...")
                        break
                    
        except Exception as e:
            print(f"❌ Error in consumer: {e}")
        finally:
            self.cleanup()
    
    def cleanup(self):
        """Clean up resources"""
        print("🧹 Cleaning up...")
        self.running = False
        
        if self.channel and not self.channel.is_closed:
            self.channel.close()
            
        if self.connection and not self.connection.is_closed:
            self.connection.close()
            
        print("✅ Cleanup complete")
    
    def stop(self, signum=None, frame=None):
        """Handle shutdown signals"""
        print(f"\n🛑 Received signal {signum}, shutting down...")
        self.running = False


def main():
    """Main entry point"""
    print("🏥 Clinical Intelligence Worker Service")
    print("=" * 50)
    
    # Load configuration
    try:
        load_config()
        print("✅ Configuration loaded")
    except Exception as e:
        print(f"❌ Failed to load configuration: {e}")
        return 1
    
    # Create and start worker service
    worker = WorkerService()
    
    # Set up signal handlers for graceful shutdown
    signal.signal(signal.SIGINT, worker.stop)
    signal.signal(signal.SIGTERM, worker.stop)
    
    # Connect to RabbitMQ
    if not worker.connect_to_rabbitmq():
        print("❌ Failed to start worker service")
        return 1
    
    # Start processing jobs
    try:
        worker.start_consuming()
    except KeyboardInterrupt:
        print("\n👋 Shutting down worker service...")
    except Exception as e:
        print(f"❌ Fatal error: {e}")
        return 1
    
    return 0


if __name__ == "__main__":
    sys.exit(main())
