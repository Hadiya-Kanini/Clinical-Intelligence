#!/usr/bin/env python3
"""
Test the Gemini API connection and basic functionality.
"""

import os
import sys
sys.path.append(os.path.join(os.path.dirname(__file__), 'worker'))

from worker.config import load_config
from worker.entity_extraction.gemini_client import GeminiClient

def test_gemini_api():
    """Test the Gemini API connection."""
    
    print("🧪 Testing Gemini API Connection")
    print("=" * 40)
    
    try:
        # Load configuration
        config = load_config()
        print(f"✅ Config loaded")
        print(f"🔑 API Key: {config.gemini_api_key[:20]}...")
        print(f"🤖 Model: {config.extraction_model}")
        print(f"⏱️ Timeout: {config.extraction_timeout}s")
        print(f"🔄 Max Retries: {config.extraction_max_retries}")
        
        # Initialize client
        client = GeminiClient(
            api_key=config.gemini_api_key,
            model=config.extraction_model,
            timeout=config.extraction_timeout,
            max_retries=config.extraction_max_retries
        )
        print(f"✅ GeminiClient initialized")
        
        # Test simple generation
        print("\n🧠 Testing simple generation...")
        test_prompt = "Extract clinical entities from this text: 'Patient John Doe has hypertension and takes lisinopril 10mg daily.'"
        
        system_instruction = """You are a clinical entity extraction expert. Extract medical entities and return them in JSON format.
        
        Return only valid JSON with this structure:
        {
          "schema_version": "1.0",
          "extracted_entities": [
            {
              "entity_group_name": "category_name",
              "entity_name": "entity_name", 
              "entity_value": "value",
              "rationale": "explanation",
              "confidence": 0.95
            }
          ]
        }"""
        
        response = client.generate_content(test_prompt, system_instruction)
        print(f"✅ Generation successful")
        print(f"📄 Response length: {len(response)} chars")
        print(f"📝 Response preview: {response[:200]}...")
        
        return True
        
    except Exception as e:
        print(f"❌ Error: {e}")
        return False

if __name__ == "__main__":
    success = test_gemini_api()
    if success:
        print("\n🎉 Gemini API test PASSED!")
    else:
        print("\n💥 Gemini API test FAILED!")
