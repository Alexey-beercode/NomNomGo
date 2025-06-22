// src/app/core/models/user.models.ts

export interface UserDetailResponse {
  userId: string;
  username: string;
  email: string;
  phoneNumber?: string;
  isBlocked: boolean;
  blockedUntil?: string;
  roles: string[];
  createdAt: string;
  updatedAt: string;
}

export interface UserListItem {
  userId: string;
  username: string;
  email: string;
  isBlocked: boolean;
  roles: string[];
  createdAt: string;
}

export interface UpdateProfileRequest {
  username: string;
  email: string;
  phoneNumber?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}
