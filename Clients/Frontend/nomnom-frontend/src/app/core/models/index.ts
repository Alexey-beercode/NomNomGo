// src/app/core/models/index.ts

// Auth models
export * from './auth.models';
export * from './user.models';

// Restaurant models
export * from './restaurant.models';

// Order models
export * from './order.models';


// Common interfaces
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

export interface PaginatedResponse<T> {
  data: T[];
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
}

export interface ApiError {
  message: string;
  statusCode: number;
  details?: any;
}
