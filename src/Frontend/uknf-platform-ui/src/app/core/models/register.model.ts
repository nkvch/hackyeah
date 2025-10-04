export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  pesel: string;
}

export interface RegisterResponse {
  userId: string;
  message: string;
}

export interface ApiError {
  error: string;
}

