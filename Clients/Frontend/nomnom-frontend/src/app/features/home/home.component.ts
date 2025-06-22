// src/app/features/home/home.component.ts

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HeaderComponent } from '../../layouts/header/header.component';
import { RestaurantsSectionComponent } from './restaurants-section/restaurants-section.component';
import { RestaurantService } from '../../core/services/restaurant.service';
import { RecommendationService } from '../../core/services/recommendation.service';
import { CartService } from '../../core/services/cart.service';
import { AuthService } from '../../core/services/auth.service';
import { RestaurantResponse } from '../../core/models/restaurant.models';
import { RecommendationResponse } from '../../core/models/recommendation.models';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    HeaderComponent,
    RestaurantsSectionComponent
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  restaurants: RestaurantResponse[] = [];
  popularRestaurants: RestaurantResponse[] = [];
  recommendations: RecommendationResponse[] = [];
  loading = true;
  error = '';

  // Состояние поиска
  currentSearchQuery = '';

  // Фильтры
  selectedCuisine = 'Все';
  cuisines = ['Все', 'Пицца', 'Бургеры', 'Суши', 'Итальянская', 'Восточная'];
  sortBy: 'rating' | 'name' | 'delivery' = 'rating';

  constructor(
    private restaurantService: RestaurantService,
    private recommendationService: RecommendationService,
    private cartService: CartService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  private loadData(): void {
    this.loading = true;
    this.error = '';

    // Загружаем рестораны
    this.restaurantService.getRestaurants().subscribe({
      next: (restaurants) => {
        this.restaurants = restaurants;
        this.popularRestaurants = this.getPopularRestaurants(restaurants);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading restaurants:', error);
        this.error = 'Ошибка загрузки ресторанов';
        this.loading = false;
      }
    });

    // Загружаем рекомендации для авторизованных пользователей
    const currentUser = this.authService.getCurrentUserValue();
    if (currentUser) {
      this.recommendationService.getRecommendations(currentUser.userId, 6).subscribe({
        next: (recommendations) => {
          this.recommendations = recommendations;
        },
        error: (error) => {
          console.error('Error loading recommendations:', error);
          // Не показываем ошибку пользователю, просто не отображаем рекомендации
          this.recommendations = [];
        }
      });
    }
  }

  private getPopularRestaurants(restaurants: RestaurantResponse[]): RestaurantResponse[] {
    return restaurants
      .filter(r => r.averageRating >= 4.0)
      .sort((a, b) => b.averageRating - a.averageRating)
      .slice(0, 6);
  }

  // Фильтрация ресторанов
  get filteredRestaurants(): RestaurantResponse[] {
    let filtered = [...this.restaurants];

    // Поиск по запросу
    if (this.currentSearchQuery) {
      const query = this.currentSearchQuery.toLowerCase();
      filtered = filtered.filter(restaurant =>
        restaurant.name.toLowerCase().includes(query) ||
        restaurant.address.toLowerCase().includes(query) ||
        this.getCuisineType(restaurant).toLowerCase().includes(query)
      );
    }

    // Фильтр по кухне
    if (this.selectedCuisine !== 'Все') {
      filtered = filtered.filter(restaurant =>
        this.getCuisineType(restaurant) === this.selectedCuisine
      );
    }

    // Сортировка
    return this.restaurantService.sortRestaurants(filtered, this.sortBy);
  }

  // Методы для UI
  onCuisineChange(cuisine: string): void {
    this.selectedCuisine = cuisine;
  }

  onSortChange(sortBy: 'rating' | 'name' | 'delivery'): void {
    this.sortBy = sortBy;
  }

  searchRestaurants(query: string): void {
    this.currentSearchQuery = query;

    // Если запрос пустой, показываем все рестораны
    if (!query.trim()) {
      return;
    }

    // Можно также делать отдельный API запрос для поиска
    this.restaurantService.searchRestaurants(query).subscribe({
      next: (restaurants) => {
        // Заменяем текущие рестораны результатами поиска
        this.restaurants = restaurants;
      },
      error: (error) => {
        console.error('Error searching restaurants:', error);
        // Оставляем текущие рестораны и применяем локальную фильтрацию
      }
    });
  }

  resetFilters(): void {
    this.selectedCuisine = 'Все';
    this.sortBy = 'rating';
    this.currentSearchQuery = '';

    // Перезагружаем оригинальные данные
    this.loadData();
  }

  // Добавление рекомендации в корзину
  addRecommendationToCart(recommendation: RecommendationResponse): void {
    // Здесь нужно получить полную информацию о товаре
    this.restaurantService.getMenuItem(recommendation.menuItemId).subscribe({
      next: (menuItem) => {
        const cartItem = this.cartService.createCartItemFromMenuItem(menuItem);

        // Проверяем совместимость с текущей корзиной
        if (!this.cartService.canAddFromRestaurant(menuItem.restaurantId)) {
          const confirmSwitch = confirm(
            'В корзине есть товары из другого ресторана. Очистить корзину и добавить этот товар?'
          );
          if (!confirmSwitch) return;
        }

        this.cartService.addItem(cartItem);
        alert(`${menuItem.name} добавлен в корзину`);
      },
      error: (error) => {
        console.error('Error adding recommendation to cart:', error);
        alert('Не удалось добавить товар в корзину');
      }
    });
  }

  // Утилиты
  getCuisineType(restaurant: RestaurantResponse): string {
    return this.restaurantService.getCuisineType(restaurant);
  }

  formatPrice(price: number): string {
    return this.restaurantService.formatPrice(price);
  }

  getRatingStars(rating: number): string {
    return this.restaurantService.getRatingStars(rating);
  }

  getDeliveryTime(restaurant: RestaurantResponse): string {
    return this.restaurantService.getDeliveryTime(restaurant);
  }

  isRestaurantOpen(restaurant: RestaurantResponse): boolean {
    return this.restaurantService.isRestaurantOpen(restaurant);
  }

  // Проверка авторизации
  get isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  get hasRecommendations(): boolean {
    return this.isAuthenticated && this.recommendations.length > 0;
  }

  get hasPopularRestaurants(): boolean {
    return this.popularRestaurants.length > 0;
  }

  // Обработка ошибок
  retryLoad(): void {
    this.error = '';
    this.loadData();
  }
}
