#!/usr/bin/env python3
"""
Manually publish a correctly formatted job to RabbitMQ to test worker processing
"""
import pika
import json
import uuid

def publish_test_job():
    """Publish a test job with correct schema"""
    
    # Create connection
    connection = pika.BlockingConnection(
        pika.ConnectionParameters('localhost')
    )
    channel = connection.channel()
    
    # Declare queue (should already exist from worker)
    channel.queue_declare(
        queue='document_processing_jobs',
        durable=True,
        arguments={'x-message-ttl': 3600000}
    )
    
    # Create job with correct schema
    job = {
        "schema_version": "1.0",
        "job_id": str(uuid.uuid4()),
        "document_id": "cec749a8-ab88-45db-a213-28791ee97cc9",  # Use a real document ID from recent upload
        "status": "pending",
        "payload": {
            "storage_path": "C:\\Users\\HadiyaAmber\\Desktop\\Clinical-Intelligence\\Server\\ClinicalIntelligence.Api\\storage\\documents\\pending\\cec749a8-ab88-45db-a213-28791ee97cc9\\original.pdf",
            "mime_type": "application/pdf",
            "patient_id": None,
            "document_id": "cec749a8-ab88-45db-a213-28791ee97cc9"
        }
    }
    
    # Serialize to JSON
    message_body = json.dumps(job)
    
    print("=" * 80)
    print("📤 Publishing Test Job to RabbitMQ")
    print("=" * 80)
    print(f"\nJob Schema:")
    print(json.dumps(job, indent=2))
    print(f"\nMessage Size: {len(message_body)} bytes")
    
    # Publish message
    channel.basic_publish(
        exchange='',
        routing_key='document_processing_jobs',
        body=message_body,
        properties=pika.BasicProperties(
            delivery_mode=2,  # Make message persistent
            content_type='application/json'
        )
    )
    
    print("\n✅ Job published successfully!")
    print(f"Job ID: {job['job_id']}")
    print(f"Document ID: {job['document_id']}")
    print("\n" + "=" * 80)
    print("🔍 Check worker logs to see if job is processed correctly")
    print("=" * 80)
    
    connection.close()

if __name__ == "__main__":
    try:
        publish_test_job()
    except Exception as e:
        print(f"❌ Error: {e}")
        import traceback
        traceback.print_exc()
