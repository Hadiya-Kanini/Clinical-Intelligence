 
import pika 
 
try: 
    # Connect to RabbitMQ 
    connection = pika.BlockingConnection(pika.ConnectionParameters('localhost')) 
    channel = connection.channel() 
    print("Successfully connected to RabbitMQ!") 
    connection.close() 
except Exception as e: 
    print(f"Error connecting to RabbitMQ: {e}") 
