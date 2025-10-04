export interface ActivateAccountResponse {
  userId: string;
  message: string;
}

export interface ResendActivationRequest {
  email: string;
}

export interface ResendActivationResponse {
  message: string;
}

