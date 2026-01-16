# Task - [TASK_002]

## Requirement Reference
- User Story: [us_049]
- Story Location: [.propel/context/tasks/us_049/us_049.md]
- Acceptance Criteria: 
    - Given the upload UI, When files are selected, Then the count is displayed and limit is communicated.
    - Given batch limit is exceeded, When detected, Then a warning is displayed about the remaining files.
    - Given a batch upload, When more than 10 files are selected, Then only the first 10 are accepted with warning.

## Task Overview
Implement frontend UI components for batch upload with 10-file limit enforcement. The UI must display file count, communicate the limit clearly, show warnings when exceeded, and provide feedback on which files were accepted vs rejected. This integrates with the batch upload API endpoint.

## Dependent Tasks
- [US_049/task_001] - Backend batch upload limit enforcement

## Impacted Components
- [MODIFY | app/src/components/upload/DocumentUpload.tsx | Add batch limit UI and file count display]
- [CREATE | app/src/components/upload/BatchLimitWarning.tsx | Warning component for batch limit exceeded]
- [CREATE | app/src/components/upload/FileSelectionList.tsx | List component showing selected files with status]
- [MODIFY | app/src/services/documentService.ts | Add batch upload API call]

## Implementation Plan

### 1. Update DocumentUpload Component
```tsx
const MAX_FILES_PER_BATCH = 10;

export const DocumentUpload: React.FC<DocumentUploadProps> = ({ patientId, onUploadComplete }) => {
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadResults, setUploadResults] = useState<FileUploadResult[]>([]);
  
  const handleFilesSelected = (files: FileList) => {
    const fileArray = Array.from(files);
    setSelectedFiles(fileArray);
  };
  
  const exceedsLimit = selectedFiles.length > MAX_FILES_PER_BATCH;
  const filesToUpload = selectedFiles.slice(0, MAX_FILES_PER_BATCH);
  const excessFiles = selectedFiles.slice(MAX_FILES_PER_BATCH);
  
  return (
    <div className="document-upload">
      <DropZone onFilesSelected={handleFilesSelected} />
      
      <FileCountDisplay 
        count={selectedFiles.length} 
        limit={MAX_FILES_PER_BATCH} 
      />
      
      {exceedsLimit && (
        <BatchLimitWarning 
          totalSelected={selectedFiles.length}
          limit={MAX_FILES_PER_BATCH}
          excessCount={excessFiles.length}
        />
      )}
      
      <FileSelectionList 
        files={selectedFiles}
        limit={MAX_FILES_PER_BATCH}
        uploadResults={uploadResults}
      />
      
      <UploadButton 
        disabled={selectedFiles.length === 0 || isUploading}
        onClick={handleUpload}
      />
    </div>
  );
};
```

### 2. Create FileCountDisplay Component
```tsx
interface FileCountDisplayProps {
  count: number;
  limit: number;
}

export const FileCountDisplay: React.FC<FileCountDisplayProps> = ({ count, limit }) => {
  const isOverLimit = count > limit;
  
  return (
    <div className={`file-count ${isOverLimit ? 'text-amber-600' : 'text-gray-600'}`}>
      <span className="font-medium">{count}</span> file{count !== 1 ? 's' : ''} selected
      <span className="text-sm ml-2">
        (max {limit} per batch)
      </span>
    </div>
  );
};
```

### 3. Create BatchLimitWarning Component
```tsx
interface BatchLimitWarningProps {
  totalSelected: number;
  limit: number;
  excessCount: number;
}

export const BatchLimitWarning: React.FC<BatchLimitWarningProps> = ({
  totalSelected,
  limit,
  excessCount
}) => {
  return (
    <div className="batch-limit-warning bg-amber-50 border border-amber-200 rounded-lg p-4 my-4">
      <div className="flex items-start">
        <AlertTriangle className="h-5 w-5 text-amber-500 mt-0.5 mr-3" />
        <div>
          <h4 className="font-medium text-amber-800">
            Batch limit exceeded
          </h4>
          <p className="text-sm text-amber-700 mt-1">
            You selected {totalSelected} files, but only {limit} files can be uploaded per batch.
            The first {limit} files will be uploaded. The remaining {excessCount} file{excessCount !== 1 ? 's' : ''} will need to be uploaded in a separate batch.
          </p>
        </div>
      </div>
    </div>
  );
};
```

### 4. Create FileSelectionList Component
```tsx
interface FileSelectionListProps {
  files: File[];
  limit: number;
  uploadResults?: FileUploadResult[];
}

export const FileSelectionList: React.FC<FileSelectionListProps> = ({
  files,
  limit,
  uploadResults
}) => {
  return (
    <div className="file-selection-list mt-4 space-y-2">
      {files.map((file, index) => {
        const isWithinLimit = index < limit;
        const result = uploadResults?.find(r => r.fileName === file.name);
        
        return (
          <div 
            key={`${file.name}-${index}`}
            className={`file-item flex items-center justify-between p-3 rounded-lg border
              ${isWithinLimit ? 'bg-white border-gray-200' : 'bg-gray-50 border-gray-300 opacity-60'}`}
          >
            <div className="flex items-center">
              <FileIcon className="h-5 w-5 text-gray-400 mr-3" />
              <div>
                <span className="font-medium text-gray-900">{file.name}</span>
                <span className="text-sm text-gray-500 ml-2">
                  ({formatFileSize(file.size)})
                </span>
              </div>
            </div>
            
            <div className="flex items-center">
              {!isWithinLimit && (
                <span className="text-sm text-gray-500 mr-2">
                  Will not be uploaded
                </span>
              )}
              {result && (
                <StatusBadge status={result.status} />
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
};
```

### 5. Update Document Service
```typescript
export interface BatchUploadResponse {
  batchId: string;
  patientId: string;
  totalFilesReceived: number;
  filesAccepted: number;
  filesRejected: number;
  batchLimitExceeded: boolean;
  batchLimitWarning?: string;
  fileResults: FileUploadResult[];
  acknowledgedAt: string;
}

export interface FileUploadResult {
  fileName: string;
  documentId?: string;
  isAccepted: boolean;
  status: string;
  validationErrors: string[];
  rejectionReason?: string;
}

export const uploadDocumentBatch = async (
  patientId: string,
  files: File[]
): Promise<BatchUploadResponse> => {
  const formData = new FormData();
  formData.append('patientId', patientId);
  
  files.forEach(file => {
    formData.append('files', file);
  });
  
  const response = await api.post<BatchUploadResponse>(
    '/documents/batch',
    formData,
    {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    }
  );
  
  return response.data;
};
```

### 6. Upload Progress and Results Display
```tsx
const handleUpload = async () => {
  setIsUploading(true);
  try {
    const response = await uploadDocumentBatch(patientId, filesToUpload);
    setUploadResults(response.fileResults);
    
    if (response.batchLimitExceeded) {
      toast.warning(response.batchLimitWarning);
    }
    
    const successCount = response.fileResults.filter(r => r.isAccepted).length;
    if (successCount > 0) {
      toast.success(`${successCount} file(s) uploaded successfully`);
    }
    
    onUploadComplete?.(response);
  } catch (error) {
    toast.error('Upload failed. Please try again.');
  } finally {
    setIsUploading(false);
  }
};
```

## Current Project State
```
app/src/
├── components/
│   └── upload/
│       └── (existing upload components)
├── services/
│   └── documentService.ts
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/components/upload/DocumentUpload.tsx | Add batch limit enforcement, file count display, warning integration |
| CREATE | app/src/components/upload/BatchLimitWarning.tsx | Warning component for batch limit exceeded |
| CREATE | app/src/components/upload/FileCountDisplay.tsx | Component showing selected file count vs limit |
| CREATE | app/src/components/upload/FileSelectionList.tsx | List component showing files with acceptance status |
| MODIFY | app/src/services/documentService.ts | Add uploadDocumentBatch function and response types |

## External References
- https://react.dev/reference/react/useState
- https://tailwindcss.com/docs/background-color
- https://lucide.dev/icons/alert-triangle

## Build Commands
- cd app && npm run build
- cd app && npm run test

## Implementation Validation Strategy
- [Automated] Unit tests verify file count display updates correctly
- [Automated] Unit tests verify warning appears when limit exceeded
- [Automated] Unit tests verify excess files are visually distinguished
- [Manual] Verify drag-and-drop with 15 files shows correct warning
- [Manual] Verify upload results show accepted vs rejected files

## Implementation Checklist
- [x] Create FileCountDisplay component with limit indicator
- [x] Create BatchLimitWarning component with clear messaging
- [x] Create FileSelectionList component with acceptance status
- [x] Update DocumentUpload to integrate batch limit components
- [x] Add uploadDocumentBatch function to documentService.ts
- [x] Add BatchUploadResponse and FileUploadResult types
- [x] Implement upload progress and results display
- [x] Add toast notifications for batch limit warnings
