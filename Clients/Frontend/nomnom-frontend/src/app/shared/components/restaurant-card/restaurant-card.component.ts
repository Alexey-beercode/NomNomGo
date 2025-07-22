// src/app/shared/components/restaurant-card/restaurant-card.component.ts
import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { RatingComponent } from '../rating/rating.component';
import { RestaurantResponse } from '../../../core/models';
import { RestaurantService } from '../../../core/services/restaurant.service';

@Component({
  selector: 'app-restaurant-card',
  standalone: true,
  imports: [CommonModule, RatingComponent, RouterModule],
  templateUrl: 'restaurant-card.component.html',
  styleUrls: ['./restaurant-card.component.css']
})
export class RestaurantCardComponent implements OnInit {
  @Input() restaurant!: RestaurantResponse;

  // Кешированные значения для предотвращения повторных вычислений
  private cachedDeliveryTime: string | null = null;
  private cachedDeliveryFee: string | null = null;
  private cachedCuisineType: string | null = null;
  private cachedTags: string[] | null = null;

  // Дефолтное изображение для ресторанов
  private defaultRestaurantImage = 'https://foni.papik.pro/uploads/posts/2024-09/foni-papik-pro-l126-p-kartinki-yeda-na-prozrachnom-fone-2.png';

  constructor(private restaurantService: RestaurantService) {}

  ngOnInit() {
    // Инициализируем кешированные значения один раз
    this.initializeCachedValues();
  }

  private initializeCachedValues() {
    if (this.restaurant) {
      // Кешируем все значения, которые могут изменяться при каждом вызове
      this.cachedDeliveryTime = '40 мин'; // Фиксированное время доставки
      this.cachedDeliveryFee = this.calculateDeliveryFee();
      this.cachedCuisineType = this.restaurantService.getCuisineType(this.restaurant);
      this.cachedTags = this.calculateRestaurantTags();
    }
  }

  // Метод для получения изображения с fallback
  getRestaurantImage(): string {
    return this.restaurant?.imageUrl || this.defaultRestaurantImage;
  }

  // Обработчик ошибки загрузки изображения
  onImageError(event: any) {
    event.target.src = this.defaultRestaurantImage;
  }

  // Получение типа кухни (кешированное)
  getCuisineType(): string {
    return this.cachedCuisineType || this.restaurantService.getCuisineType(this.restaurant);
  }

  // Получение времени доставки (кешированное и фиксированное)
  getDeliveryTime(): string {
    return this.cachedDeliveryTime || '40 мин';
  }

  // Получение стоимости доставки (кешированное)
  getDeliveryFee(): string {
    return this.cachedDeliveryFee || this.calculateDeliveryFee();
  }

  private calculateDeliveryFee(): string {
    const fee = this.restaurantService.getDeliveryFee(this.restaurant, 0);
    return fee === 0 ? 'Бесплатно' : `от ${fee} руб`;
  }

  // Проверка открыт ли ресторан
  isOpen(): boolean {
    return this.restaurantService.isRestaurantOpen(this.restaurant);
  }

  // Получение тегов ресторана (кешированное)
  getRestaurantTags(): string[] {
    return this.cachedTags || this.calculateRestaurantTags();
  }

  private calculateRestaurantTags(): string[] {
    const tags: string[] = [];

    // Добавляем теги на основе характеристик ресторана
    if (this.restaurant.averageRating >= 4.5) {
      tags.push('Топ рейтинг');
    }

    if (this.getDeliveryFee() === 'Бесплатно') {
      tags.push('Бесплатная доставка');
    }

    // Можно добавить другие теги на основе данных
    if (this.restaurant.reviewCount > 100) {
      tags.push('Популярный');
    }

    // Ограничиваем количество тегов
    return tags.slice(0, 2);
  }

  // Форматирование рейтинга
  formatRating(rating: number): string {
    return rating.toFixed(1);
  }

  // Переход к ресторану (навигация через routerLink)
  navigateToRestaurant(): void {
    // Навигация обрабатывается через routerLink в шаблоне
  }
}
