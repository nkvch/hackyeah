import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * JWT HTTP Interceptor
 * - Adds Authorization header with access token to all requests
 * - Handles 401 errors by attempting token refresh
 * - Redirects to login on token refresh failure
 */
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  // Skip adding token for auth endpoints (login, register, etc.)
  const authEndpoints = ['/api/auth/login', '/api/auth/register', '/api/auth/activate', '/api/auth/resend-activation', '/api/auth/set-password', '/api/auth/password-policy'];
  const isAuthEndpoint = authEndpoints.some(endpoint => req.url.includes(endpoint));

  // Get access token
  const accessToken = authService.getAccessToken();

  // Clone request and add authorization header if token exists and not auth endpoint
  let clonedReq = req;
  if (accessToken && !isAuthEndpoint) {
    clonedReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${accessToken}`
      }
    });
  }

  // Handle the request and catch 401 errors
  return next(clonedReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // If 401 Unauthorized and not already on login/auth endpoint
      if (error.status === 401 && !isAuthEndpoint) {
        // Check if this is a refresh token request
        if (req.url.includes('/api/auth/refresh-token')) {
          // Refresh token is invalid, clear session and redirect to login
          console.error('Refresh token expired or invalid. Logging out...');
          // Don't call authService methods here to avoid circular dependencies
          return throwError(() => error);
        }

        // Try to refresh the token
        console.log('Access token expired. Attempting to refresh...');
        return authService.refreshToken().pipe(
          switchMap((response) => {
            // Retry the original request with new token
            console.log('Token refreshed successfully. Retrying request...');
            const retryReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${response.accessToken}`
              }
            });
            return next(retryReq);
          }),
          catchError((refreshError) => {
            // Refresh failed, clear session (handled in AuthService)
            console.error('Token refresh failed. User will be redirected to login.');
            return throwError(() => refreshError);
          })
        );
      }

      // For other errors, just pass them through
      return throwError(() => error);
    })
  );
};

