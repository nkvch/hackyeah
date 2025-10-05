import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // Default route redirects to login
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  
  // Auth Routes (Public)
  { 
    path: 'register', 
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then(m => m.LoginComponent)
  },
  {
    path: 'activate',
    loadComponent: () => import('./features/auth/activate/activate.component').then(m => m.ActivateComponent)
  },
  {
    path: 'resend-activation',
    loadComponent: () => import('./features/auth/resend-activation/resend-activation.component').then(m => m.ResendActivationComponent)
  },
  {
    path: 'set-password',
    loadComponent: () => import('./features/auth/set-password/set-password.component').then(m => m.SetPasswordComponent)
  },
  
  // Legacy auth/* routes for backwards compatibility
  { path: 'auth/register', redirectTo: '/register' },
  { path: 'auth/login', redirectTo: '/login' },
  { path: 'auth/activate', redirectTo: '/activate' },
  { path: 'auth/resend-activation', redirectTo: '/resend-activation' },
  { path: 'auth/set-password', redirectTo: '/set-password' },
  
  // Protected Routes (Require Authentication)
  {
    path: 'profile',
    loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent),
    canActivate: [authGuard] // Story 1.5: Profile management
  },
  {
    path: 'access-request',
    loadComponent: () => import('./features/access-request/my-access-request/my-access-request.component').then(m => m.MyAccessRequestComponent),
    canActivate: [authGuard] // Story 2.1: Access request
  },
  {
    path: 'reports/submit',
    loadComponent: () => import('./features/reporting/submit-report/submit-report.component').then(m => m.SubmitReportComponent),
    canActivate: [authGuard] // Story 1.4: Added auth guard
    // TODO: Add permission guard when Epic 2 is complete
    // canActivate: [authGuard, PermissionGuard('communication.reports.submit')]
  },
  
  // Wildcard route redirects to login
  { path: '**', redirectTo: '/login' }
];
