import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MessagesService } from '../../../core/services/messages.service';
import { MessageSummary, GetMessagesResponse } from '../../../core/models/message.model';

/**
 * Messages list component
 * Story 5.2: Receive and View Messages (MVP)
 */
@Component({
  selector: 'app-messages-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './messages-list.component.html',
  styleUrl: './messages-list.component.scss',
})
export class MessagesListComponent implements OnInit {
  messages: MessageSummary[] = [];
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;
  totalPages = 0;
  isLoading = false;
  error: string | null = null;

  constructor(private messagesService: MessagesService) {}

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages(): void {
    this.isLoading = true;
    this.error = null;

    this.messagesService.getMessages(this.currentPage, this.pageSize).subscribe({
      next: (response: GetMessagesResponse) => {
        this.messages = response.messages;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.currentPage = response.pageNumber;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading messages:', err);
        this.error = 'Failed to load messages. Please try again.';
        this.isLoading = false;
      },
    });
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadMessages();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
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
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));

    if (days === 0) {
      return 'Today, ' + date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
    } else if (days === 1) {
      return 'Yesterday';
    } else if (days < 7) {
      return `${days} days ago`;
    } else {
      return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  }
}
