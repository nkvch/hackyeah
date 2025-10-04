import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    InputTextModule,
    ButtonModule,
    CardModule,
    MessageModule
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  registerForm!: FormGroup;
  loading = false;
  successMessage = '';
  errorMessage = '';
  peselDisplay = '';

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
      phone: ['', [Validators.required, Validators.pattern(/^\+(?:[0-9] ?){6,14}[0-9]$/)]],
      pesel: ['', [Validators.required, Validators.pattern(/^\d{11}$/), Validators.minLength(11), Validators.maxLength(11)]]
    });

    // Watch PESEL field for masking
    this.registerForm.get('pesel')?.valueChanges.subscribe(value => {
      if (value && value.length >= 4) {
        const last4 = value.slice(-4);
        this.peselDisplay = '*'.repeat(value.length - 4) + last4;
      } else {
        this.peselDisplay = value || '';
      }
    });
  }

  get f() {
    return this.registerForm.controls;
  }

  getErrorMessage(controlName: string): string {
    const control = this.registerForm.get(controlName);
    if (!control || !control.errors || !control.touched) {
      return '';
    }

    if (control.errors['required']) {
      return `${this.getFieldName(controlName)} is required`;
    }
    if (control.errors['email']) {
      return 'Invalid email format';
    }
    if (control.errors['maxLength']) {
      return `${this.getFieldName(controlName)} cannot exceed ${control.errors['maxLength'].requiredLength} characters`;
    }
    if (control.errors['pattern']) {
      if (controlName === 'phone') {
        return 'Phone number must be in international format (e.g., +48123456789)';
      }
      if (controlName === 'pesel') {
        return 'PESEL must be exactly 11 digits';
      }
    }
    if (control.errors['minLength'] || control.errors['maxLength']) {
      if (controlName === 'pesel') {
        return 'PESEL must be exactly 11 digits';
      }
    }

    return 'Invalid input';
  }

  private getFieldName(controlName: string): string {
    const fieldNames: { [key: string]: string } = {
      firstName: 'First name',
      lastName: 'Last name',
      email: 'Email',
      phone: 'Phone number',
      pesel: 'PESEL'
    };
    return fieldNames[controlName] || controlName;
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      Object.keys(this.registerForm.controls).forEach(key => {
        this.registerForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.register(this.registerForm.value).subscribe({
      next: (response) => {
        this.loading = false;
        this.successMessage = response.message;
        
        // Navigate to login page after 3 seconds
        setTimeout(() => {
          this.router.navigate(['/auth/login']);
        }, 3000);
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.message || 'Registration failed. Please try again.';
      }
    });
  }
}

