// src/app/core/services/recommendation.service.ts

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateReviewRequest,
  ReviewResponse,
  RecommendationResponse,
  RatingResponse,
  AddOrderRequest
} from '../models/recommendation.models';

@Injectable({
  providedIn: 'root'
})
export class RecommendationService {
  private readonly apiUrl = `${environment.recommendationApiUrl}/api`;

  constructor(private http: HttpClient) {}

  // Reviews
  createReview(request: CreateReviewRequest): Observable<ReviewResponse> {
    return this.http.post<ReviewResponse>(`${this.apiUrl}/reviews`, request);
  }

  getReviews(targetId: string, targetType: string): Observable<ReviewResponse[]> {
    return this.http.get<ReviewResponse[]>(`${this.apiUrl}/reviews/${targetType}/${targetId}`);
  }

  getUserReviews(userId: string): Observable<ReviewResponse[]> {
    return this.http.get<ReviewResponse[]>(`${this.apiUrl}/reviews/user/${userId}`);
  }

  updateReview(reviewId: string, request: CreateReviewRequest): Observable<ReviewResponse> {
    return this.http.put<ReviewResponse>(`${this.apiUrl}/reviews/${reviewId}`, request);
  }

  deleteReview(reviewId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/reviews/${reviewId}`);
  }

  // Ratings
  getRating(targetId: string, targetType: string): Observable<RatingResponse> {
    return this.http.get<RatingResponse>(`${this.apiUrl}/ratings/${targetType}/${targetId}`);
  }

  // Recommendations
  getRecommendations(userId: string, limit: number = 10): Observable<RecommendationResponse[]> {
    return this.http.get<RecommendationResponse[]>(`${this.apiUrl}/recommendations/user/${userId}?limit=${limit}`);
  }

  getPopularItems(restaurantId?: string, limit: number = 10): Observable<RecommendationResponse[]> {
    const url = restaurantId
      ? `${this.apiUrl}/recommendations/popular?restaurantId=${restaurantId}&limit=${limit}`
      : `${this.apiUrl}/recommendations/popular?limit=${limit}`;
    return this.http.get<RecommendationResponse[]>(url);
  }

  getSimilarItems(menuItemId: string, limit: number = 5): Observable<RecommendationResponse[]> {
    return this.http.get<RecommendationResponse[]>(`${this.apiUrl}/recommendations/similar/${menuItemId}?limit=${limit}`);
  }

  // Order tracking for ML
  addOrderToRecommendations(request: AddOrderRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/orders`, request);
  }

  // Utility methods
  getStarRating(rating: number): string {
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 >= 0.5;
    const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

    return '★'.repeat(fullStars) +
      (hasHalfStar ? '☆' : '') +
      '☆'.repeat(emptyStars);
  }

  getRatingColor(rating: number): string {
    if (rating >= 4.5) return '#4CAF50'; // Зеленый
    if (rating >= 4.0) return '#8BC34A'; // Светло-зеленый
    if (rating >= 3.5) return '#FFC107'; // Желтый
    if (rating >= 3.0) return '#FF9800'; // Оранжевый
    return '#F44336'; // Красный
  }

  formatReviewDate(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffInDays = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24));

    if (diffInDays === 0) return 'Сегодня';
    if (diffInDays === 1) return 'Вчера';
    if (diffInDays < 7) return `${diffInDays} дней назад`;
    if (diffInDays < 30) return `${Math.floor(diffInDays / 7)} недель назад`;

    return date.toLocaleDateString('ru-RU', {
      day: 'numeric',
      month: 'short',
      year: date.getFullYear() !== now.getFullYear() ? 'numeric' : undefined
    });
  }

  getSentimentIcon(sentiment?: string): string {
    switch (sentiment?.toLowerCase()) {
      case 'positive': return '😊';
      case 'negative': return '😞';
      case 'neutral': return '😐';
      default: return '';
    }
  }
}
