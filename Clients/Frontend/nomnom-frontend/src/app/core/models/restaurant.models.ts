// src/app/core/models/restaurant.models.ts

export interface RestaurantResponse {
  id: string;
  name: string;
  address: string;
  phoneNumber: string;
  isActive: boolean;
  averageRating: number;
  reviewCount: number;
  // Дополнительные поля для UI
  imageUrl?: string;
  delivery?: string;
}

export interface CreateRestaurantRequest {
  name: string;
  address: string;
  phoneNumber: string;
}

export interface CategoryResponse {
  id: string;
  name: string;
  itemsCount: number;
}

export interface MenuItemResponse {
  id: string;
  restaurantId: string;
  restaurantName: string;
  categoryId: string;
  categoryName: string;
  name: string;
  description: string;
  price: number;
  isAvailable: boolean;
  imageUrl?: string;
  averageRating: number;
  reviewCount: number;
  // Дополнительные поля для UI
  image?: string;
  discount?: number;
  oldPrice?: number;
}

export interface CreateMenuItemRequest {
  restaurantId: string;
  categoryId: string;
  name: string;
  description: string;
  price: number;
  imageUrl?: string;
}

export interface RatingResponse {
  targetId: string;
  targetType: string;
  averageRating: number;
  reviewCount: number;
  lastUpdated: string;
}
