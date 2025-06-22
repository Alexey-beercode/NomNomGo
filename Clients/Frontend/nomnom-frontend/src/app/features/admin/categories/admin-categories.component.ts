// src/app/features/admin/categories/admin-categories.component.ts

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, takeUntil, finalize } from 'rxjs';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { CategoryResponse } from '../../../core/models/restaurant.models';
import { InputComponent } from '../../../shared/components/input/input.component';

interface CategoryFormData {
  name: string;
}

interface CategoryFilters {
  search: string;
  sortBy: 'name' | 'itemsCount';
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
  data: CategoryResponse | null;
}

@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, InputComponent],
  templateUrl: './admin-categories.component.html',
  styleUrls: ['./admin-categories.component.css']
})
export class AdminCategoriesComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  categories: CategoryResponse[] = [];
  filteredCategories: CategoryResponse[] = [];
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
  categoryForm: CategoryFormData = {
    name: ''
  };

  // Filters
  filters: CategoryFilters = {
    search: '',
    sortBy: 'name',
    sortOrder: 'asc'
  };

  constructor(private restaurantService: RestaurantService) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCategories(): void {
    this.loading = true;
    this.error = '';

    this.restaurantService.getCategories()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.loading = false)
      )
      .subscribe({
        next: (categories) => {
          this.categories = categories;
          this.applyFilters();
        },
        error: (error) => {
          console.error('Error loading categories:', error);
          this.error = 'Ошибка при загрузке категорий';
          this.categories = [];
          this.filteredCategories = [];
        }
      });
  }

  applyFilters(): void {
    let filtered = [...this.categories];

    if (this.filters.search?.trim()) {
      const searchLower = this.filters.search.toLowerCase();
      filtered = filtered.filter(category =>
        category.name.toLowerCase().includes(searchLower)
      );
    }

    // Sorting
    filtered.sort((a, b) => {
      let aValue: any;
      let bValue: any;

      switch (this.filters.sortBy) {
        case 'name':
          aValue = a.name.toLowerCase();
          bValue = b.name.toLowerCase();
          break;
        case 'itemsCount':
          aValue = a.itemsCount;
          bValue = b.itemsCount;
          break;
        default:
          return 0;
      }

      if (aValue < bValue) return this.filters.sortOrder === 'asc' ? -1 : 1;
      if (aValue > bValue) return this.filters.sortOrder === 'asc' ? 1 : -1;
      return 0;
    });

    this.filteredCategories = filtered;
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
      title: 'Добавить категорию',
      type: 'create',
      data: null
    };
    this.resetForm();
  }

  openEditModal(category: CategoryResponse): void {
    this.modalState = {
      isOpen: true,
      title: 'Редактировать категорию',
      type: 'edit',
      data: category
    };
    this.populateForm(category);
  }

  openDeleteModal(category: CategoryResponse): void {
    this.modalState = {
      isOpen: true,
      title: 'Удалить категорию',
      type: 'delete',
      data: category
    };
  }

  closeModal(): void {
    this.modalState.isOpen = false;
    this.resetForm();
  }

  // Form methods
  private resetForm(): void {
    this.categoryForm = {
      name: ''
    };
    this.formState = {
      isLoading: false,
      errors: {},
      isDirty: false,
      isValid: false
    };
  }

  private populateForm(category: CategoryResponse): void {
    this.categoryForm = {
      name: category.name
    };
    this.validateForm();
  }

  private validateForm(): void {
    const errors: { [key: string]: string } = {};

    if (!this.categoryForm.name?.trim()) {
      errors['name'] = 'Название категории обязательно';
    }

    this.formState.errors = errors;
    this.formState.isValid = Object.keys(errors).length === 0;
  }

  onFormChange(): void {
    this.formState.isDirty = true;
    this.validateForm();
  }

  saveCategory(): void {
    if (!this.formState.isValid) return;

    this.formState.isLoading = true;

    // Пока API для создания/обновления категорий не готово, используем заглушку
    setTimeout(() => {
      if (this.modalState.type === 'create') {
        const newCategory: CategoryResponse = {
          id: Date.now().toString(),
          name: this.categoryForm.name,
          itemsCount: 0
        };
        this.categories.unshift(newCategory);
      } else if (this.modalState.type === 'edit' && this.modalState.data) {
        const index = this.categories.findIndex(c => c.id === this.modalState.data!.id);
        if (index !== -1) {
          this.categories[index] = {
            ...this.categories[index],
            name: this.categoryForm.name
          };
        }
      }

      this.applyFilters();
      this.formState.isLoading = false;
      this.closeModal();
    }, 1000);
  }

  deleteCategory(): void {
    if (this.modalState.data) {
      const id = this.modalState.data.id;
      // Пока API для удаления категорий не готово, используем заглушку
      this.categories = this.categories.filter(c => c.id !== id);
      this.applyFilters();
      this.closeModal();
    }
  }

  refreshData(): void {
    this.loadCategories();
  }

  // Utility methods
  getSortIcon(sortBy: string): string {
    if (this.filters.sortBy !== sortBy) return '↕️';
    return this.filters.sortOrder === 'asc' ? '↑' : '↓';
  }

  trackCategory(index: number, category: CategoryResponse): string {
    return category.id;
  }
}
