/**
 * User profile data transfer object
 */
export interface UserProfileDto {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  pendingEmail: string | null;
  phoneNumber: string;
  peselLast4: string;
  userType: string;
  createdDate: string;
  lastLoginDate: string | null;
}

/**
 * Request to update user profile
 */
export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  phoneNumber: string;
  email: string;
}

/**
 * Response after updating profile
 */
export interface UpdateProfileResponse {
  success: boolean;
  message: string;
  emailChangeRequiresConfirmation: boolean;
  updatedProfile: UserProfileDto;
}

