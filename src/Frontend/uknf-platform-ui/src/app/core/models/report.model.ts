/**
 * Report-related models for Epic 4
 */

/**
 * Request to submit a regulatory report
 * Sent as multipart/form-data
 */
export interface SubmitReportRequest {
  entityId: number;
  reportType: string;
  reportingPeriod: string;
  file: File;
}

/**
 * Response after successful report submission
 */
export interface SubmitReportResponse {
  reportId: string;
  uniqueValidationId: string | null;
  status: string;
  message: string;
  submittedDate: string;
  submitterName: string;
  entityName: string;
}

/**
 * Report validation status enum
 */
export enum ValidationStatus {
  Working = 'Working',
  Transmitted = 'Transmitted',
  Ongoing = 'Ongoing',
  Successful = 'Successful',
  ValidationErrors = 'ValidationErrors',
  TechnicalError = 'TechnicalError',
  TimeoutError = 'TimeoutError',
  ContestedByUKNF = 'ContestedByUKNF'
}

/**
 * Entity (supervised entity like bank, insurance company)
 * Stub for MVP - full implementation in Epic 2
 */
export interface Entity {
  entityId: number;
  name: string;
  type: string;
  isActive: boolean;
}

/**
 * Report type options
 */
export const REPORT_TYPES = [
  { value: 'Quarterly', label: 'Quarterly Report' },
  { value: 'Annual', label: 'Annual Report' },
  { value: 'Monthly', label: 'Monthly Report' },
  { value: 'Ad-hoc', label: 'Ad-hoc Report' }
];

