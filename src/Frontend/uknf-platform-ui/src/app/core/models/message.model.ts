/**
 * Models for messaging functionality
 * Story 5.1: Send Message with Attachments
 * Story 5.2: Receive and View Messages
 */

export interface SendMessageRequest {
  subject: string;
  body: string;
  recipientUserIds: string[];
  contextType: 'Standalone' | 'AccessRequest' | 'Report' | 'Case';
  contextId?: string;
  parentMessageId?: string;
  attachments: File[];
}

export interface SendMessageResponse {
  messageId: string;
  sentDate: string;
  recipientCount: number;
  attachmentCount: number;
  message: string;
}

// Story 5.2: Message list response
export interface GetMessagesResponse {
  messages: MessageSummary[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Story 5.2: Message summary for list view
export interface MessageSummary {
  messageId: string;
  subject: string;
  bodyPreview: string;
  senderName: string;
  senderEmail: string;
  sentDate: string;
  messageStatus: string;
  isRead: boolean;
  attachmentCount: number;
  contextType: string;
  contextId?: string;
}

// Story 5.2: Full message detail
export interface MessageDetail {
  messageId: string;
  subject: string;
  body: string;
  senderName: string;
  senderEmail: string;
  sentDate: string;
  messageStatus: string;
  isRead: boolean;
  contextType: string;
  contextId?: string;
  parentMessageId?: string;
  attachments: MessageAttachment[];
}

// Legacy interface - kept for backward compatibility
export interface Message {
  id: string;
  subject: string;
  body: string;
  senderId: string;
  status: 'AwaitingUknfResponse' | 'AwaitingUserResponse' | 'Closed';
  contextType: 'Standalone' | 'AccessRequest' | 'Report' | 'Case';
  contextId?: string;
  parentMessageId?: string;
  sentDate: string;
  isRead: boolean;
  attachments: MessageAttachment[];
}

export interface MessageAttachment {
  attachmentId: string;
  fileName: string;
  fileSize: number;
  contentType?: string;
  uploadedDate: string;
}

// Story 5.1: Get available recipients
export interface GetAvailableRecipientsResponse {
  recipients: RecipientDto[];
}

export interface RecipientDto {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  isInternal: boolean;
}

export const ALLOWED_FILE_EXTENSIONS = [
  '.pdf',
  '.doc',
  '.docx',
  '.xls',
  '.xlsx',
  '.csv',
  '.txt',
  '.mp3',
  '.zip',
];
export const MAX_FILE_SIZE = 104857600; // 100 MB in bytes
export const MAX_TOTAL_SIZE = 104857600; // 100 MB in bytes
