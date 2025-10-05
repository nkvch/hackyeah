import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AccessRequestService } from '../../../core/services/access-request.service';
import { AccessRequestDto } from '../../../core/models/access-request.model';

@Component({
  selector: 'app-my-access-request',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './my-access-request.component.html',
  styleUrls: ['./my-access-request.component.scss']
})
export class MyAccessRequestComponent implements OnInit {
  accessRequest: AccessRequestDto | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private accessRequestService: AccessRequestService) {}

  ngOnInit(): void {
    this.loadAccessRequest();
  }

  loadAccessRequest(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.accessRequestService.getMyAccessRequest().subscribe({
      next: (data) => {
        this.accessRequest = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading access request:', error);
        this.errorMessage = error.error?.error || 'Failed to load access request';
        this.isLoading = false;
      }
    });
  }

  getStatusClass(status: string): string {
    switch(status.toLowerCase()) {
      case 'working': return 'status-working';
      case 'new': return 'status-new';
      case 'accepted': return 'status-accepted';
      case 'blocked': return 'status-blocked';
      case 'updated': return 'status-updated';
      default: return '';
    }
  }

  getStatusLabel(status: string): string {
    switch(status.toLowerCase()) {
      case 'working': return 'Draft';
      case 'new': return 'Submitted';
      case 'accepted': return 'Accepted';
      case 'blocked': return 'Blocked';
      case 'updated': return 'Updated';
      default: return status;
    }
  }
}

