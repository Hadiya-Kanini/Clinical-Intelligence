import requests
import json

# Test the 360 view API to see what's actually being returned
try:
    # Get a patient ID first
    response = requests.get('http://localhost:8000/api/v1/patients')
    if response.status_code == 200:
        patients = response.json()
        if patients and len(patients) > 0:
            patient_id = patients[0]['id']
            print(f'Testing with patient ID: {patient_id}')
            
            # Test the 360 view endpoint
            response = requests.get(f'http://localhost:8000/api/v1/patients/{patient_id}/360')
            if response.status_code == 200:
                data = response.json()
                print(f'Patient: {data.get("patient", {})}')
                print(f'Number of entities: {len(data.get("entities", []))}')
                
                # Group entities by category to see what we have
                categories = {}
                for entity in data.get('entities', []):
                    cat = entity.get('category', 'unknown')
                    if cat not in categories:
                        categories[cat] = []
                    categories[cat].append(entity)
                
                print('\nCategories found:')
                for cat, entities in categories.items():
                    print(f'  {cat}: {len(entities)} entities')
                    for e in entities[:2]:  # Show first 2 entities per category
                        print(f'    - {e.get("name", "unknown")}: {e.get("value", "unknown")}')
                    if len(entities) > 2:
                        print(f'    ... and {len(entities) - 2} more')
            else:
                print(f'360 view API failed: {response.status_code} - {response.text}')
        else:
            print('No patients found')
    else:
        print(f'Failed to get patients: {response.status_code} - {response.text}')
except Exception as e:
    print(f'Error: {e}')
