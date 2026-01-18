#!/usr/bin/env python3
"""
Test RabbitMQ connection and check queue status
"""
import pika
import json

def test_connection():
    """Test RabbitMQ connection and check queue"""
    print("=" * 80)
    print("🔍 Testing RabbitMQ Connection")
    print("=" * 80)
    
    try:
        # Connect to RabbitMQ
        connection = pika.BlockingConnection(
            pika.ConnectionParameters('localhost')
        )
        channel = connection.channel()
        print("✅ Connected to RabbitMQ")
        
        # Check queue status
        queue_name = 'document_processing_jobs'
        result = channel.queue_declare(
            queue=queue_name,
            durable=True,
            passive=True  # Don't create, just check
        )
        
        message_count = result.method.message_count
        consumer_count = result.method.consumer_count
        
        print(f"\n📊 Queue Status: {queue_name}")
        print(f"   Messages: {message_count}")
        print(f"   Consumers: {consumer_count}")
        
        # Get a message without acknowledging (peek)
        if message_count > 0:
            method, properties, body = channel.basic_get(queue_name, auto_ack=False)
            if method:
                print(f"\n📨 First message in queue:")
                try:
                    msg = json.loads(body.decode('utf-8'))
                    print(f"   Keys: {list(msg.keys())}")
                    if 'schema_version' in msg:
                        print(f"   ✅ Correct format (has schema_version)")
                    else:
                        print(f"   ❌ Wrong format (missing schema_version)")
                    print(f"   Full message: {json.dumps(msg, indent=2)[:500]}...")
                except:
                    print(f"   Raw: {body[:200]}...")
                
                # Reject the message back to queue
                channel.basic_nack(method.delivery_tag, requeue=True)
        else:
            print("\n⚠️ Queue is empty - no messages")
        
        connection.close()
        print("\n✅ Connection test complete")
        
    except Exception as e:
        print(f"❌ Error: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    test_connection()
