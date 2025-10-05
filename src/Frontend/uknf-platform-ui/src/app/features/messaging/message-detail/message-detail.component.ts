import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { MessagesService } from '../../../core/services/messages.service';
import { MessageDetail } from '../../../core/models/message.model';

/**
 * Message detail component
 * Story 5.2: Receive and View Messages (MVP)
 */
@Component({
  selector: 'app-message-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './message-detail.component.html',
  styleUrl: './message-detail.component.scss',
})
export class MessageDetailComponent implements OnInit {
  message: MessageDetail | null = null;
  isLoading = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private messagesService: MessagesService,
  ) {}

  ngOnInit(): void {
    const messageId = this.route.snapshot.paramMap.get('id');
    if (messageId) {
      this.loadMessage(messageId);
    } else {
      this.error = 'No message ID provided';
    }
  }

  loadMessage(messageId: string): void {
    this.isLoading = true;
    this.error = null;

    this.messagesService.getMessageDetail(messageId).subscribe({
      next: (message: MessageDetail) => {
        this.message = message;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading message:', err);
        this.error = 'Failed to load message. Please try again.';
        this.isLoading = false;
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/messages']);
  }

  getAttachmentDownloadUrl(attachmentId: string): string {
    if (!this.message) return '';
    return this.messagesService.getAttachmentDownloadUrl(this.message.messageId, attachmentId);
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'AwaitingUserResponse':
        return 'status-badge status-warning';
      case 'AwaitingUknfResponse':
        return 'status-badge status-info';
      case 'Closed':
        return 'status-badge status-neutral';
      default:
        return 'status-badge';
    }
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  }

  downloadAttachment(attachmentId: string, fileName: string): void {
    if (!this.message) return;

    this.messagesService.downloadAttachment(this.message.messageId, attachmentId).subscribe({
      next: (blob: Blob) => {
        // Create a blob URL and trigger download
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        // Clean up the blob URL
        setTimeout(() => window.URL.revokeObjectURL(url), 100);
      },
      error: (err) => {
        console.error('Error downloading attachment:', err);
        alert('Failed to download attachment. Please try again.');
      },
    });
  }
}
