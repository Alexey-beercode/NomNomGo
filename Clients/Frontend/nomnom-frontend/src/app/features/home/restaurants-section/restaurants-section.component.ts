// src/app/features/home/restaurants-section/restaurants-section.component.ts
import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RestaurantCardComponent } from '../../../shared/components/restaurant-card/restaurant-card.component';
import { RestaurantResponse } from '../../../core/models';
import { RestaurantService } from '../../../core/services/restaurant.service';

@Component({
  selector: 'app-restaurants-section',
  standalone: true,
  imports: [CommonModule, RestaurantCardComponent],
  templateUrl: './restaurants-section.component.html',
  styleUrls: ['./restaurants-section.component.css']
})
export class RestaurantsSectionComponent implements OnInit {
  @Input() restaurants: RestaurantResponse[] = [];

  // Категории будут загружаться из API
  categories: string[] = ['Все'];
  selectedCategory = 'Все';

  // Поисковый запрос
  searchQuery = '';

  // Сортировка
  sortBy: 'rating' | 'name' | 'delivery' = 'rating';

  constructor(private restaurantService: RestaurantService) {}

  ngOnInit() {
    this.loadCategories();
  }

  // Загрузка категорий из API
  private loadCategories() {
    this.restaurantService.getCategories().subscribe({
      next: (categories) => {
        this.categories = ['Все', ...categories.map(c => c.name)];
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        // Fallback к статичным категориям
        this.categories = [
          'Все', 'Бургеры', 'Суши', 'Пицца', 'Вок',
          'Паста', 'Завтраки', 'Обеды', 'Грузия', 'Италия'
        ];
      }
    });
  }

  selectCategory(category: string) {
    this.selectedCategory = category;
  }

  onSearch(query: string) {
    this.searchQuery = query;
  }

  onSortChange(sortBy: 'rating' | 'name' | 'delivery') {
    this.sortBy = sortBy;
  }

  // Фильтрованные рестораны
  get filteredRestaurants(): RestaurantResponse[] {
    let filtered = [...this.restaurants];

    // Поиск
    if (this.searchQuery.trim()) {
      const query = this.searchQuery.toLowerCase();
      filtered = filtered.filter(restaurant =>
        restaurant.name.toLowerCase().includes(query) ||
        restaurant.address.toLowerCase().includes(query)
      );
    }

    // Фильтрация по категории
    if (this.selectedCategory !== 'Все') {
      filtered = filtered.filter(restaurant =>
        this.getCuisineType(restaurant) === this.selectedCategory
      );
    }

    // Сортировка
    return this.restaurantService.sortRestaurants(filtered, this.sortBy);
  }

  // Определение типа кухни ресторана
  private getCuisineType(restaurant: RestaurantResponse): string {
    return this.restaurantService.getCuisineType(restaurant);
  }
}
