#!/usr/bin/env python3
"""
Purge all messages from the RabbitMQ queue to start fresh
"""
import pika

def purge_queue():
    """Purge the document processing queue"""
    
    # Create connection
    connection = pika.BlockingConnection(
        pika.ConnectionParameters('localhost')
    )
    channel = connection.channel()
    
    # Purge queue
    queue_name = 'document_processing_jobs'
    result = channel.queue_purge(queue_name)
    
    print("=" * 80)
    print("🗑️  Purging RabbitMQ Queue")
    print("=" * 80)
    print(f"\nQueue: {queue_name}")
    print(f"Messages purged: {result.method.message_count}")
    print("\n✅ Queue purged successfully!")
    print("=" * 80)
    
    connection.close()

if __name__ == "__main__":
    try:
        purge_queue()
    except Exception as e:
        print(f"❌ Error: {e}")
        import traceback
        traceback.print_exc()
