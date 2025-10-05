import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { MessagesService } from '../../../core/services/messages.service';
import { MessageDetail } from '../../../core/models/message.model';

/**
 * Message detail component
 * Story 5.2: Receive and View Messages (MVP)
 * Story 5.3: Reply to Message
 */
@Component({
  selector: 'app-message-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './message-detail.component.html',
  styleUrl: './message-detail.component.scss',
})
export class MessageDetailComponent implements OnInit {
  message: MessageDetail | null = null;
  isLoading = false;
  error: string | null = null;

  // Reply form state
  showReplyForm = false;
  replyBody = '';
  replyAttachments: File[] = [];
  replyLoading = false;
  replyError: string | null = null;
  replySuccess: string | null = null;

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

  toggleReplyForm(): void {
    this.showReplyForm = !this.showReplyForm;
    if (!this.showReplyForm) {
      this.clearReplyForm();
    }
  }

  onReplyFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.replyAttachments = [...this.replyAttachments, ...Array.from(input.files)];
    }
  }

  removeReplyAttachment(index: number): void {
    this.replyAttachments.splice(index, 1);
  }

  sendReply(): void {
    if (!this.message || !this.replyBody.trim()) {
      this.replyError = 'Reply body is required';
      return;
    }

    this.replyLoading = true;
    this.replyError = null;
    this.replySuccess = null;

    this.messagesService
      .replyToMessage(this.message.messageId, this.replyBody, this.replyAttachments)
      .subscribe({
        next: (response) => {
          this.replySuccess = 'Reply sent successfully!';
          this.replyLoading = false;
          this.showReplyForm = false;
          this.clearReplyForm();
          // Refresh to show updated status
          setTimeout(() => {
            this.router.navigate(['/messages']);
          }, 1500);
        },
        error: (err) => {
          console.error('Error sending reply:', err);
          this.replyError = err.error?.error || 'Failed to send reply. Please try again.';
          this.replyLoading = false;
        },
      });
  }

  clearReplyForm(): void {
    this.replyBody = '';
    this.replyAttachments = [];
    this.replyError = null;
    this.replySuccess = null;
  }
}
