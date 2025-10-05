import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { RegisterRequest, RegisterResponse } from '../models/register.model';
import { ActivateAccountResponse, ResendActivationRequest, ResendActivationResponse } from '../models/activation.model';
import { SetPasswordRequest, SetPasswordResponse, PasswordPolicy } from '../models/password.model';
import { LoginRequest, LoginResponse, RefreshTokenRequest, LogoutRequest, UserInfo } from '../models/login.model';
import { UserProfileDto, UpdateProfileRequest, UpdateProfileResponse } from '../models/profile.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private readonly apiUrl = '/api';

  // Authentication state management
  private currentUserSubject = new BehaviorSubject<UserInfo | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  // Local storage keys
  private readonly ACCESS_TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_INFO_KEY = 'user_info';
  private readonly TOKEN_EXPIRY_KEY = 'token_expiry';

  constructor() {
    // Initialize authentication state from storage on service creation
    this.initializeAuthState();
  }

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

  /**
   * Set initial password for newly activated account
   */
  setPassword(token: string, password: string, passwordConfirmation: string): Observable<SetPasswordResponse> {
    const request: SetPasswordRequest = { token, password, passwordConfirmation };
    return this.http.post<SetPasswordResponse>(`${this.apiUrl}/auth/set-password`, request)
      .pipe(
        catchError(this.handleSetPasswordError)
      );
  }

  /**
   * Get current password policy configuration
   */
  getPasswordPolicy(): Observable<PasswordPolicy> {
    return this.http.get<PasswordPolicy>(`${this.apiUrl}/auth/password-policy`)
      .pipe(
        catchError(this.handleGenericError)
      );
  }

  /**
   * Login with email and password
   */
  login(email: string, password: string): Observable<LoginResponse> {
    const request: LoginRequest = { email, password };
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, request)
      .pipe(
        tap(response => this.handleLoginSuccess(response)),
        catchError(this.handleLoginError)
      );
  }

  /**
   * Logout and clear session
   */
  logout(): Observable<void> {
    const refreshToken = this.getRefreshToken();
    const request: LogoutRequest = { refreshToken: refreshToken || '' };
    
    return this.http.post<void>(`${this.apiUrl}/auth/logout`, request)
      .pipe(
        tap(() => this.clearSession()),
        catchError(error => {
          // Clear session even if logout fails
          this.clearSession();
          return throwError(() => error);
        })
      );
  }

  /**
   * Refresh access token using refresh token
   */
  refreshToken(): Observable<LoginResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    const request: RefreshTokenRequest = { refreshToken };
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/refresh-token`, request)
      .pipe(
        tap(response => this.handleLoginSuccess(response)),
        catchError(error => {
          // If refresh fails, clear session and redirect to login
          this.clearSession();
          return throwError(() => error);
        })
      );
  }

  /**
   * Get current user information
   */
  getCurrentUser(): UserInfo | null {
    return this.currentUserSubject.value;
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  /**
   * Get access token
   */
  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_TOKEN_KEY);
  }

  /**
   * Get refresh token
   */
  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  /**
   * Get current user's profile
   */
  getProfile(): Observable<UserProfileDto> {
    return this.http.get<UserProfileDto>(`${this.apiUrl}/auth/profile`)
      .pipe(
        catchError(this.handleGenericError)
      );
  }

  /**
   * Update user profile
   */
  updateProfile(profile: UpdateProfileRequest): Observable<UpdateProfileResponse> {
    return this.http.put<UpdateProfileResponse>(`${this.apiUrl}/auth/profile`, profile)
      .pipe(
        catchError(this.handleGenericError)
      );
  }

  /**
   * Handle successful login response
   */
  private handleLoginSuccess(response: LoginResponse): void {
    // Store tokens
    localStorage.setItem(this.ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(this.USER_INFO_KEY, JSON.stringify(response.user));
    
    // Calculate and store expiry time
    const expiryTime = Date.now() + (response.expiresIn * 1000);
    localStorage.setItem(this.TOKEN_EXPIRY_KEY, expiryTime.toString());

    // Update state
    this.currentUserSubject.next(response.user);
    this.isAuthenticatedSubject.next(true);
  }

  /**
   * Clear session and logout
   */
  private clearSession(): void {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_INFO_KEY);
    localStorage.removeItem(this.TOKEN_EXPIRY_KEY);

    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);

    this.router.navigate(['/login']);
  }

  /**
   * Get user from local storage
   */
  private getUserFromStorage(): UserInfo | null {
    const userJson = localStorage.getItem(this.USER_INFO_KEY);
    if (!userJson) {
      return null;
    }

    try {
      return JSON.parse(userJson);
    } catch {
      return null;
    }
  }

  /**
   * Initialize authentication state from localStorage
   * Called on service creation to restore session after page refresh
   */
  private initializeAuthState(): void {
    const hasToken = this.hasValidToken();
    const user = this.getUserFromStorage();

    console.log('[AuthService] Initializing auth state', {
      hasToken,
      hasUser: !!user,
      userId: user?.userId
    });

    if (hasToken && user) {
      this.currentUserSubject.next(user);
      this.isAuthenticatedSubject.next(true);
      console.log('[AuthService] User authenticated from storage', user.email);
    } else {
      this.currentUserSubject.next(null);
      this.isAuthenticatedSubject.next(false);
      console.log('[AuthService] No valid session found');
    }
  }

  /**
   * Check if access token is valid
   */
  private hasValidToken(): boolean {
    const token = localStorage.getItem(this.ACCESS_TOKEN_KEY);
    const expiryStr = localStorage.getItem(this.TOKEN_EXPIRY_KEY);

    if (!token || !expiryStr) {
      console.log('[AuthService] No token or expiry found in storage');
      return false;
    }

    const expiry = parseInt(expiryStr, 10);
    const isValid = Date.now() < expiry;
    
    if (!isValid) {
      console.log('[AuthService] Token expired', {
        expiry: new Date(expiry),
        now: new Date()
      });
    }

    return isValid;
  }

  private handleLoginError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Login failed';

    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else if (error.status === 401) {
      errorMessage = 'Invalid email or password';
    } else if (error.status === 403) {
      errorMessage = 'Account not activated. Please check your email for activation link.';
    } else if (error.status === 429) {
      errorMessage = 'Too many login attempts. Please try again later.';
    } else if (error.error?.error) {
      errorMessage = error.error.error;
    }

    console.error('Login error:', error);
    return throwError(() => new Error(errorMessage));
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

  private handleSetPasswordError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Failed to set password';

    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else if (error.error?.error) {
      errorMessage = error.error.error;
    } else if (error.status === 400) {
      errorMessage = 'Invalid password or activation link expired';
    }

    console.error('Set password error:', error);
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

