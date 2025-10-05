import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  SendMessageResponse,
  GetMessagesResponse,
  MessageDetail,
  GetAvailableRecipientsResponse,
} from '../models/message.model';

/**
 * Service for managing messages
 * Story 5.1: Send Message with Attachments
 * Story 5.2: Receive and View Messages
 */
@Injectable({
  providedIn: 'root',
})
export class MessagesService {
  private readonly apiUrl = 'http://localhost:8080/api/messages';

  constructor(private http: HttpClient) {}

  /**
   * Sends a message with optional file attachments
   * Uses multipart/form-data for file uploads
   */
  sendMessage(
    subject: string,
    body: string,
    recipientUserIds: string[],
    contextType: string,
    contextId: string | null,
    parentMessageId: string | null,
    attachments: File[],
  ): Observable<SendMessageResponse> {
    const formData = new FormData();

    formData.append('subject', subject);
    formData.append('body', body);
    formData.append('contextType', contextType);

    if (contextId) {
      formData.append('contextId', contextId);
    }

    if (parentMessageId) {
      formData.append('parentMessageId', parentMessageId);
    }

    // Add recipient IDs
    recipientUserIds.forEach((id, index) => {
      formData.append(`recipientUserIds[${index}]`, id);
    });

    // Add file attachments
    attachments.forEach((file, index) => {
      formData.append(`attachments`, file, file.name);
    });

    // Don't set Content-Type header - browser will set it automatically with boundary
    return this.http.post<SendMessageResponse>(this.apiUrl, formData);
  }

  /**
   * Gets list of messages for current user with pagination
   * Story 5.2: Receive and View Messages
   */
  getMessages(pageNumber: number = 1, pageSize: number = 20): Observable<GetMessagesResponse> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<GetMessagesResponse>(this.apiUrl, { params });
  }

  /**
   * Gets a single message by ID with full details
   * Story 5.2: Receive and View Messages
   */
  getMessageDetail(id: string): Observable<MessageDetail> {
    return this.http.get<MessageDetail>(`${this.apiUrl}/${id}`);
  }

  /**
   * Gets download URL for an attachment
   * Story 5.2: Receive and View Messages
   */
  getAttachmentDownloadUrl(messageId: string, attachmentId: string): string {
    return `${this.apiUrl}/${messageId}/attachments/${attachmentId}/download`;
  }

  /**
   * Downloads an attachment as a blob (with authentication)
   * Story 5.2: Receive and View Messages - Download attachments
   */
  downloadAttachment(messageId: string, attachmentId: string): Observable<Blob> {
    const url = `${this.apiUrl}/${messageId}/attachments/${attachmentId}/download`;
    return this.http.get(url, { responseType: 'blob' });
  }

  /**
   * Gets list of available recipients for messaging
   * Story 5.1: Compose message - get recipients
   */
  getAvailableRecipients(): Observable<GetAvailableRecipientsResponse> {
    return this.http.get<GetAvailableRecipientsResponse>(`${this.apiUrl}/recipients`);
  }
}
