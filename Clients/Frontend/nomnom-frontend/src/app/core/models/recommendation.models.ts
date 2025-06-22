// src/app/core/models/recommendation.models.ts

export interface AddOrderRequest {
  userId: string;
  restaurantId: string;
  menuItemId: string;
}

export interface MenuItemDto {
  id: string;
  name: string;
  restaurantId: string;
  restaurantName: string;
}

export interface CreateReviewRequest {
  userId: string;
  targetId: string; // RestaurantId, MenuItemId, или CourierId
  targetType: string; // "Restaurant", "MenuItem", "Courier"
  rating: number;
  comment?: string;
}

export interface RatingResponse {
  targetId: string;
  targetType: string;
  averageRating: number;
  reviewCount: number;
  lastUpdated: string;
}

export interface RecommendationResponse {
  menuItemId: string;
  itemName: string;
  restaurantId: string;
  restaurantName: string;
  score: number; // Вероятность понравится
  reason: string; // "Popular", "Similar taste", etc.
}

export interface ReviewResponse {
  id: string;
  userId: string;
  targetId: string;
  targetType: string;
  rating: number;
  comment?: string;
  sentiment?: string;
  createdAt: string;
}
