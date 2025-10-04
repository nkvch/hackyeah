import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-resend-activation',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    InputTextModule,
    ButtonModule,
    MessageModule
  ],
  templateUrl: './resend-activation.component.html',
  styleUrl: './resend-activation.component.scss'
})
export class ResendActivationComponent implements OnInit {
  resendForm!: FormGroup;
  isLoading = false;
  successMessage = '';
  errorMessage = '';
  
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit(): void {
    this.resendForm = this.fb.group({
      email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]]
    });
  }

  get f() {
    return this.resendForm.controls;
  }

  onSubmit(): void {
    if (this.resendForm.invalid) {
      this.resendForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const email = this.resendForm.value.email;

    this.authService.resendActivation(email).subscribe({
      next: (response) => {
        this.successMessage = response.message;
        this.isLoading = false;
        this.resendForm.reset();
      },
      error: (error: Error) => {
        this.errorMessage = error.message;
        this.isLoading = false;
      }
    });
  }

  onGoToLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  onGoToRegister(): void {
    this.router.navigate(['/auth/register']);
  }
}

