import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LanguageSwitcherComponent } from '../language-switcher/language-switcher.component';
import { TranslationService } from '../../../core/services/translation.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, LanguageSwitcherComponent],
  templateUrl: './app-header.component.html',
  styleUrls: ['./app-header.component.scss'],
})
export class AppHeaderComponent implements OnInit {
  protected isLoggedIn = signal(false);

  constructor(
    private authService: AuthService,
    private router: Router,
    protected t: TranslationService,
  ) {}

  ngOnInit() {
    this.isLoggedIn.set(this.authService.isAuthenticated());
  }

  onLogout() {
    this.authService.logout().subscribe({
      next: () => {
        // Logout successful, navigation handled by AuthService
        console.log('Logout successful from navigation');
      },
      error: (error) => {
        // Even if logout API fails, session is cleared by AuthService
        console.error('Logout error from navigation:', error);
      },
    });
  }
}
