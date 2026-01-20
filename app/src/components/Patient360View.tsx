import { useState, useEffect } from 'react';
import Card from '../components/ui/Card';
import Badge from '../components/ui/Badge';
import Tooltip from '../components/ui/Tooltip';
import DataStatusBadge from '../components/ui/DataStatusBadge';
import { Loader2, User, FileText, ExternalLink, Info } from 'lucide-react';
import { getPatient360, type Patient360Response, type ExtractedEntity, type EntityCitation } from '../lib/patientApi';

interface GroupedEntities {
  [category: string]: ExtractedEntity[];
}

interface Patient360ViewProps {
  patientId: string;
  onCitationClick?: (citation: EntityCitation) => void;
}

export default function Patient360View({ patientId, onCitationClick }: Patient360ViewProps) {
  const [entities, setEntities] = useState<GroupedEntities>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [patientData, setPatientData] = useState<Patient360Response | null>(null);

  useEffect(() => {
    fetchPatientData();
  }, [patientId]);

  const fetchPatientData = async () => {
    if (!patientId) {
      setError('Patient ID is required');
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      setError(null);
      
      const result = await getPatient360(patientId);
      
      if (result.success) {
        const data = result.data;
        setPatientData(data);
        
        // Group entities by category
        const grouped: GroupedEntities = {};
        data.entities.forEach((entity) => {
          if (!grouped[entity.category]) {
            grouped[entity.category] = [];
          }
          grouped[entity.category].push(entity);
        });
        
        setEntities(grouped);
      } else {
        setError(result.error.message);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '2rem' }}>
        <Loader2 style={{ width: '2rem', height: '2rem', animation: 'spin 1s linear infinite' }} />
        <span style={{ marginLeft: '0.5rem' }}>Loading 360° view...</span>
      </div>
    );
  }

  if (error) {
    return (
      <Card style={{ margin: '1rem' }}>
        <div style={{ padding: '1.5rem', textAlign: 'center', color: '#dc2626' }}>
          <p>Error loading 360° view: {error}</p>
          <button 
            onClick={fetchPatientData}
            style={{
              marginTop: '0.5rem',
              padding: '0.5rem 1rem',
              backgroundColor: '#3b82f6',
              color: 'white',
              border: 'none',
              borderRadius: '0.375rem',
              cursor: 'pointer'
            }}
          >
            Retry
          </button>
        </div>
      </Card>
    );
  }

  return (
    <div style={{ padding: '1.5rem', display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      {/* Header */}
      <Card>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <User style={{ width: '1.25rem', height: '1.25rem' }} />
          <h2 style={{ margin: 0, fontSize: '1.25rem', fontWeight: 'bold' }}>Patient 360° View</h2>
        </div>
        <div style={{ padding: '1rem 0 0 0' }}>
          {patientData ? (
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '1rem' }}>
              <div>
                <p style={{ fontSize: '0.875rem', color: '#6b7280', margin: '0 0 0.25rem 0' }}>Patient Name</p>
                <p style={{ margin: 0, fontWeight: 'medium' }}>{patientData.name || 'N/A'}</p>
              </div>
              <div>
                <p style={{ fontSize: '0.875rem', color: '#6b7280', margin: '0 0 0.25rem 0' }}>MRN</p>
                <p style={{ margin: 0, fontWeight: 'medium' }}>{patientData.mrn || 'N/A'}</p>
              </div>
              <div>
                <p style={{ fontSize: '0.875rem', color: '#6b7280', margin: '0 0 0.25rem 0' }}>Date of Birth</p>
                <p style={{ margin: 0, fontWeight: 'medium' }}>
                  {patientData.dob ? new Date(patientData.dob).toLocaleDateString() : 'N/A'}
                </p>
              </div>
              <div>
                <p style={{ fontSize: '0.875rem', color: '#6b7280', margin: '0 0 0.25rem 0' }}>Documents</p>
                <p style={{ margin: 0, fontWeight: 'medium' }}>{patientData.documents.length}</p>
              </div>
            </div>
          ) : (
            <p style={{ textAlign: 'center', color: '#6b7280', margin: 0 }}>No patient data available</p>
          )}
        </div>
      </Card>

      {/* Entities by Category */}
      {Object.keys(entities).length > 0 ? (
        Object.entries(entities).map(([category, categoryEntities]) => (
          <Card key={category}>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <Badge variant="neutral">{category}</Badge>
                <span style={{ fontSize: '0.875rem', color: '#6b7280' }}>
                  {categoryEntities.length} items
                </span>
              </div>
              <Badge variant="info" style={{ fontSize: '0.75rem' }}>
                {categoryEntities[0]?.displayCategory || category}
              </Badge>
            </div>
            <div style={{ padding: '1rem 0 0 0' }}>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                {categoryEntities.map((entity) => (
                  <div 
                    key={entity.id} 
                    style={{ 
                      display: 'flex', 
                      alignItems: 'flex-start', 
                      justifyContent: 'space-between',
                      padding: '0.75rem',
                      border: '1px solid #e5e7eb',
                      borderRadius: '0.5rem'
                    }}
                  >
                    <div style={{ flex: 1 }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.25rem' }}>
                        <span style={{ fontWeight: 'medium' }}>{entity.name}</span>
                        <DataStatusBadge status={entity.isVerified ? 'verified' : 'unverified'} />
                        {entity.rationale && (
                          <Tooltip content={entity.rationale} position="top">
                            <button
                              type="button"
                              style={{ 
                                background: 'none', 
                                border: 'none', 
                                padding: 0, 
                                cursor: 'pointer',
                                display: 'flex',
                                alignItems: 'center'
                              }}
                              aria-label="View extraction rationale"
                            >
                              <Info style={{ width: '0.875rem', height: '0.875rem', color: '#6b7280' }} />
                            </button>
                          </Tooltip>
                        )}
                      </div>
                      <p style={{ color: '#6b7280', margin: '0 0 0.25rem 0' }}>{entity.value}</p>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', fontSize: '0.75rem', color: '#6b7280' }}>
                        {entity.confidenceScore && (
                          <span>Confidence: {Math.round(entity.confidenceScore * 100)}%</span>
                        )}
                        {entity.units && <span>Units: {entity.units}</span>}
                        {entity.effectiveAt && (
                          <span>Effective: {new Date(entity.effectiveAt).toLocaleDateString()}</span>
                        )}
                      </div>
                      {entity.citations && entity.citations.length > 0 && (
                        <div style={{ marginTop: '0.5rem', display: 'flex', flexWrap: 'wrap', gap: '0.5rem' }}>
                          {entity.citations.map((citation) => (
                            <button
                              key={citation.id}
                              type="button"
                              onClick={() => onCitationClick?.(citation)}
                              style={{
                                display: 'inline-flex',
                                alignItems: 'center',
                                gap: '0.25rem',
                                padding: '0.25rem 0.5rem',
                                fontSize: '0.75rem',
                                color: '#2563eb',
                                background: '#eff6ff',
                                border: '1px solid #bfdbfe',
                                borderRadius: '0.25rem',
                                cursor: 'pointer',
                                textDecoration: 'none'
                              }}
                              title={citation.sourceText || 'View source'}
                            >
                              <ExternalLink style={{ width: '0.75rem', height: '0.75rem' }} />
                              {citation.documentName || 'Document'}
                              {citation.pageNumber && ` (p.${citation.pageNumber})`}
                            </button>
                          ))}
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </Card>
        ))
      ) : (
        <Card>
          <div style={{ padding: '1.5rem', textAlign: 'center', color: '#6b7280' }}>
            <FileText style={{ width: '3rem', height: '3rem', margin: '0 auto 1rem', opacity: 0.5 }} />
            <p style={{ margin: '0 0 0.5rem 0' }}>No entities found for this patient</p>
            <p style={{ fontSize: '0.875rem', margin: 0 }}>Upload and process a document to see extracted entities here.</p>
          </div>
        </Card>
      )}
    </div>
  );
}
