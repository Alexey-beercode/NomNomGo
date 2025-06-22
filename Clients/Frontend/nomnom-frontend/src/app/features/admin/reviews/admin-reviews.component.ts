// src/app/features/admin/reviews/admin-reviews.component.ts

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, takeUntil, finalize, of } from 'rxjs';
import { InputComponent } from '../../../shared/components/input/input.component';

interface ReviewResponse {
  id: string;
  userId: string;
  userName: string;
  targetId: string;
  targetName: string;
  targetType: 'Restaurant' | 'MenuItem' | 'Courier';
  rating: number;
  comment?: string;
  sentiment?: 'Positive' | 'Negative' | 'Neutral';
  isHidden: boolean;
  hiddenReason?: string;
  createdAt: string;
  expanded?: boolean; // для UI
}

interface ReviewFilters {
  search: string;
  targetType?: string;
  rating?: number;
  isHidden?: boolean;
  dateFrom?: string;
  dateTo?: string;
  sortBy: 'createdAt' | 'rating' | 'userName';
  sortOrder: 'asc' | 'desc';
}

interface ModalState {
  isOpen: boolean;
  title: string;
  type: 'view' | 'hide' | 'show' | 'delete' | 'report';
  data: ReviewResponse | null;
}

@Component({
  selector: 'app-admin-reviews',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, InputComponent],
  templateUrl: './admin-reviews.component.html',
  styleUrls: ['./admin-reviews.component.css']
})
export class AdminReviewsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  reviews: ReviewResponse[] = [];
  filteredReviews: ReviewResponse[] = [];
  loading = false;
  error = '';

  // Modal state
  modalState: ModalState = {
    isOpen: false,
    title: '',
    type: 'view',
    data: null
  };

  // Form data
  hideReason = '';
  deleteReason = '';
  reportReason = '';
  reportComment = '';

  // Filters
  filters: ReviewFilters = {
    search: '',
    targetType: undefined,
    rating: undefined,
    isHidden: undefined,
    dateFrom: undefined,
    dateTo: undefined,
    sortBy: 'createdAt',
    sortOrder: 'desc'
  };

  constructor() {}

  ngOnInit(): void {
    this.loadReviews();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadReviews(): void {
    this.loading = true;
    this.error = '';

    // Создаем моковые данные для демонстрации
    // В реальном приложении здесь будет вызов API
    of(this.createMockReviews())
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.loading = false)
      )
      .subscribe({
        next: (reviews) => {
          this.reviews = reviews;
          this.applyFilters();
        },
        error: (error) => {
          console.error('Error loading reviews:', error);
          this.error = 'Ошибка при загрузке отзывов';
          this.reviews = [];
          this.filteredReviews = [];
        }
      });
  }

  private createMockReviews(): ReviewResponse[] {
    return [
      {
        id: 'review-1',
        userId: 'user-1',
        userName: 'Иван Петров',
        targetId: 'restaurant-1',
        targetName: 'Пицца Дом',
        targetType: 'Restaurant',
        rating: 5,
        comment: 'Отличный ресторан! Очень вкусная пицца, быстрая доставка. Рекомендую всем!',
        sentiment: 'Positive',
        isHidden: false,
        createdAt: new Date(Date.now() - 86400000 * 1).toISOString()
      },
      {
        id: 'review-2',
        userId: 'user-2',
        userName: 'Мария Сидорова',
        targetId: 'menu-item-1',
        targetName: 'Маргарита',
        targetType: 'MenuItem',
        rating: 4,
        comment: 'Хорошая пицца, но могла бы быть больше сыра. В целом довольна.',
        sentiment: 'Positive',
        isHidden: false,
        createdAt: new Date(Date.now() - 86400000 * 2).toISOString()
      },
      {
        id: 'review-3',
        userId: 'user-3',
        userName: 'Алексей Смирнов',
        targetId: 'courier-1',
        targetName: 'Курьер Андрей',
        targetType: 'Courier',
        rating: 2,
        comment: 'Курьер опоздал на 40 минут, пицца была холодной. Очень расстроен качеством сервиса.',
        sentiment: 'Negative',
        isHidden: false,
        createdAt: new Date(Date.now() - 86400000 * 3).toISOString()
      },
      {
        id: 'review-4',
        userId: 'user-4',
        userName: 'Елена Козлова',
        targetId: 'restaurant-2',
        targetName: 'Суши Бар',
        targetType: 'Restaurant',
        rating: 1,
        comment: 'Ужасно! Роллы были несвежими, в ресторане грязно. Никому не советую!',
        sentiment: 'Negative',
        isHidden: true,
        hiddenReason: 'Слишком агрессивный тон',
        createdAt: new Date(Date.now() - 86400000 * 4).toISOString()
      },
      {
        id: 'review-5',
        userId: 'user-5',
        userName: 'Дмитрий Волков',
        targetId: 'menu-item-2',
        targetName: 'Филадельфия',
        targetType: 'MenuItem',
        rating: 5,
        comment: 'Лучшие роллы в городе! Всегда заказываю только здесь. Качество на высоте, доставка быстрая.',
        sentiment: 'Positive',
        isHidden: false,
        createdAt: new Date(Date.now() - 86400000 * 5).toISOString()
      },
      {
        id: 'review-6',
        userId: 'user-6',
        userName: 'Анна Белова',
        targetId: 'restaurant-1',
        targetName: 'Пицца Дом',
        targetType: 'Restaurant',
        rating: 3,
        comment: 'Нормально, но ничего особенного. Цены завышены для такого качества.',
        sentiment: 'Neutral',
        isHidden: false,
        createdAt: new Date(Date.now() - 86400000 * 6).toISOString()
      }
    ];
  }

  applyFilters(): void {
    let filtered = [...this.reviews];

    // Поиск
    if (this.filters.search?.trim()) {
      const searchLower = this.filters.search.toLowerCase();
      filtered = filtered.filter(review =>
        review.userName.toLowerCase().includes(searchLower) ||
        review.targetName.toLowerCase().includes(searchLower) ||
        (review.comment && review.comment.toLowerCase().includes(searchLower))
      );
    }

    // Фильтр по типу
    if (this.filters.targetType) {
      filtered = filtered.filter(review => review.targetType === this.filters.targetType);
    }

    // Фильтр по рейтингу
    if (this.filters.rating) {
      filtered = filtered.filter(review => review.rating === this.filters.rating);
    }

    // Фильтр по статусу скрытия
    if (this.filters.isHidden !== undefined) {
      filtered = filtered.filter(review => review.isHidden === this.filters.isHidden);
    }

    // Фильтр по дате
    if (this.filters.dateFrom) {
      const fromDate = new Date(this.filters.dateFrom);
      filtered = filtered.filter(review => new Date(review.createdAt) >= fromDate);
    }

    if (this.filters.dateTo) {
      const toDate = new Date(this.filters.dateTo);
      toDate.setHours(23, 59, 59, 999);
      filtered = filtered.filter(review => new Date(review.createdAt) <= toDate);
    }

    // Сортировка
    filtered.sort((a, b) => {
      let aValue: any = a[this.filters.sortBy];
      let bValue: any = b[this.filters.sortBy];

      if (this.filters.sortBy === 'createdAt') {
        aValue = new Date(aValue).getTime();
        bValue = new Date(bValue).getTime();
      }

      if (aValue < bValue) return this.filters.sortOrder === 'asc' ? -1 : 1;
      if (aValue > bValue) return this.filters.sortOrder === 'asc' ? 1 : -1;
      return 0;
    });

    this.filteredReviews = filtered;
  }

  onSearchChange(value: string): void {
    this.filters.search = value;
    this.applyFilters();
  }

  onFilterChange(): void {
    this.applyFilters();
  }

  clearFilters(): void {
    this.filters = {
      search: '',
      targetType: undefined,
      rating: undefined,
      isHidden: undefined,
      dateFrom: undefined,
      dateTo: undefined,
      sortBy: 'createdAt',
      sortOrder: 'desc'
    };
    this.applyFilters();
  }

  refreshData(): void {
    this.loadReviews();
  }

  // Modal methods
  openViewModal(review: ReviewResponse): void {
    this.modalState = {
      isOpen: true,
      title: `Отзыв от ${review.userName}`,
      type: 'view',
      data: review
    };
  }

  openDeleteModal(review: ReviewResponse): void {
    this.modalState = {
      isOpen: true,
      title: 'Удалить отзыв',
      type: 'delete',
      data: review
    };
    this.deleteReason = '';
  }

  openReportModal(review: ReviewResponse): void {
    this.modalState = {
      isOpen: true,
      title: 'Пожаловаться на отзыв',
      type: 'report',
      data: review
    };
    this.reportReason = '';
    this.reportComment = '';
  }

  closeModal(): void {
    this.modalState.isOpen = false;
    this.hideReason = '';
    this.deleteReason = '';
    this.reportReason = '';
    this.reportComment = '';
  }

  // Review actions
  toggleReviewVisibility(review: ReviewResponse): void {
    this.modalState = {
      isOpen: true,
      title: review.isHidden ? 'Показать отзыв' : 'Скрыть отзыв',
      type: review.isHidden ? 'show' : 'hide',
      data: review
    };
    this.hideReason = '';
  }

  confirmHideReview(): void {
    if (this.modalState.data && this.hideReason.trim()) {
      const reviewId = this.modalState.data.id;
      const index = this.reviews.findIndex(r => r.id === reviewId);

      if (index !== -1) {
        this.reviews[index].isHidden = true;
        this.reviews[index].hiddenReason = this.hideReason;
        this.applyFilters();
      }

      this.closeModal();
    }
  }

  confirmShowReview(): void {
    if (this.modalState.data) {
      const reviewId = this.modalState.data.id;
      const index = this.reviews.findIndex(r => r.id === reviewId);

      if (index !== -1) {
        this.reviews[index].isHidden = false;
        this.reviews[index].hiddenReason = undefined;
        this.applyFilters();
      }

      this.closeModal();
    }
  }

  confirmDeleteReview(): void {
    if (this.modalState.data && this.deleteReason.trim()) {
      const reviewId = this.modalState.data.id;
      this.reviews = this.reviews.filter(r => r.id !== reviewId);
      this.applyFilters();
      this.closeModal();
    }
  }

  confirmReportReview(): void {
    if (this.modalState.data && this.reportReason) {
      // В реальном приложении здесь будет отправка жалобы на сервер
      console.log('Report sent for review:', this.modalState.data.id, {
        reason: this.reportReason,
        comment: this.reportComment
      });
      this.closeModal();
    }
  }

  toggleReviewExpansion(review: ReviewResponse): void {
    review.expanded = !review.expanded;
  }

  // Utility methods
  getUserInitials(userName: string): string {
    return userName.split(' ').map(name => name[0]).join('').toUpperCase().substring(0, 2);
  }

  getStarsArray(rating: number): boolean[] {
    return Array.from({ length: 5 }, (_, i) => i < rating);
  }

  getTargetTypeClass(targetType: string): string {
    switch (targetType) {
      case 'Restaurant': return 'type-restaurant';
      case 'MenuItem': return 'type-menuitem';
      case 'Courier': return 'type-courier';
      default: return 'type-default';
    }
  }

  getTargetTypeText(targetType: string): string {
    switch (targetType) {
      case 'Restaurant': return 'Ресторан';
      case 'MenuItem': return 'Блюдо';
      case 'Courier': return 'Курьер';
      default: return targetType;
    }
  }

  getSentimentClass(sentiment: string): string {
    switch (sentiment) {
      case 'Positive': return 'sentiment-positive';
      case 'Negative': return 'sentiment-negative';
      case 'Neutral': return 'sentiment-neutral';
      default: return 'sentiment-unknown';
    }
  }

  getSentimentText(sentiment: string): string {
    switch (sentiment) {
      case 'Positive': return 'Позитивный';
      case 'Negative': return 'Негативный';
      case 'Neutral': return 'Нейтральный';
      default: return sentiment;
    }
  }

  formatDateTime(dateString: string): string {
    return new Date(dateString).toLocaleString('ru-RU');
  }

  // Stats methods
  getTotalReviews(): number {
    return this.reviews.length;
  }

  getPositiveReviews(): number {
    return this.reviews.filter(r => r.rating >= 4).length;
  }

  getNegativeReviews(): number {
    return this.reviews.filter(r => r.rating <= 2).length;
  }

  getAverageRating(): string {
    if (this.reviews.length === 0) return '0.0';
    const sum = this.reviews.reduce((acc, review) => acc + review.rating, 0);
    return (sum / this.reviews.length).toFixed(1);
  }

  getHiddenReviews(): number {
    return this.reviews.filter(r => r.isHidden).length;
  }

  trackReview(index: number, review: ReviewResponse): string {
    return review.id;
  }
}
