import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/auth/register', pathMatch: 'full' },
  { 
    path: 'auth/register', 
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'auth/activate',
    loadComponent: () => import('./features/auth/activate/activate.component').then(m => m.ActivateComponent)
  },
  {
    path: 'auth/resend-activation',
    loadComponent: () => import('./features/auth/resend-activation/resend-activation.component').then(m => m.ResendActivationComponent)
  },
  { path: 'auth/login', redirectTo: '/auth/register' }, // Placeholder for future login
  { path: 'auth/set-password', redirectTo: '/auth/register' }, // Placeholder for Story 1.3
  { path: '**', redirectTo: '/auth/register' }
];
