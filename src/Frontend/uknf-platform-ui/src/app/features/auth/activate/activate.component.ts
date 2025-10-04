import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { AuthService } from '../../../core/services/auth.service';

type ActivationState = 'loading' | 'success' | 'error-invalid' | 'error-expired' | 'error-already-used' | 'error-generic';

@Component({
  selector: 'app-activate',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    ProgressSpinnerModule,
    MessageModule
  ],
  templateUrl: './activate.component.html',
  styleUrl: './activate.component.scss'
})
export class ActivateComponent implements OnInit {
  state: ActivationState = 'loading';
  errorMessage = '';
  successMessage = '';
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);

  ngOnInit(): void {
    // Get token from query parameters
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!token) {
      this.state = 'error-invalid';
      this.errorMessage = 'No activation token provided';
      return;
    }

    // Activate account
    this.activateAccount(token);
  }

  private activateAccount(token: string): void {
    this.state = 'loading';

    this.authService.activateAccount(token).subscribe({
      next: (response) => {
        this.state = 'success';
        this.successMessage = response.message;
      },
      error: (error: Error) => {
        this.handleError(error.message);
      }
    });
  }

  private handleError(errorMessage: string): void {
    this.errorMessage = errorMessage;

    if (errorMessage.includes('invalid') || errorMessage.includes('Invalid')) {
      this.state = 'error-invalid';
    } else if (errorMessage.includes('expired') || errorMessage.includes('Expired')) {
      this.state = 'error-expired';
    } else if (errorMessage.includes('already') || errorMessage.includes('Already')) {
      this.state = 'error-already-used';
    } else {
      this.state = 'error-generic';
    }
  }

  onSetPassword(): void {
    // Navigate to password creation page (Story 1.3)
    this.router.navigate(['/auth/set-password']);
  }

  onRequestNewLink(): void {
    // Navigate to resend activation page
    this.router.navigate(['/auth/resend-activation']);
  }

  onGoToLogin(): void {
    // Navigate to login page
    this.router.navigate(['/auth/login']);
  }

  get isLoading(): boolean {
    return this.state === 'loading';
  }

  get isSuccess(): boolean {
    return this.state === 'success';
  }

  get isError(): boolean {
    return this.state.startsWith('error-');
  }

  get showRequestNewLink(): boolean {
    return this.state === 'error-expired';
  }
}

