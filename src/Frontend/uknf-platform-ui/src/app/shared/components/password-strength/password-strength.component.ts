import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PasswordStrength, PasswordStrengthResult, PasswordPolicy } from '../../../core/models/password.model';

/**
 * Reusable password strength indicator component
 * Displays visual meter and feedback for password quality
 */
@Component({
  selector: 'app-password-strength',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './password-strength.component.html',
  styleUrls: ['./password-strength.component.scss']
})
export class PasswordStrengthComponent implements OnChanges {
  @Input() password: string = '';
  @Input() passwordPolicy?: PasswordPolicy;

  strengthResult: PasswordStrengthResult = {
    score: 0,
    strength: PasswordStrength.Weak,
    feedback: []
  };

  ngOnChanges(): void {
    this.strengthResult = this.calculateStrength(this.password);
  }

  get strengthLabel(): string {
    switch (this.strengthResult.strength) {
      case PasswordStrength.Weak:
        return 'Weak';
      case PasswordStrength.Fair:
        return 'Fair';
      case PasswordStrength.Good:
        return 'Good';
      case PasswordStrength.Strong:
        return 'Strong';
      default:
        return '';
    }
  }

  get strengthClass(): string {
    switch (this.strengthResult.strength) {
      case PasswordStrength.Weak:
        return 'weak';
      case PasswordStrength.Fair:
        return 'fair';
      case PasswordStrength.Good:
        return 'good';
      case PasswordStrength.Strong:
        return 'strong';
      default:
        return '';
    }
  }

  get strengthColor(): string {
    switch (this.strengthResult.strength) {
      case PasswordStrength.Weak:
        return '#ef4444'; // red-500
      case PasswordStrength.Fair:
        return '#f97316'; // orange-500
      case PasswordStrength.Good:
        return '#eab308'; // yellow-500
      case PasswordStrength.Strong:
        return '#22c55e'; // green-500
      default:
        return '#94a3b8'; // gray-400
    }
  }

  private calculateStrength(password: string): PasswordStrengthResult {
    if (!password) {
      return { score: 0, strength: PasswordStrength.Weak, feedback: [] };
    }

    let score = 0;
    const feedback: string[] = [];

    // Length contribution (up to 40 points)
    score += Math.min(password.length * 4, 40);
    if (password.length < 8) {
      feedback.push('Add more characters');
    }

    // Character diversity (up to 30 points)
    if (/[A-Z]/.test(password)) {
      score += 10;
    } else {
      feedback.push('Add uppercase letters');
    }

    if (/[a-z]/.test(password)) {
      score += 10;
    } else {
      feedback.push('Add lowercase letters');
    }

    if (/[0-9]/.test(password)) {
      score += 5;
    } else {
      feedback.push('Add numbers');
    }

    if (/[^A-Za-z0-9]/.test(password)) {
      score += 5;
    } else {
      feedback.push('Add special characters');
    }

    // Uniqueness (up to 20 points)
    const uniqueChars = new Set(password).size;
    score += Math.min(uniqueChars * 2, 20);
    if (uniqueChars < 5) {
      feedback.push('Use more unique characters');
    }

    // Penalties
    if (this.containsCommonPattern(password)) {
      score -= 10;
      feedback.push('Avoid common patterns');
    }
    if (this.containsRepeatingChars(password)) {
      score -= 10;
      feedback.push('Avoid repeating characters');
    }
    if (this.containsSequentialChars(password)) {
      score -= 10;
      feedback.push('Avoid sequential characters');
    }

    score = Math.max(0, Math.min(score, 100));

    const strength = this.getStrengthLevel(score);

    return { score, strength, feedback };
  }

  private getStrengthLevel(score: number): PasswordStrength {
    if (score <= 40) return PasswordStrength.Weak;
    if (score <= 60) return PasswordStrength.Fair;
    if (score <= 80) return PasswordStrength.Good;
    return PasswordStrength.Strong;
  }

  private containsCommonPattern(password: string): boolean {
    const commonPatterns = [
      'password', '123456', 'qwerty', 'admin', 'welcome',
      'letmein', 'monkey', 'dragon', 'master', 'sunshine'
    ];
    const lowerPassword = password.toLowerCase();
    return commonPatterns.some(pattern => lowerPassword.includes(pattern));
  }

  private containsRepeatingChars(password: string): boolean {
    for (let i = 0; i < password.length - 2; i++) {
      if (password[i] === password[i + 1] && password[i] === password[i + 2]) {
        return true;
      }
    }
    return false;
  }

  private containsSequentialChars(password: string): boolean {
    const lower = password.toLowerCase();
    for (let i = 0; i < lower.length - 2; i++) {
      const charCode1 = lower.charCodeAt(i);
      const charCode2 = lower.charCodeAt(i + 1);
      const charCode3 = lower.charCodeAt(i + 2);
      
      if (charCode1 + 1 === charCode2 && charCode1 + 2 === charCode3) {
        return true;
      }
      if (charCode1 - 1 === charCode2 && charCode1 - 2 === charCode3) {
        return true;
      }
    }
    return false;
  }
}

