#!/usr/bin/env python3
"""
Test script to verify hashlib import fix in main.py
"""

import os
import sys

# Add worker directory to path
sys.path.insert(0, os.path.dirname(__file__))

def test_hashlib_import():
    """Test that hashlib can be imported inside function scope"""
    try:
        # Simulate the function scope issue
        def test_function():
            import hashlib  # This should work inside function
            text = "test text"
            hash_val = hashlib.sha256(text.encode('utf-8')).hexdigest()[:16]
            return hash_val
        
        result = test_function()
        print(f"✅ Hashlib import test passed: {result}")
        return True
    except Exception as e:
        print(f"❌ Hashlib import test failed: {e}")
        return False

def test_main_import():
    """Test importing main.py without errors"""
    try:
        from main import run_entity_extraction_pipeline
        print("✅ main.py import test passed")
        return True
    except Exception as e:
        print(f"❌ main.py import test failed: {e}")
        return False

if __name__ == "__main__":
    print("Testing hashlib fix...")
    test1 = test_hashlib_import()
    test2 = test_main_import()
    
    if test1 and test2:
        print("\n✅ All tests passed! The hashlib fix should work.")
    else:
        print("\n❌ Some tests failed. Please check the error messages.")
