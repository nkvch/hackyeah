import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MessagesService } from '../../../core/services/messages.service';
import { AuthService } from '../../../core/services/auth.service';
import { ALLOWED_FILE_EXTENSIONS, MAX_TOTAL_SIZE } from '../../../core/models/message.model';

/**
 * Component for composing and sending messages with attachments
 * Story 5.1: Send Message with Attachments
 */
@Component({
  selector: 'app-compose-message',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './compose-message.component.html',
  styleUrls: ['./compose-message.component.scss'],
})
export class ComposeMessageComponent implements OnInit {
  // Form data
  subject = '';
  body = '';
  contextType: 'Standalone' | 'AccessRequest' | 'Report' | 'Case' = 'Standalone';
  contextId: string | null = null;
  recipientUserIds: string[] = [];
  attachments: File[] = [];

  // UI state
  loading = false;
  error: string | null = null;
  success: string | null = null;

  // File validation
  readonly allowedExtensions = ALLOWED_FILE_EXTENSIONS;
  readonly maxTotalSize = MAX_TOTAL_SIZE;

  // For demo/testing - in production, these would come from backend
  availableRecipients: Array<{ id: string; name: string; email: string }> = [];

  constructor(
    private messagesService: MessagesService,
    private authService: AuthService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.loadAvailableRecipients();
  }

  /**
   * Loads available recipients from backend
   */
  loadAvailableRecipients(): void {
    this.messagesService.getAvailableRecipients().subscribe({
      next: (response) => {
        this.availableRecipients = response.recipients.map((r) => ({
          id: r.userId,
          name: `${r.firstName} ${r.lastName}`,
          email: r.email,
        }));
      },
      error: (err) => {
        console.error('Error loading recipients:', err);
        this.error = 'Failed to load recipients. Please refresh the page.';
      },
    });
  }

  /**
   * Handles file selection
   */
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const newFiles = Array.from(input.files);

      // Validate each file
      for (const file of newFiles) {
        const error = this.validateFile(file);
        if (error) {
          this.error = error;
          return;
        }
      }

      // Add files to attachments
      this.attachments = [...this.attachments, ...newFiles];

      // Validate total size
      const totalError = this.validateTotalSize();
      if (totalError) {
        this.error = totalError;
        this.attachments = this.attachments.slice(0, -newFiles.length); // Remove newly added files
        return;
      }

      this.error = null;
      input.value = ''; // Reset input so same file can be selected again
    }
  }

  /**
   * Validates a single file
   */
  private validateFile(file: File): string | null {
    // Check file extension
    const extension = '.' + file.name.split('.').pop()?.toLowerCase();
    if (!this.allowedExtensions.includes(extension)) {
      return `File format not allowed. Allowed: ${this.allowedExtensions.join(', ')}`;
    }

    // Check individual file size
    if (file.size > this.maxTotalSize) {
      return `File "${file.name}" exceeds 100 MB limit`;
    }

    return null;
  }

  /**
   * Validates total size of all attachments
   */
  private validateTotalSize(): string | null {
    const totalSize = this.attachments.reduce((sum, file) => sum + file.size, 0);
    if (totalSize > this.maxTotalSize) {
      return `Total file size exceeds 100 MB limit. Current: ${this.formatFileSize(totalSize)}`;
    }
    return null;
  }

  /**
   * Removes a file from attachments
   */
  removeFile(index: number): void {
    this.attachments = this.attachments.filter((_, i) => i !== index);
    this.error = null;
  }

  /**
   * Formats file size for display
   */
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  }

  /**
   * Gets total size of all attachments
   */
  getTotalSize(): number {
    return this.attachments.reduce((sum, file) => sum + file.size, 0);
  }

  /**
   * Toggles recipient selection
   */
  toggleRecipient(recipientId: string): void {
    const index = this.recipientUserIds.indexOf(recipientId);
    if (index === -1) {
      this.recipientUserIds.push(recipientId);
    } else {
      this.recipientUserIds.splice(index, 1);
    }
  }

  /**
   * Checks if recipient is selected
   */
  isRecipientSelected(recipientId: string): boolean {
    return this.recipientUserIds.includes(recipientId);
  }

  /**
   * Validates the form
   */
  private validateForm(): string | null {
    if (!this.subject.trim()) {
      return 'Subject is required';
    }
    if (this.subject.length > 500) {
      return 'Subject cannot exceed 500 characters';
    }
    if (!this.body.trim()) {
      return 'Message body is required';
    }
    if (this.body.length > 10000) {
      return 'Message body cannot exceed 10,000 characters';
    }
    if (this.recipientUserIds.length === 0) {
      return 'At least one recipient is required';
    }
    if (this.contextType !== 'Standalone' && !this.contextId) {
      return 'Context ID is required when context type is not Standalone';
    }

    const sizeError = this.validateTotalSize();
    if (sizeError) {
      return sizeError;
    }

    return null;
  }

  /**
   * Sends the message
   */
  sendMessage(): void {
    this.error = null;
    this.success = null;

    // Validate form
    const validationError = this.validateForm();
    if (validationError) {
      this.error = validationError;
      return;
    }

    this.loading = true;

    this.messagesService
      .sendMessage(
        this.subject,
        this.body,
        this.recipientUserIds,
        this.contextType,
        this.contextId,
        null, // parentMessageId
        this.attachments,
      )
      .subscribe({
        next: (response) => {
          this.loading = false;
          this.success =
            response.message ||
            `Message sent successfully to ${response.recipientCount} recipient(s)`;

          // Reset form after short delay
          setTimeout(() => {
            this.router.navigate(['/messages']); // Navigate to messages list (Story 5.2+)
          }, 2000);
        },
        error: (err) => {
          this.loading = false;

          // Handle specific error types
          if (err.error?.error) {
            const errorMsg = err.error.error.toLowerCase();

            if (errorMsg.includes('virus')) {
              this.error =
                'File contains a virus and was rejected. Please remove the infected file.';
            } else if (errorMsg.includes('spam')) {
              this.error = 'Message flagged as spam. Please review your content.';
            } else if (errorMsg.includes('size') || errorMsg.includes('100 mb')) {
              this.error = 'Total file size exceeds 100 MB limit. Please remove some files.';
            } else if (errorMsg.includes('format') || errorMsg.includes('extension')) {
              this.error = `File format not allowed. Allowed: ${this.allowedExtensions.join(', ')}`;
            } else {
              this.error = err.error.error || 'Failed to send message';
            }
          } else {
            this.error = 'Failed to send message. Please try again.';
          }
        },
      });
  }

  /**
   * Cancels and goes back to profile
   */
  cancel(): void {
    this.router.navigate(['/profile']);
  }
}
