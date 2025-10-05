/**
 * Request to set initial password for newly activated account
 */
export interface SetPasswordRequest {
  token: string;
  password: string;
  passwordConfirmation: string;
}

/**
 * Response after successfully setting password
 */
export interface SetPasswordResponse {
  userId: string;
  message: string;
  redirectUrl: string;
}

/**
 * Password policy configuration from backend
 */
export interface PasswordPolicy {
  minLength: number;
  maxLength: number;
  requireUppercase: boolean;
  requireLowercase: boolean;
  requireDigit: boolean;
  requireSpecialChar: boolean;
  minUniqueChars: number;
}

/**
 * Password strength levels
 */
export enum PasswordStrength {
  Weak = 0,
  Fair = 1,
  Good = 2,
  Strong = 3
}

/**
 * Password strength result with score and feedback
 */
export interface PasswordStrengthResult {
  score: number;
  strength: PasswordStrength;
  feedback: string[];
}

