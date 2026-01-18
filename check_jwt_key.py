import base64
import hashlib
import hmac

# The JWT key from .env
jwt_key = "beeceb79055d84704f3ea9b0b85be8719567868c337ee9a907abf1fc66751277"

print(f"JWT Key: {jwt_key}")
print(f"Key Length: {len(jwt_key)}")
print(f"Key Bytes: {len(jwt_key.encode('utf-8'))}")

# Test if this is a valid key for HMAC-SHA256
message = "test message"
signature = hmac.new(jwt_key.encode('utf-8'), message.encode('utf-8'), hashlib.sha256).hexdigest()
print(f"HMAC-SHA256 Test: {signature[:16]}...")

# Check if it's base64 encoded
try:
    decoded = base64.b64decode(jwt_key)
    print(f"Base64 decoded length: {len(decoded)}")
    print(f"Base64 decoded: {decoded[:32]}...")
except Exception as e:
    print(f"Not base64 encoded: {e}")
