export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number; // Seconds until token expires
  user: UserInfo;
  requiresPasswordChange: boolean;
}

export interface UserInfo {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  userType: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface LogoutRequest {
  refreshToken: string;
}

