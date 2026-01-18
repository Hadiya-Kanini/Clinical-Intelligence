#!/usr/bin/env python3
"""
Test script to verify document upload functionality
"""
import requests
import json
import os

# Configuration
BASE_URL = "http://localhost:5000"
LOGIN_URL = f"{BASE_URL}/api/v1/auth/login"
UPLOAD_URL = f"{BASE_URL}/api/v1/documents/upload"
CSRF_URL = f"{BASE_URL}/api/v1/auth/csrf"

def test_login():
    """Test login functionality"""
    print("Testing login...")
    
    # Get CSRF token first
    try:
        csrf_response = requests.get(CSRF_URL, allow_redirects=True)
        print(f"CSRF Response Status: {csrf_response.status_code}")
    except Exception as e:
        print(f"CSRF request failed: {e}")
        return None
    
    # Login credentials
    login_data = {
        "email": "test@example.com",
        "password": "Test123456"
    }
    
    try:
        session = requests.Session()
        login_response = session.post(LOGIN_URL, json=login_data, allow_redirects=True)
        print(f"Login Response Status: {login_response.status_code}")
        print(f"Login Response: {login_response.text[:200]}...")
        
        if login_response.status_code == 200:
            print("Login successful")
            
            # Extract JWT token and add to session headers
            login_data = login_response.json()
            if 'access_token' in login_data:
                token = login_data['access_token']
                session.headers.update({'Authorization': f'Bearer {token}'})
                print("JWT token added to session")
            
            return session
        else:
            print("Login failed")
            return None
    except Exception as e:
        print(f"Login request failed: {e}")
        return None

def test_upload(session):
    """Test file upload functionality"""
    print("\nTesting file upload...")
    
    # Use an existing PDF file if available, otherwise create a simple PDF-like content
    test_files = [
        "test-document.pdf",
        "test-document.txt"  # fallback
    ]
    
    test_file_path = None
    test_content = None
    content_type = None
    
    # Try to find an existing test file first
    for filename in test_files:
        if os.path.exists(filename):
            test_file_path = filename
            if filename.endswith('.pdf'):
                content_type = 'application/pdf'
            else:
                content_type = 'text/plain'
            break
    
    # If no existing file, create a simple test file with PDF extension
    if test_file_path is None:
        test_file_path = "test-document.pdf"
        # Create a minimal PDF-like content (just for testing upload validation)
        test_content = b"%PDF-1.4\n1 0 obj\n<<\n/Type /Catalog\n/Pages 2 0 R\n>>\nendobj\n2 0 obj\n<<\n/Type /Pages\n/Kids [3 0 R]\n/Count 1\n>>\nendobj\n3 0 obj\n<<\n/Type /Page\n/Parent 2 0 R\n/MediaBox [0 0 612 792]\n>>\nendobj\nxref\n0 4\n0000000000 65535 f\n0000000009 00000 n\n0000000058 00000 n\n0000000115 00000 n\ntrailer\n<<\n/Size 4\n/Root 1 0 R\n>>\nstartxref\n179\n%%EOF"
        content_type = 'application/pdf'
        
        try:
            with open(test_file_path, 'wb') as f:
                f.write(test_content)
        except Exception as e:
            print(f"Failed to create test file: {e}")
            return False
    
    try:
        # Prepare file for upload
        with open(test_file_path, 'rb') as f:
            files = {'file': (test_file_path, f, content_type)}
            data = {
                'description': 'Test document upload',
                'patientId': '00000000-0000-0000-0000-000000000001'  # Use a valid GUID format
            }
            
            try:
                upload_response = session.post(UPLOAD_URL, files=files, data=data, allow_redirects=True)
                print(f"Upload Response Status: {upload_response.status_code}")
                print(f"Upload Response: {upload_response.text[:500]}...")
                
                if upload_response.status_code in [200, 201, 202]:
                    print("✅ Upload successful")
                    return True
                else:
                    print("❌ Upload failed")
                    return False
            except Exception as e:
                print(f"Upload request failed: {e}")
                return False
    except Exception as e:
        print(f"Failed to read test file: {e}")
        return False
    finally:
        # Clean up test file if we created it
        if test_content and os.path.exists(test_file_path):
            try:
                os.remove(test_file_path)
            except:
                pass

def main():
    """Main test function"""
    print("Testing Document Upload Functionality")
    print("=" * 50)
    
    # Test login first
    session = test_login()
    if not session:
        print("Cannot proceed with upload test - login failed")
        return
    
    # Test upload
    upload_success = test_upload(session)
    
    print("\n" + "=" * 50)
    if upload_success:
        print("All tests passed! Upload functionality is working.")
    else:
        print("Upload test failed. Check the logs above.")

if __name__ == "__main__":
    main()
