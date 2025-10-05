import { Component, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslationService, Language } from '../../../core/services/translation.service';

@Component({
  selector: 'app-language-switcher',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './language-switcher.component.html',
  styleUrls: ['./language-switcher.component.scss'],
})
export class LanguageSwitcherComponent {
  currentLanguage: Language = 'en';

  constructor(private translationService: TranslationService) {
    // React to language changes
    effect(() => {
      this.currentLanguage = this.translationService.language();
    });
  }

  toggleLanguage(): void {
    const newLanguage: Language = this.currentLanguage === 'en' ? 'pl' : 'en';
    this.translationService.setLanguage(newLanguage);
  }

  getLanguageLabel(): string {
    return this.currentLanguage === 'en' ? 'EN' : 'PL';
  }

  getOtherLanguageLabel(): string {
    return this.currentLanguage === 'en' ? 'Polski' : 'English';
  }
}
