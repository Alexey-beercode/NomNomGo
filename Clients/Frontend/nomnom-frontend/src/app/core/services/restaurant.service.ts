// src/app/core/services/restaurant.service.ts

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  RestaurantResponse,
  CreateRestaurantRequest,
  MenuItemResponse,
  CreateMenuItemRequest,
  CategoryResponse,
  RatingResponse
} from '../models/restaurant.models';

@Injectable({
  providedIn: 'root'
})
export class RestaurantService {
  private readonly apiUrl = `${environment.menuOrderApiUrl}/api`;

  constructor(private http: HttpClient) {}

  // Restaurants
  getRestaurants(): Observable<RestaurantResponse[]> {
    return this.http.get<RestaurantResponse[]>(`${this.apiUrl}/restaurants`);
  }

  getRestaurant(id: string): Observable<RestaurantResponse> {
    return this.http.get<RestaurantResponse>(`${this.apiUrl}/restaurants/${id}`);
  }

  createRestaurant(request: CreateRestaurantRequest): Observable<RestaurantResponse> {
    return this.http.post<RestaurantResponse>(`${this.apiUrl}/restaurants`, request);
  }

  updateRestaurant(id: string, request: CreateRestaurantRequest): Observable<RestaurantResponse> {
    return this.http.put<RestaurantResponse>(`${this.apiUrl}/restaurants/${id}`, request);
  }

  deleteRestaurant(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/restaurants/${id}`);
  }

  // Menu Items
  getMenu(restaurantId: string): Observable<MenuItemResponse[]> {
    return this.http.get<MenuItemResponse[]>(`${this.apiUrl}/menu-items/restaurant/${restaurantId}`);
  }

  getMenuItem(id: string): Observable<MenuItemResponse> {
    return this.http.get<MenuItemResponse>(`${this.apiUrl}/menu-items/${id}`);
  }

  createMenuItem(request: CreateMenuItemRequest): Observable<MenuItemResponse> {
    return this.http.post<MenuItemResponse>(`${this.apiUrl}/menu-items`, request);
  }

  updateMenuItem(id: string, request: CreateMenuItemRequest): Observable<MenuItemResponse> {
    return this.http.put<MenuItemResponse>(`${this.apiUrl}/menu-items/${id}`, request);
  }

  deleteMenuItem(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/menu-items/${id}`);
  }

  // Categories
  getCategories(): Observable<CategoryResponse[]> {
    return this.http.get<CategoryResponse[]>(`${this.apiUrl}/menu-items/categories`);
  }

  getCategoriesByRestaurant(restaurantId: string): Observable<CategoryResponse[]> {
    return this.http.get<CategoryResponse[]>(`${this.apiUrl}/restaurants/${restaurantId}/categories`);
  }

  // Search
  searchRestaurants(query: string): Observable<RestaurantResponse[]> {
    return this.http.get<RestaurantResponse[]>(`${this.apiUrl}/restaurants/search?q=${encodeURIComponent(query)}`);
  }

  searchMenuItems(query: string, restaurantId?: string): Observable<MenuItemResponse[]> {
    const url = restaurantId
      ? `${this.apiUrl}/menu-items/search?q=${encodeURIComponent(query)}&restaurantId=${restaurantId}`
      : `${this.apiUrl}/menu-items/search?q=${encodeURIComponent(query)}`;
    return this.http.get<MenuItemResponse[]>(url);
  }

  // Ratings
  getRestaurantRating(restaurantId: string): Observable<RatingResponse> {
    return this.http.get<RatingResponse>(`${this.apiUrl}/restaurants/${restaurantId}/rating`);
  }

  getMenuItemRating(menuItemId: string): Observable<RatingResponse> {
    return this.http.get<RatingResponse>(`${this.apiUrl}/menu-items/${menuItemId}/rating`);
  }

  // Utility methods for UI
  formatPrice(price: number): string {
    return `${price.toFixed(2)} ₽`;
  }

  getDeliveryTime(restaurant: RestaurantResponse): string {
    // Простая логика для времени доставки
    const baseTime = 20;
    const randomAdd = Math.floor(Math.random() * 20);
    return `${baseTime + randomAdd}-${baseTime + randomAdd + 15} мин`;
  }

  getDeliveryFee(restaurant: RestaurantResponse, orderAmount: number): number {
    // Простая логика для стоимости доставки
    const freeDeliveryThreshold = 500;
    return orderAmount >= freeDeliveryThreshold ? 0 : 99;
  }

  isRestaurantOpen(restaurant: RestaurantResponse): boolean {
    // Простая проверка - можно расширить логикой рабочих часов
    return restaurant.isActive;
  }

  getCuisineType(restaurant: RestaurantResponse): string {
    // Можно добавить поле cuisineType в модель или определять по названию
    const cuisineKeywords: { [key: string]: string } = {
      'pizza': 'Пицца',
      'burger': 'Бургеры',
      'sushi': 'Суши',
      'pasta': 'Итальянская',
      'дoner': 'Восточная',
      'кафе': 'Кафе',
      'ресторан': 'Ресторан'
    };

    const name = restaurant.name.toLowerCase();
    for (const [keyword, cuisine] of Object.entries(cuisineKeywords)) {
      if (name.includes(keyword)) {
        return cuisine;
      }
    }
    return 'Разное';
  }

  getRatingStars(rating: number): string {
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 >= 0.5;
    const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

    return '★'.repeat(fullStars) +
      (hasHalfStar ? '☆' : '') +
      '☆'.repeat(emptyStars);
  }

  sortRestaurants(restaurants: RestaurantResponse[], sortBy: 'rating' | 'name' | 'delivery'): RestaurantResponse[] {
    return [...restaurants].sort((a, b) => {
      switch (sortBy) {
        case 'rating':
          return b.averageRating - a.averageRating;
        case 'name':
          return a.name.localeCompare(b.name);
        case 'delivery':
          // Сортировка по времени доставки (если добавите это поле)
          return a.name.localeCompare(b.name); // Заглушка
        default:
          return 0;
      }
    });
  }

  filterRestaurants(restaurants: RestaurantResponse[], filters: {
    minRating?: number;
    cuisine?: string;
    isOpen?: boolean;
  }): RestaurantResponse[] {
    return restaurants.filter(restaurant => {
      if (filters.minRating && restaurant.averageRating < filters.minRating) {
        return false;
      }
      if (filters.cuisine && this.getCuisineType(restaurant) !== filters.cuisine) {
        return false;
      }
      if (filters.isOpen !== undefined && this.isRestaurantOpen(restaurant) !== filters.isOpen) {
        return false;
      }
      return true;
    });
  }
}
