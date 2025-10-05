export interface AccessRequestDto {
  id: string;
  userId: string;
  status: string;
  submittedDate?: string;
  createdDate: string;
  
  // User data
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  peselMasked: string;
  
  isEditable: boolean;
  isVisibleToReviewers: boolean;
}

