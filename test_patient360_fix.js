// Test script to verify the Patient 360 data structure fix
// Run this in browser console when the app is running

async function testPatient360Fix() {
  console.log('🔍 Testing Patient 360 data structure fix...');
  
  try {
    // Get current patient ID from URL or use first available
    const urlParams = new URLSearchParams(window.location.search);
    let patientId = urlParams.get('patientId');
    
    if (!patientId) {
      // Try to get first patient from API
      const patientsResponse = await fetch('/api/v1/patients');
      if (patientsResponse.ok) {
        const patients = await patientsResponse.json();
        if (patients.length > 0) {
          patientId = patients[0].id;
          console.log(`Using first patient: ${patientId}`);
        }
      }
    }
    
    if (!patientId) {
      console.error('❌ No patient ID found for testing');
      return false;
    }
    
    // Test the 360 endpoint
    console.log(`Testing /api/v1/patients/${patientId}/360...`);
    const response = await fetch(`/api/v1/patients/${patientId}/360`);
    
    if (!response.ok) {
      console.error(`❌ API call failed: ${response.status} ${response.statusText}`);
      return false;
    }
    
    const data = await response.json();
    console.log('✅ API response received');
    console.log('Response structure:', data);
    
    // Test expected structure
    const requiredFields = ['patientId', 'mrn', 'name', 'dob', 'address', 'contact', 'entities', 'documents', 'generatedAt'];
    const missingFields = requiredFields.filter(field => !(field in data));
    
    if (missingFields.length > 0) {
      console.error(`❌ Missing required fields: ${missingFields.join(', ')}`);
      return false;
    }
    
    console.log('✅ All required fields present');
    
    // Test patient data access (this was causing the error)
    console.log('Testing patient data access...');
    try {
      const patientInfo = {
        mrn: data.mrn,
        name: data.name,
        dob: data.dob,
        address: data.address,
        contact: data.contact
      };
      console.log('✅ Patient data accessible:', patientInfo);
    } catch (error) {
      console.error('❌ Error accessing patient data:', error);
      return false;
    }
    
    // Test entities structure
    console.log('Testing entities structure...');
    if (data.entities && Array.isArray(data.entities)) {
      console.log(`✅ Found ${data.entities.length} entities`);
      
      // Check first entity structure
      if (data.entities.length > 0) {
        const entity = data.entities[0];
        console.log('Sample entity:', entity);
        
        const entityFields = ['id', 'category', 'name', 'value', 'citations'];
        const missingEntityFields = entityFields.filter(field => !(field in entity));
        
        if (missingEntityFields.length > 0) {
          console.warn(`⚠️ Entity missing fields: ${missingEntityFields.join(', ')}`);
        } else {
          console.log('✅ Entity structure looks good');
        }
      }
    } else {
      console.warn('⚠️ No entities found or entities is not an array');
    }
    
    // Test documents structure
    console.log('Testing documents structure...');
    if (data.documents && Array.isArray(data.documents)) {
      console.log(`✅ Found ${data.documents.length} documents`);
      
      if (data.documents.length > 0) {
        const document = data.documents[0];
        console.log('Sample document:', document);
        
        const docFields = ['id', 'originalName', 'status', 'uploadedAt', 'groundedEntityCount'];
        const missingDocFields = docFields.filter(field => !(field in document));
        
        if (missingDocFields.length > 0) {
          console.warn(`⚠️ Document missing fields: ${missingDocFields.join(', ')}`);
        } else {
          console.log('✅ Document structure looks good');
        }
      }
    } else {
      console.warn('⚠️ No documents found or documents is not an array');
    }
    
    console.log('🎉 Patient 360 data structure test PASSED!');
    console.log('The frontend should now be able to access patient data without errors.');
    
    return true;
    
  } catch (error) {
    console.error('❌ Test failed with error:', error);
    return false;
  }
}

// Auto-run test if on Patient 360 page
if (window.location.pathname.includes('/patient/')) {
  console.log('🚀 Detected Patient 360 page, running test...');
  testPatient360Fix();
} else {
  console.log('💡 Run testPatient360Fix() in browser console to test the fix');
}
