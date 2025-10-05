import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UserProfileDto } from '../../core/models/profile.model';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);
  private router = inject(Router);

  profile: UserProfileDto | null = null;
  profileForm!: FormGroup;
  isEditMode = false;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  phonePattern = /^\+(?:[0-9] ?){6,14}[0-9]$/;

  ngOnInit(): void {
    this.initializeForm();
    this.loadProfile();
  }

  private initializeForm(): void {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      phoneNumber: ['', [Validators.required, Validators.pattern(this.phonePattern)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]]
    });

    // Disable form initially (view mode)
    this.profileForm.disable();
  }

  loadProfile(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.authService.getProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
        this.populateForm(profile);
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.message || 'Failed to load profile';
        this.isLoading = false;
        console.error('Error loading profile:', error);
      }
    });
  }

  private populateForm(profile: UserProfileDto): void {
    this.profileForm.patchValue({
      firstName: profile.firstName,
      lastName: profile.lastName,
      phoneNumber: profile.phoneNumber,
      email: profile.email
    });
  }

  enableEditMode(): void {
    this.isEditMode = true;
    this.profileForm.enable();
    this.errorMessage = '';
    this.successMessage = '';
  }

  cancelEdit(): void {
    this.isEditMode = false;
    this.profileForm.disable();
    if (this.profile) {
      this.populateForm(this.profile);
    }
    this.errorMessage = '';
    this.successMessage = '';
  }

  onSubmit(): void {
    if (this.profileForm.invalid) {
      this.markFormGroupTouched(this.profileForm);
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const updateRequest = this.profileForm.value;

    this.authService.updateProfile(updateRequest).subscribe({
      next: (response) => {
        this.profile = response.updatedProfile;
        this.populateForm(response.updatedProfile);
        this.isEditMode = false;
        this.profileForm.disable();
        this.isLoading = false;

        if (response.emailChangeRequiresConfirmation) {
          this.successMessage = 'Profile updated successfully. Please check your new email to confirm the email change.';
        } else {
          this.successMessage = 'Profile updated successfully.';
        }

        // Clear success message after 5 seconds
        setTimeout(() => {
          this.successMessage = '';
        }, 5000);
      },
      error: (error) => {
        this.errorMessage = error.message || 'Failed to update profile';
        this.isLoading = false;
        console.error('Error updating profile:', error);
      }
    });
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.profileForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.profileForm.get(fieldName);
    if (!field || !field.errors) {
      return '';
    }

    if (field.errors['required']) {
      return `${this.getFieldLabel(fieldName)} is required`;
    }
    if (field.errors['email']) {
      return 'Invalid email format';
    }
    if (field.errors['maxLength']) {
      return `${this.getFieldLabel(fieldName)} is too long`;
    }
    if (field.errors['pattern']) {
      return 'Phone number must be in international format (e.g., +48123456789)';
    }

    return 'Invalid value';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      firstName: 'First name',
      lastName: 'Last name',
      phoneNumber: 'Phone number',
      email: 'Email'
    };
    return labels[fieldName] || fieldName;
  }

  formatDate(dateString: string | null): string {
    if (!dateString) {
      return 'Never';
    }
    return new Date(dateString).toLocaleString();
  }

  onLogout(): void {
    if (confirm('Are you sure you want to logout?')) {
      this.authService.logout().subscribe({
        next: () => {
          // Logout successful, user will be redirected to login by AuthService
          console.log('Logout successful');
        },
        error: (error) => {
          // Even if logout API fails, clear local session
          console.error('Logout error:', error);
          // User is still redirected to login by AuthService.clearSession()
        }
      });
    }
  }
}

