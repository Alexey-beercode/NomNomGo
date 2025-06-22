// src/app/features/admin/menu-items/admin-menu-items.component.ts

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, takeUntil, finalize } from 'rxjs';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { MenuItemResponse, CategoryResponse, RestaurantResponse, CreateMenuItemRequest } from '../../../core/models/restaurant.models';
import { InputComponent } from '../../../shared/components/input/input.component';
import {
  AdminMenuItem,
  MenuItemFormData,
  MenuItemFilters,
  FormState,
  ModalState
} from '../models/admin.models';

@Component({
  selector: 'app-admin-menu-items',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, InputComponent],
  template: `
    <div class="admin-menu-items">
      <!-- Header -->
      <div class="page-header">
        <div class="header-left">
          <h1>Управление блюдами</h1>
          <p class="page-description">Создавайте и редактируйте блюда для ресторанов</p>
        </div>
        <div class="header-actions">
          <button class="btn btn-primary" (click)="openCreateModal()">
            <span class="btn-icon">➕</span>
            Добавить блюдо
          </button>
        </div>
      </div>

      <!-- Filters -->
      <div class="filters-section">
        <div class="search-container">
          <app-input
            placeholder="Поиск блюд..."
            [value]="filters.search || ''"
            (valueChange)="onSearchChange($event)">
          </app-input>
        </div>

        <div class="filters-container">
          <div class="filter-group">
            <label class="filter-label">Ресторан:</label>
            <select class="filter-select" [(ngModel)]="filters.restaurantId" (change)="onFilterChange()">
              <option [ngValue]="undefined">Все рестораны</option>
              <option *ngFor="let restaurant of restaurants" [ngValue]="restaurant.id">
                {{ restaurant.name }}
              </option>
            </select>
          </div>

          <div class="filter-group">
            <label class="filter-label">Категория:</label>
            <select class="filter-select" [(ngModel)]="filters.categoryId" (change)="onFilterChange()">
              <option [ngValue]="undefined">Все категории</option>
              <option *ngFor="let category of categories" [ngValue]="category.id">
                {{ category.name }}
              </option>
            </select>
          </div>

          <div class="filter-group">
            <label class="filter-label">Статус:</label>
            <select class="filter-select" [(ngModel)]="filters.isAvailable" (change)="onFilterChange()">
              <option [ngValue]="undefined">Все</option>
              <option [ngValue]="true">Доступные</option>
              <option [ngValue]="false">Недоступные</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Content -->
      <div class="content-section" *ngIf="!loading && !error">
        <div class="menu-items-grid">
          <div class="menu-item-card" *ngFor="let item of filteredMenuItems; trackBy: trackMenuItem">
            <div class="card-image">
              <img [src]="getImageUrl(item.imageUrl)"
                   [alt]="item.name"
                   class="item-image"
                   (error)="onImageError($event)">
              <div class="card-badges">
                <span class="status-badge" [class]="getStatusClass(item.isAvailable)">
                  {{ getStatusText(item.isAvailable) }}
                </span>
              </div>
            </div>

            <div class="card-content">
              <h3 class="item-name">{{ item.name }}</h3>
              <p class="item-description">{{ item.description }}</p>

              <div class="item-meta">
                <div class="restaurant-name">{{ item.restaurantName }}</div>
                <div class="category-name">{{ item.categoryName }}</div>
              </div>

              <div class="item-stats">
                <div class="stat-item">
                  <span class="stat-value">{{ formatCurrency(item.price) }}</span>
                  <span class="stat-label">Цена</span>
                </div>
                <div class="stat-item">
                  <span class="stat-value">{{ item.averageRating | number:'1.1-1' }}</span>
                  <span class="stat-label">Рейтинг ⭐</span>
                </div>
                <div class="stat-item">
                  <span class="stat-value">{{ item.totalOrders }}</span>
                  <span class="stat-label">Заказов</span>
                </div>
              </div>
            </div>

            <div class="card-actions">
              <button class="action-btn edit" (click)="openEditModal(item)" title="Редактировать">
                ✏️
              </button>
              <button class="action-btn toggle"
                      (click)="toggleItemStatus(item)"
                      [title]="item.isAvailable ? 'Сделать недоступным' : 'Сделать доступным'">
                {{ item.isAvailable ? '⏸️' : '▶️' }}
              </button>
              <button class="action-btn delete" (click)="openDeleteModal(item)" title="Удалить">
                🗑️
              </button>
            </div>
          </div>
        </div>

        <!-- Empty State -->
        <div class="empty-state" *ngIf="filteredMenuItems.length === 0">
          <div class="empty-icon">🍽️</div>
          <h3>Блюда не найдены</h3>
          <p>Добавьте первое блюдо в меню ресторана</p>
          <button class="btn btn-primary" (click)="openCreateModal()">
            Добавить блюдо
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div class="loading-container" *ngIf="loading">
        <div class="loading-spinner"></div>
        <p>Загружаем блюда...</p>
      </div>

      <!-- Error State -->
      <div class="error-container" *ngIf="error && !loading">
        <div class="error-message">
          <span class="error-icon">⚠️</span>
          {{ error }}
        </div>
        <button class="btn btn-secondary" (click)="loadData()">Попробовать снова</button>
      </div>
    </div>

    <!-- Modal -->
    <div class="modal-overlay" *ngIf="modalState.isOpen" (click)="closeModal()">
      <div class="modal-content" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h2>{{ modalState.title }}</h2>
          <button class="modal-close" (click)="closeModal()">×</button>
        </div>

        <div class="modal-body" *ngIf="modalState.type !== 'delete'">
          <form (ngSubmit)="saveMenuItem()">
            <div class="form-group">
              <label>Ресторан *</label>
              <select class="form-select" [(ngModel)]="menuItemForm.restaurantId" name="restaurantId" (change)="onFormChange()">
                <option value="">Выберите ресторан</option>
                <option *ngFor="let restaurant of restaurants" [value]="restaurant.id">
                  {{ restaurant.name }}
                </option>
              </select>
              <div class="error-message" *ngIf="formState.errors['restaurantId']">
                {{ formState.errors['restaurantId'] }}
              </div>
            </div>

            <div class="form-group">
              <label>Категория *</label>
              <select class="form-select" [(ngModel)]="menuItemForm.categoryId" name="categoryId" (change)="onFormChange()">
                <option value="">Выберите категорию</option>
                <option *ngFor="let category of categories" [value]="category.id">
                  {{ category.name }}
                </option>
              </select>
              <div class="error-message" *ngIf="formState.errors['categoryId']">
                {{ formState.errors['categoryId'] }}
              </div>
            </div>

            <div class="form-group">
              <label>Название блюда *</label>
              <app-input
                placeholder="Введите название блюда"
                [value]="menuItemForm.name"
                (valueChange)="menuItemForm.name = $event; onFormChange()">
              </app-input>
              <div class="error-message" *ngIf="formState.errors['name']">
                {{ formState.errors['name'] }}
              </div>
            </div>

            <div class="form-group">
              <label>Описание *</label>
              <textarea
                class="form-textarea"
                placeholder="Введите описание блюда"
                [(ngModel)]="menuItemForm.description"
                name="description"
                rows="3"
                (input)="onFormChange()">
              </textarea>
              <div class="error-message" *ngIf="formState.errors['description']">
                {{ formState.errors['description'] }}
              </div>
            </div>

            <div class="form-group">
              <label>Цена *</label>
              <app-input
                type="number"
                placeholder="0.00"
                [value]="menuItemForm.price?.toString() || ''"
                (valueChange)="menuItemForm.price = parseFloat($event) || 0; onFormChange()">
              </app-input>
              <div class="error-message" *ngIf="formState.errors['price']">
                {{ formState.errors['price'] }}
              </div>
            </div>

            <div class="form-group">
              <label>URL изображения</label>
              <app-input
                placeholder="Введите ссылку на изображение"
                [value]="menuItemForm.imageUrl"
                (valueChange)="menuItemForm.imageUrl = $event; onFormChange()">
              </app-input>
            </div>
          </form>
        </div>

        <!-- Delete Confirmation -->
        <div class="modal-body" *ngIf="modalState.type === 'delete'">
          <div class="delete-confirmation">
            <div class="warning-icon">⚠️</div>
            <p>
              Вы уверены, что хотите удалить блюдо
              <strong>{{ modalState.data?.name }}</strong>?
            </p>
            <p class="warning-text">
              Это действие нельзя отменить.
            </p>
          </div>
        </div>

        <div class="modal-footer">
          <button class="btn btn-secondary" (click)="closeModal()">
            Отмена
          </button>

          <button *ngIf="modalState.type !== 'delete'"
                  class="btn btn-primary"
                  [disabled]="!formState.isValid || formState.isLoading"
                  (click)="saveMenuItem()">
            <span *ngIf="formState.isLoading" class="loading-spinner-small"></span>
            {{ modalState.type === 'create' ? 'Создать' : 'Сохранить' }}
          </button>

          <button *ngIf="modalState.type === 'delete'"
                  class="btn btn-danger"
                  (click)="deleteMenuItem()">
            Удалить
          </button>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./admin-menu-items.component.css']
})
export class AdminMenuItemsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  menuItems: AdminMenuItem[] = [];
  filteredMenuItems: AdminMenuItem[] = [];
  restaurants: RestaurantResponse[] = [];
  categories: CategoryResponse[] = [];
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
  menuItemForm: MenuItemFormData = {
    restaurantId: '',
    categoryId: '',
    name: '',
    description: '',
    price: 0,
    imageUrl: ''
  };

  // Filters
  filters: MenuItemFilters = {
    search: '',
    restaurantId: undefined,
    categoryId: undefined,
    isAvailable: undefined,
    sortBy: 'name',
    sortOrder: 'asc'
  };

  constructor(private restaurantService: RestaurantService) {}

  ngOnInit(): void {
    this.loadData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadData(): void {
    this.loading = true;
    this.error = '';

    // Загружаем рестораны и категории с бэкенда
    Promise.all([
      this.restaurantService.getRestaurants().toPromise(),
      this.restaurantService.getCategories().toPromise()
    ]).then(([restaurants, categories]) => {
      this.restaurants = restaurants || [];
      this.categories = categories || [];
      this.loadMenuItems();
    }).catch(error => {
      console.error('Error loading data:', error);
      this.error = 'Ошибка при загрузке данных';
      this.loading = false;
    });
  }

  loadMenuItems(): void {
    // Загружаем все блюда через API
    this.restaurantService.searchMenuItems('')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (menuItems) => {
          this.menuItems = this.mapToAdminMenuItems(menuItems);
          this.applyFilters();
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading menu items:', error);
          this.error = 'Ошибка при загрузке блюд';
          this.loading = false;
        }
      });
  }

  // Маппинг MenuItemResponse в AdminMenuItem
  private mapToAdminMenuItems(items: MenuItemResponse[]): AdminMenuItem[] {
    return items.map(item => this.mapToAdminMenuItem(item));
  }

  // Маппинг одного MenuItemResponse в AdminMenuItem
  private mapToAdminMenuItem(item: MenuItemResponse): AdminMenuItem {
    return {
      id: item.id,
      restaurantId: item.restaurantId,
      restaurantName: item.restaurantName,
      categoryId: item.categoryId,
      categoryName: item.categoryName,
      name: item.name,
      description: item.description,
      price: item.price,
      isAvailable: item.isAvailable,
      imageUrl: item.imageUrl,
      averageRating: item.averageRating,
      reviewCount: item.reviewCount,
      totalOrders: 0, // TODO: добавить в API если нужно
      createdAt: new Date().toISOString(), // TODO: добавить в модель
      updatedAt: new Date().toISOString()  // TODO: добавить в модель
    };
  }

  applyFilters(): void {
    let filtered = [...this.menuItems];

    if (this.filters.search?.trim()) {
      const searchLower = this.filters.search.toLowerCase();
      filtered = filtered.filter(item =>
        item.name.toLowerCase().includes(searchLower) ||
        item.description.toLowerCase().includes(searchLower) ||
        item.restaurantName.toLowerCase().includes(searchLower)
      );
    }

    if (this.filters.restaurantId) {
      filtered = filtered.filter(item => item.restaurantId === this.filters.restaurantId);
    }

    if (this.filters.categoryId) {
      filtered = filtered.filter(item => item.categoryId === this.filters.categoryId);
    }

    if (this.filters.isAvailable !== undefined) {
      filtered = filtered.filter(item => item.isAvailable === this.filters.isAvailable);
    }

    this.filteredMenuItems = filtered;
  }

  onSearchChange(value: string): void {
    this.filters.search = value;
    this.applyFilters();
  }

  onFilterChange(): void {
    this.applyFilters();
  }

  // Modal methods
  openCreateModal(): void {
    this.modalState = {
      isOpen: true,
      title: 'Добавить блюдо',
      type: 'create',
      data: null
    };
    this.resetForm();
  }

  openEditModal(item: AdminMenuItem): void {
    this.modalState = {
      isOpen: true,
      title: 'Редактировать блюдо',
      type: 'edit',
      data: item
    };
    this.populateForm(item);
  }

  openDeleteModal(item: AdminMenuItem): void {
    this.modalState = {
      isOpen: true,
      title: 'Удалить блюдо',
      type: 'delete',
      data: item
    };
  }

  closeModal(): void {
    this.modalState.isOpen = false;
    this.resetForm();
  }

  // Form methods
  private resetForm(): void {
    this.menuItemForm = {
      restaurantId: '',
      categoryId: '',
      name: '',
      description: '',
      price: 0,
      imageUrl: ''
    };
    this.formState = {
      isLoading: false,
      errors: {},
      isDirty: false,
      isValid: false
    };
  }

  private populateForm(item: AdminMenuItem): void {
    this.menuItemForm = {
      restaurantId: item.restaurantId,
      categoryId: item.categoryId,
      name: item.name,
      description: item.description,
      price: item.price,
      imageUrl: item.imageUrl || ''
    };
    this.validateForm();
  }

  private validateForm(): void {
    const errors: { [key: string]: string } = {};

    if (!this.menuItemForm.restaurantId) {
      errors['restaurantId'] = 'Выберите ресторан';
    }

    if (!this.menuItemForm.categoryId) {
      errors['categoryId'] = 'Выберите категорию';
    }

    if (!this.menuItemForm.name?.trim()) {
      errors['name'] = 'Название блюда обязательно';
    }

    if (!this.menuItemForm.description?.trim()) {
      errors['description'] = 'Описание обязательно';
    }

    if (!this.menuItemForm.price || this.menuItemForm.price <= 0) {
      errors['price'] = 'Введите корректную цену';
    }

    this.formState.errors = errors;
    this.formState.isValid = Object.keys(errors).length === 0;
  }

  onFormChange(): void {
    this.formState.isDirty = true;
    this.validateForm();
  }

  saveMenuItem(): void {
    if (!this.formState.isValid) return;

    this.formState.isLoading = true;

    const menuItemData: CreateMenuItemRequest = {
      restaurantId: this.menuItemForm.restaurantId,
      categoryId: this.menuItemForm.categoryId,
      name: this.menuItemForm.name,
      description: this.menuItemForm.description,
      price: this.menuItemForm.price,
      imageUrl: this.menuItemForm.imageUrl || undefined
    };

    if (this.modalState.type === 'create') {
      // Создание нового блюда
      this.restaurantService.createMenuItem(menuItemData)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (newItem) => {
            const adminItem = this.mapToAdminMenuItem(newItem);
            this.menuItems.unshift(adminItem);
            this.applyFilters();
            this.formState.isLoading = false;
            this.closeModal();
          },
          error: (error) => {
            console.error('Error creating menu item:', error);
            this.formState.isLoading = false;
          }
        });
    } else if (this.modalState.type === 'edit' && this.modalState.data) {
      // Редактирование существующего блюда
      this.restaurantService.updateMenuItem(this.modalState.data.id, menuItemData)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (updatedItem) => {
            const index = this.menuItems.findIndex(i => i.id === this.modalState.data!.id);
            if (index !== -1) {
              this.menuItems[index] = this.mapToAdminMenuItem(updatedItem);
              this.applyFilters();
            }
            this.formState.isLoading = false;
            this.closeModal();
          },
          error: (error) => {
            console.error('Error updating menu item:', error);
            this.formState.isLoading = false;
          }
        });
    }
  }

  deleteMenuItem(): void {
    if (this.modalState.data) {
      const id = this.modalState.data.id;

      this.restaurantService.deleteMenuItem(id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.menuItems = this.menuItems.filter(i => i.id !== id);
            this.applyFilters();
            this.closeModal();
          },
          error: (error) => {
            console.error('Error deleting menu item:', error);
            this.error = 'Ошибка при удалении блюда';
          }
        });
    }
  }

  toggleItemStatus(item: AdminMenuItem): void {
    const updatedData: CreateMenuItemRequest = {
      restaurantId: item.restaurantId,
      categoryId: item.categoryId,
      name: item.name,
      description: item.description,
      price: item.price,
      imageUrl: item.imageUrl
    };

    this.restaurantService.updateMenuItem(item.id, updatedData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updatedItem) => {
          const index = this.menuItems.findIndex(i => i.id === item.id);
          if (index !== -1) {
            this.menuItems[index] = this.mapToAdminMenuItem(updatedItem);
            this.applyFilters();
          }
        },
        error: (error) => {
          console.error('Error toggling item status:', error);
        }
      });
  }

  // Utility methods
  getImageUrl(imageUrl?: string): string {
    return imageUrl || 'https://foni.papik.pro/uploads/posts/2024-09/foni-papik-pro-l126-p-kartinki-yeda-na-prozrachnom-fone-2.png';
  }

  onImageError(event: any): void {
    event.target.src = 'https://foni.papik.pro/uploads/posts/2024-09/foni-papik-pro-l126-p-kartinki-yeda-na-prozrachnom-fone-2.png';
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
      minimumFractionDigits: 2
    }).format(amount);
  }

  getStatusClass(isAvailable: boolean): string {
    return isAvailable ? 'status-active' : 'status-inactive';
  }

  getStatusText(isAvailable: boolean): string {
    return isAvailable ? 'Доступно' : 'Недоступно';
  }

  trackMenuItem(index: number, item: AdminMenuItem): string {
    return item.id;
  }

  protected readonly parseFloat = parseFloat;
}
