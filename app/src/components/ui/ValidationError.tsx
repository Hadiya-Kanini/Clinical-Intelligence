import type { HTMLAttributes } from 'react'

export type ValidationErrorType =
  | 'invalid_type'
  | 'file_too_large'
  | 'file_corrupted'
  | 'password_protected'
  | 'empty_file'

export interface ValidationErrorInfo {
  type: ValidationErrorType
  message: string
  guidance: string
}

const ERROR_MESSAGES: Record<ValidationErrorType, { message: string; guidance: string }> = {
  invalid_type: {
    message: 'Unsupported file type',
    guidance: 'Please upload PDF or DOCX files only',
  },
  file_too_large: {
    message: 'File exceeds 50MB limit',
    guidance: 'Reduce file size or split into smaller documents',
  },
  file_corrupted: {
    message: 'File appears to be corrupted',
    guidance: 'Try re-downloading or re-exporting the file',
  },
  password_protected: {
    message: 'Password-protected files not supported',
    guidance: 'Remove password protection and try again',
  },
  empty_file: {
    message: 'File is empty',
    guidance: 'Ensure the file contains content before uploading',
  },
}

export function getValidationError(type: ValidationErrorType): ValidationErrorInfo {
  const errorInfo = ERROR_MESSAGES[type]
  return {
    type,
    message: errorInfo.message,
    guidance: errorInfo.guidance,
  }
}

type ValidationErrorProps = {
  error: ValidationErrorInfo
  showIcon?: boolean
} & Omit<HTMLAttributes<HTMLDivElement>, 'children'>

export default function ValidationError({
  error,
  showIcon = true,
  ...props
}: ValidationErrorProps): JSX.Element {
  return (
    <div
      className="ui-validation-error"
      role="alert"
      aria-live="assertive"
      data-testid="validation-error"
      {...props}
    >
      <div className="ui-validation-error__content">
        {showIcon ? (
          <span className="ui-validation-error__icon" aria-hidden="true">
            <svg
              width="16"
              height="16"
              viewBox="0 0 16 16"
              fill="currentColor"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                fillRule="evenodd"
                clipRule="evenodd"
                d="M8 1C4.13401 1 1 4.13401 1 8C1 11.866 4.13401 15 8 15C11.866 15 15 11.866 15 8C15 4.13401 11.866 1 8 1ZM7.25 4.5V8.5H8.75V4.5H7.25ZM8 11.5C8.55228 11.5 9 11.0523 9 10.5C9 9.94772 8.55228 9.5 8 9.5C7.44772 9.5 7 9.94772 7 10.5C7 11.0523 7.44772 11.5 8 11.5Z"
              />
            </svg>
          </span>
        ) : null}
        <div className="ui-validation-error__text">
          <span className="ui-validation-error__message" data-testid="validation-error-message">
            {error.message}
          </span>
          <span className="ui-validation-error__guidance" data-testid="validation-error-guidance">
            {error.guidance}
          </span>
        </div>
      </div>
    </div>
  )
}
