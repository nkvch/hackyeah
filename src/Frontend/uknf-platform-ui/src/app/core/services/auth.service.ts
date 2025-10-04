import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { RegisterRequest, RegisterResponse } from '../models/register.model';
import { ActivateAccountResponse, ResendActivationRequest, ResendActivationResponse } from '../models/activation.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private readonly apiUrl = '/api';

  /**
   * Register a new external user
   */
  register(registerData: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.apiUrl}/auth/register`, registerData)
      .pipe(
        catchError(this.handleRegistrationError)
      );
  }

  /**
   * Activate a user account using activation token
   */
  activateAccount(token: string): Observable<ActivateAccountResponse> {
    return this.http.get<ActivateAccountResponse>(`${this.apiUrl}/auth/activate?token=${token}`)
      .pipe(
        catchError(this.handleActivationError)
      );
  }

  /**
   * Resend activation email to a user
   */
  resendActivation(email: string): Observable<ResendActivationResponse> {
    const request: ResendActivationRequest = { email };
    return this.http.post<ResendActivationResponse>(`${this.apiUrl}/auth/resend-activation`, request)
      .pipe(
        catchError(this.handleGenericError)
      );
  }

  private handleRegistrationError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An error occurred during registration';

    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else {
      if (error.status === 409) {
        errorMessage = 'Email or PESEL already registered';
      } else if (error.status === 400) {
        errorMessage = 'Invalid registration data. Please check your input.';
      } else if (error.error?.error) {
        errorMessage = error.error.error;
      }
    }

    console.error('Registration error:', error);
    return throwError(() => new Error(errorMessage));
  }

  private handleActivationError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Account activation failed';

    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else if (error.error?.error) {
      errorMessage = error.error.error;
    } else if (error.status === 400) {
      errorMessage = 'Invalid or expired activation link';
    }

    console.error('Activation error:', error);
    return throwError(() => new Error(errorMessage));
  }

  private handleGenericError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An error occurred';

    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else if (error.error?.error) {
      errorMessage = error.error.error;
    }

    console.error('Error:', error);
    return throwError(() => new Error(errorMessage));
  }
}

