// src/app/core/models/auth.models.ts

export interface RegisterRequest {
  email: string;
  username: string;
  password: string;
  phoneNumber?: string;
}

export interface LoginRequest {
  login: string;
  password: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

// Responses
export interface CurrentUserResponse {
  userId: string;
  username: string;
  email: string;
  phoneNumber?: string;
  isBlocked: boolean;
  blockedUntil?: string;
  roles: string[];
  permissions: string[];
  createdAt: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  username: string;
  roles: string[];
  expiresAt: string;
}

export interface RegisterResponse {
  userId: string;
  username: string;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

// Интерфейс для локального использования (совместимость)
export interface CurrentUser {
  userId: string;
  username: string;
  email: string;
  phoneNumber?: string;
  roles: string[];
  createdAt: string;
}
