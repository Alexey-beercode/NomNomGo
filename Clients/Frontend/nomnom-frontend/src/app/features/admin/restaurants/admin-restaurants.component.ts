// src/app/features/admin/restaurants/admin-restaurants.component.ts

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, takeUntil, finalize } from 'rxjs';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { RestaurantResponse, CreateRestaurantRequest } from '../../../core/models/restaurant.models';
import { InputComponent } from '../../../shared/components/input/input.component';

interface RestaurantFormData {
  name: string;
  address: string;
  phoneNumber: string;
}

interface RestaurantFilters {
  search: string;
  isActive?: boolean;
  sortBy: 'name' | 'rating' | 'reviewCount';
  sortOrder: 'asc' | 'desc';
}

interface FormState {
  isLoading: boolean;
  errors: { [key: string]: string };
  isDirty: boolean;
  isValid: boolean;
}

interface ModalState {
  isOpen: boolean;
  title: string;
  type: 'create' | 'edit' | 'delete';
  data: RestaurantResponse | null;
}

@Component({
  selector: 'app-admin-restaurants',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, InputComponent],
  templateUrl: './admin-restaurants.component.html',
  styleUrls: ['./admin-restaurants.component.css']
})
export class AdminRestaurantsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  restaurants: RestaurantResponse[] = [];
  filteredRestaurants: RestaurantResponse[] = [];
  loading = false;
  error = '';

  // Modal state
  modalState: ModalState = {
    isOpen: false,
    title: '',
    type: 'create',
    data: null
  };

  // Form state
  formState: FormState = {
    isLoading: false,
    errors: {},
    isDirty: false,
    isValid: false
  };

  // Form data
  restaurantForm: RestaurantFormData = {
    name: '',
    address: '',
    phoneNumber: ''
  };

  // Filters
  filters: RestaurantFilters = {
    search: '',
    isActive: undefined,
    sortBy: 'name',
    sortOrder: 'asc'
  };

  constructor(private restaurantService: RestaurantService) {}

  ngOnInit(): void {
    this.loadRestaurants();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadRestaurants(): void {
    this.loading = true;
    this.error = '';

    this.restaurantService.getRestaurants()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.loading = false)
      )
      .subscribe({
        next: (restaurants) => {
          this.restaurants = restaurants;
          this.applyFilters();
        },
        error: (error) => {
          console.error('Error loading restaurants:', error);
          this.error = 'Ошибка при загрузке ресторанов. Попробуйте еще раз.';
          this.restaurants = [];
          this.filteredRestaurants = [];
        }
      });
  }

  applyFilters(): void {
    let filtered = [...this.restaurants];

    // Search filter
    if (this.filters.search?.trim()) {
      const searchLower = this.filters.search.toLowerCase();
      filtered = filtered.filter(restaurant =>
        restaurant.name.toLowerCase().includes(searchLower) ||
        restaurant.address.toLowerCase().includes(searchLower) ||
        restaurant.phoneNumber.includes(searchLower)
      );
    }

    // Active status filter
    if (this.filters.isActive !== undefined) {
      filtered = filtered.filter(restaurant => restaurant.isActive === this.filters.isActive);
    }

    // Sorting
    if (this.filters.sortBy) {
      filtered.sort((a, b) => {
        let aValue: any;
        let bValue: any;

        switch (this.filters.sortBy) {
          case 'name':
            aValue = a.name.toLowerCase();
            bValue = b.name.toLowerCase();
            break;
          case 'rating':
            aValue = a.averageRating;
            bValue = b.averageRating;
            break;
          case 'reviewCount':
            aValue = a.reviewCount;
            bValue = b.reviewCount;
            break;
          default:
            return 0;
        }

        if (aValue < bValue) return this.filters.sortOrder === 'asc' ? -1 : 1;
        if (aValue > bValue) return this.filters.sortOrder === 'asc' ? 1 : -1;
        return 0;
      });
    }

    this.filteredRestaurants = filtered;
  }

  onSearchChange(value: string): void {
    this.filters.search = value;
    this.applyFilters();
  }

  onFilterChange(): void {
    this.applyFilters();
  }

  onSortChange(sortBy: string): void {
    if (this.filters.sortBy === sortBy) {
      this.filters.sortOrder = this.filters.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.filters.sortBy = sortBy as any;
      this.filters.sortOrder = 'asc';
    }
    this.applyFilters();
  }

  // Modal methods
  openCreateModal(): void {
    this.modalState = {
      isOpen: true,
      title: 'Добавить ресторан',
      type: 'create',
      data: null
    };
    this.resetForm();
  }

  openEditModal(restaurant: RestaurantResponse): void {
    this.modalState = {
      isOpen: true,
      title: 'Редактировать ресторан',
      type: 'edit',
      data: restaurant
    };
    this.populateForm(restaurant);
  }

  openDeleteModal(restaurant: RestaurantResponse): void {
    this.modalState = {
      isOpen: true,
      title: 'Удалить ресторан',
      type: 'delete',
      data: restaurant
    };
  }

  closeModal(): void {
    this.modalState.isOpen = false;
    this.resetForm();
  }

  // Form methods
  private resetForm(): void {
    this.restaurantForm = {
      name: '',
      address: '',
      phoneNumber: ''
    };
    this.formState = {
      isLoading: false,
      errors: {},
      isDirty: false,
      isValid: false
    };
  }

  private populateForm(restaurant: RestaurantResponse): void {
    this.restaurantForm = {
      name: restaurant.name,
      address: restaurant.address,
      phoneNumber: restaurant.phoneNumber
    };
    this.validateForm();
  }

  private validateForm(): void {
    const errors: { [key: string]: string } = {};

    if (!this.restaurantForm.name?.trim()) {
      errors['name'] = 'Название ресторана обязательно';
    }

    if (!this.restaurantForm.address?.trim()) {
      errors['address'] = 'Адрес обязателен';
    }

    if (!this.restaurantForm.phoneNumber?.trim()) {
      errors['phoneNumber'] = 'Номер телефона обязателен';
    } else if (!/^\+?[0-9\s\-\(\)]+$/.test(this.restaurantForm.phoneNumber)) {
      errors['phoneNumber'] = 'Некорректный формат номера телефона';
    }

    this.formState.errors = errors;
    this.formState.isValid = Object.keys(errors).length === 0;
  }

  onFormChange(): void {
    this.formState.isDirty = true;
    this.validateForm();
  }

  saveRestaurant(): void {
    if (!this.formState.isValid) return;

    this.formState.isLoading = true;

    const request: CreateRestaurantRequest = {
      name: this.restaurantForm.name,
      address: this.restaurantForm.address,
      phoneNumber: this.restaurantForm.phoneNumber
    };

    const operation = this.modalState.type === 'create'
      ? this.restaurantService.createRestaurant(request)
      : this.restaurantService.updateRestaurant(this.modalState.data!.id, request);

    operation
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.formState.isLoading = false)
      )
      .subscribe({
        next: (response) => {
          if (this.modalState.type === 'create') {
            this.restaurants.unshift(response);
          } else {
            const index = this.restaurants.findIndex(r => r.id === response.id);
            if (index !== -1) {
              this.restaurants[index] = response;
            }
          }
          this.applyFilters();
          this.closeModal();
        },
        error: (error) => {
          console.error('Error saving restaurant:', error);
          this.formState.errors['general'] = 'Ошибка при сохранении ресторана. Попробуйте еще раз.';
        }
      });
  }

  deleteRestaurant(): void {
    if (!this.modalState.data) return;

    const id = this.modalState.data.id;

    this.restaurantService.deleteRestaurant(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.restaurants = this.restaurants.filter(r => r.id !== id);
          this.applyFilters();
          this.closeModal();
        },
        error: (error) => {
          console.error('Error deleting restaurant:', error);
          this.error = 'Ошибка при удалении ресторана';
        }
      });
  }

  refreshData(): void {
    this.loadRestaurants();
  }

  // Utility methods
  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
      minimumFractionDigits: 0,
      maximumFractionDigits: 2
    }).format(amount);
  }

  formatDate(dateString: string): string {
    return new Intl.DateTimeFormat('ru-RU', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    }).format(new Date(dateString));
  }

  getStatusClass(isActive: boolean): string {
    return isActive ? 'status-active' : 'status-inactive';
  }

  getStatusText(isActive: boolean): string {
    return isActive ? 'Активен' : 'Неактивен';
  }

  getSortIcon(sortBy: string): string {
    if (this.filters.sortBy !== sortBy) return '↕️';
    return this.filters.sortOrder === 'asc' ? '↑' : '↓';
  }

  trackRestaurant(index: number, restaurant: RestaurantResponse): string {
    return restaurant.id;
  }
}
