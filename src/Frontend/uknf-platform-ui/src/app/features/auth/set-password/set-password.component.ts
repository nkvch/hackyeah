import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { AuthService } from '../../../core/services/auth.service';
import { PasswordPolicy } from '../../../core/models/password.model';
import { PasswordStrengthComponent } from '../../../shared/components/password-strength/password-strength.component';
import { TranslationService } from '../../../core/services/translation.service';
import { LanguageSwitcherComponent } from '../../../shared/components/language-switcher/language-switcher.component';

/**
 * Component for setting initial password after account activation
 */
@Component({
  selector: 'app-set-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    ButtonModule,
    InputTextModule,
    MessageModule,
    ProgressSpinnerModule,
    PasswordStrengthComponent,
    LanguageSwitcherComponent,
  ],
  templateUrl: './set-password.component.html',
  styleUrls: ['./set-password.component.scss'],
})
export class SetPasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  public t = inject(TranslationService);

  setPasswordForm!: FormGroup;
  token: string = '';
  passwordPolicy?: PasswordPolicy;
  isLoading = false;
  isSuccess = false;
  errorMessage = '';
  showPassword = false;
  showPasswordConfirmation = false;
  countdown = 3;

  ngOnInit(): void {
    // Initialize form first to prevent errors
    this.initializeForm();

    // Extract token from query parameters
    this.token = this.route.snapshot.queryParamMap.get('token') || '';

    if (!this.token) {
      this.errorMessage = 'Invalid activation link. Please request a new one.';
      return;
    }

    // Load password policy
    this.loadPasswordPolicy();
  }

  private loadPasswordPolicy(): void {
    this.authService.getPasswordPolicy().subscribe({
      next: (policy) => {
        this.passwordPolicy = policy;
      },
      error: (error) => {
        console.error('Failed to load password policy:', error);
        // Continue anyway with default validation
      },
    });
  }

  private initializeForm(): void {
    this.setPasswordForm = this.fb.group(
      {
        password: ['', [Validators.required, Validators.minLength(8)]],
        passwordConfirmation: ['', [Validators.required]],
      },
      {
        validators: this.passwordsMatchValidator,
      },
    );
  }

  private passwordsMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const password = group.get('password')?.value;
    const confirmation = group.get('passwordConfirmation')?.value;
    return password === confirmation ? null : { passwordMismatch: true };
  }

  get password() {
    return this.setPasswordForm.get('password');
  }

  get passwordConfirmation() {
    return this.setPasswordForm.get('passwordConfirmation');
  }

  get passwordValue(): string {
    return this.password?.value || '';
  }

  get policyRequirementsMet(): { [key: string]: boolean } {
    const pwd = this.passwordValue;
    if (!this.passwordPolicy) return {};

    return {
      minLength: pwd.length >= (this.passwordPolicy.minLength || 8),
      hasUppercase: !this.passwordPolicy.requireUppercase || /[A-Z]/.test(pwd),
      hasLowercase: !this.passwordPolicy.requireLowercase || /[a-z]/.test(pwd),
      hasDigit: !this.passwordPolicy.requireDigit || /[0-9]/.test(pwd),
      hasSpecialChar: !this.passwordPolicy.requireSpecialChar || /[^A-Za-z0-9]/.test(pwd),
    };
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  togglePasswordConfirmationVisibility(): void {
    this.showPasswordConfirmation = !this.showPasswordConfirmation;
  }

  onSubmit(): void {
    if (this.setPasswordForm.invalid || this.isLoading) {
      this.markFormAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const { password, passwordConfirmation } = this.setPasswordForm.value;

    this.authService.setPassword(this.token, password, passwordConfirmation).subscribe({
      next: (response) => {
        this.isSuccess = true;
        this.isLoading = false;
        this.startCountdown(response.redirectUrl);
      },
      error: (error) => {
        this.errorMessage = error.message || 'Failed to set password. Please try again.';
        this.isLoading = false;
      },
    });
  }

  private startCountdown(redirectUrl: string): void {
    const interval = setInterval(() => {
      this.countdown--;
      if (this.countdown === 0) {
        clearInterval(interval);
        this.router.navigate(['/auth/login']);
      }
    }, 1000);
  }

  private markFormAsTouched(): void {
    Object.keys(this.setPasswordForm.controls).forEach((key) => {
      this.setPasswordForm.get(key)?.markAsTouched();
    });
  }
}
