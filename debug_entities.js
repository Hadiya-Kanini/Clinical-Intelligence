// Debug script to check entity display issues in Patient 360 view
// Run this in browser console when on the Patient 360 page

async function debugEntityDisplay() {
  console.log('🔍 Debugging Patient 360 entity display...');
  
  try {
    // Get current patient ID from URL
    const urlParams = new URLSearchParams(window.location.search);
    let patientId = urlParams.get('patientId');
    
    if (!patientId) {
      // Try to extract from URL path
      const pathParts = window.location.pathname.split('/');
      const patientIndex = pathParts.indexOf('patient');
      if (patientIndex !== -1 && pathParts[patientIndex + 1]) {
        patientId = pathParts[patientIndex + 1];
      }
    }
    
    if (!patientId) {
      console.error('❌ No patient ID found');
      return;
    }
    
    console.log(`📋 Testing with patient ID: ${patientId}`);
    
    // Test the entities/360-view endpoint (used by Patient360View component)
    console.log('\n🔍 Testing /api/v1/entities/360-view endpoint...');
    const entitiesResponse = await fetch(`/api/v1/entities/360-view?patientId=${patientId}`);
    
    if (!entitiesResponse.ok) {
      console.error(`❌ Entities endpoint failed: ${entitiesResponse.status}`);
      const text = await entitiesResponse.text();
      console.error('Response:', text);
      return;
    }
    
    const entitiesData = await entitiesResponse.json();
    console.log('✅ Entities endpoint response:', entitiesData);
    
    if (!entitiesData.entities || !Array.isArray(entitiesData.entities)) {
      console.error('❌ No entities array in response');
      return;
    }
    
    console.log(`📊 Found ${entitiesData.entities.length} entities`);
    
    // Group entities by category to see what we have
    const categories = {};
    entitiesData.entities.forEach(entity => {
      const cat = entity.category || 'unknown';
      if (!categories[cat]) {
        categories[cat] = [];
      }
      categories[cat].push(entity);
    });
    
    console.log('\n📂 Entities by category:');
    Object.entries(categories).forEach(([category, entityList]) => {
      console.log(`  ${category}: ${entityList.length} entities`);
      entityList.slice(0, 3).forEach(entity => {
        console.log(`    - ${entity.name}: ${entity.value || 'No value'}`);
      });
      if (entityList.length > 3) {
        console.log(`    ... and ${entityList.length - 3} more`);
      }
    });
    
    // Check entity structure
    if (entitiesData.entities.length > 0) {
      console.log('\n🔍 Sample entity structure:');
      const sampleEntity = entitiesData.entities[0];
      console.log('Sample entity:', sampleEntity);
      
      // Check for required fields
      const requiredFields = ['id', 'category', 'name', 'value'];
      const missingFields = requiredFields.filter(field => !(field in sampleEntity));
      
      if (missingFields.length > 0) {
        console.error(`❌ Missing required fields: ${missingFields.join(', ')}`);
      } else {
        console.log('✅ Entity structure looks correct');
      }
      
      // Check citations
      if (sampleEntity.citations && Array.isArray(sampleEntity.citations)) {
        console.log(`✅ Entity has ${sampleEntity.citations.length} citations`);
        if (sampleEntity.citations.length > 0) {
          console.log('Sample citation:', sampleEntity.citations[0]);
        }
      } else {
        console.log('⚠️  Entity has no citations');
      }
    }
    
    // Test the main 360 endpoint (used by Patient360Page)
    console.log('\n🔍 Testing /api/v1/patients/{id}/360 endpoint...');
    const patient360Response = await fetch(`/api/v1/patients/${patientId}/360`);
    
    if (!patient360Response.ok) {
      console.error(`❌ Patient 360 endpoint failed: ${patient360Response.status}`);
      return;
    }
    
    const patient360Data = await patient360Response.json();
    console.log('✅ Patient 360 endpoint response:', patient360Data);
    
    // Compare the two responses
    console.log('\n🔍 Comparing endpoints:');
    console.log(`Entities endpoint: ${entitiesData.entities?.length || 0} entities`);
    console.log(`Patient 360 endpoint: ${patient360Data.entities?.length || 0} entities`);
    
    if (entitiesData.entities?.length !== patient360Data.entities?.length) {
      console.warn('⚠️  Different entity counts between endpoints');
    }
    
    console.log('\n🎯 Frontend should display:');
    console.log(`- ${Object.keys(categories).length} categories`);
    console.log(`- ${entitiesData.entities.length} total entities`);
    console.log('- Categories:', Object.keys(categories).join(', '));
    
    // Check if the page is actually showing the entities
    setTimeout(() => {
      console.log('\n🔍 Checking what\'s actually displayed on the page...');
      
      // Look for entity cards
      const entityCards = document.querySelectorAll('[data-testid*="entity"], .entity-card, [class*="entity"]');
      console.log(`Found ${entityCards.length} potential entity elements`);
      
      // Look for category sections
      const categorySections = document.querySelectorAll('[class*="category"], [data-testid*="category"]');
      console.log(`Found ${categorySections.length} potential category sections`);
      
      // Look for "No entities found" message
      const noEntitiesMsg = Array.from(document.querySelectorAll('*')).find(el => 
        el.textContent?.includes('No entities found') || el.textContent?.includes('No entities')
      );
      
      if (noEntitiesMsg) {
        console.log('⚠️  Page shows "No entities found" message');
        console.log('Message element:', noEntitiesMsg);
      } else {
        console.log('✅ No "No entities found" message detected');
      }
      
    }, 2000);
    
  } catch (error) {
    console.error('❌ Debug script failed:', error);
  }
}

// Auto-run if on a patient page
if (window.location.pathname.includes('/patient')) {
  console.log('🚀 Detected patient page, running debug...');
  debugEntityDisplay();
} else {
  console.log('💡 Run debugEntityDisplay() in browser console to debug entity display');
}
