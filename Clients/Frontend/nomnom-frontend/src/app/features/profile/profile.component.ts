// src/app/features/profile/profile.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil, switchMap } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { OrderService } from '../../core/services/order.service';
import { RecommendationService } from '../../core/services/recommendation.service';
import { CurrentUser } from '../../core/models/auth.models';
import { OrderResponse } from '../../core/models/order.models';
import { CreateReviewRequest } from '../../core/models/recommendation.models';

interface OrderWithReviewOptions extends OrderResponse {
  canBeReviewed?: boolean;
}

interface ReviewForm {
  targetId: string;
  targetType: 'Restaurant' | 'MenuItem';
  targetName: string;
  rating: number;
  comment: string;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  currentUser: CurrentUser | null = null;
  orders: OrderWithReviewOptions[] = [];
  loading = true;
  error = '';

  // Состояние вкладок
  activeTab: 'profile' | 'orders' | 'settings' = 'profile';

  // Состояние редактирования профиля
  isEditingProfile = false;
  profileForm = {
    username: '',
    email: '',
    phoneNumber: ''
  };

  // Состояние смены пароля
  isChangingPassword = false;
  passwordForm = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  // Фильтр заказов
  orderFilter: 'all' | 'active' | 'completed' | 'cancelled' = 'all';

  // Состояние отзывов
  isLeavingReview = false;
  selectedOrder: OrderWithReviewOptions | null = null;
  reviewForm: ReviewForm = {
    targetId: '',
    targetType: 'Restaurant',
    targetName: '',
    rating: 5,
    comment: ''
  };
  savingReview = false;

  constructor(
    private authService: AuthService,
    private orderService: OrderService,
    private recommendationService: RecommendationService,
    public router: Router
  ) {}

  ngOnInit(): void {
    this.loadUserData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadUserData(): void {
    this.loading = true;

    this.authService.getCurrentUser().pipe(
      takeUntil(this.destroy$),
      switchMap(user => {
        if (!user) {
          throw new Error('Пользователь не найден');
        }
        this.currentUser = user;
        this.initializeProfileForm();

        return this.orderService.getUserOrders(user.userId);
      })
    ).subscribe({
      next: (orders) => {
        this.orders = this.processOrders(orders);
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading user data:', error);
        this.error = 'Ошибка загрузки данных';
        this.loading = false;

        if (error.message === 'Пользователь не найден') {
          this.router.navigate(['/login']);
        }
      }
    });
  }

  private processOrders(orders: OrderResponse[]): OrderWithReviewOptions[] {
    const now = new Date();
    const threeDaysAgo = new Date(now.getTime() - 3 * 24 * 60 * 60 * 1000);

    return orders.map(order => {
      const orderDate = new Date(order.createdAt);
      const canBeReviewed = order.status.toLowerCase() === 'delivered' && orderDate > threeDaysAgo;

      return {
        ...order,
        canBeReviewed
      };
    }).sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
  }

  // === МЕТОДЫ ДЛЯ ОТЗЫВОВ ===
  startReview(order: OrderWithReviewOptions, targetType: 'Restaurant' | 'MenuItem', targetId?: string): void {
    this.selectedOrder = order;
    this.isLeavingReview = true;

    if (targetType === 'Restaurant') {
      this.reviewForm = {
        targetId: order.restaurant.id,
        targetType: 'Restaurant',
        targetName: order.restaurant.name,
        rating: 5,
        comment: ''
      };
    } else if (targetType === 'MenuItem' && targetId) {
      const menuItem = order.items.find(item => item.menuItem.id === targetId)?.menuItem;
      if (menuItem) {
        this.reviewForm = {
          targetId: targetId,
          targetType: 'MenuItem',
          targetName: menuItem.name,
          rating: 5,
          comment: ''
        };
      }
    }
  }

  cancelReview(): void {
    this.isLeavingReview = false;
    this.selectedOrder = null;
    this.reviewForm = {
      targetId: '',
      targetType: 'Restaurant',
      targetName: '',
      rating: 5,
      comment: ''
    };
  }

  setRating(rating: number): void {
    this.reviewForm.rating = rating;
  }

  saveReview(): void {
    if (!this.currentUser || !this.validateReviewForm()) return;

    this.savingReview = true;

    const request: CreateReviewRequest = {
      userId: this.currentUser.userId,
      targetId: this.reviewForm.targetId,
      targetType: this.reviewForm.targetType,
      rating: this.reviewForm.rating,
      comment: this.reviewForm.comment.trim() || undefined
    };

    this.recommendationService.createReview(request).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: () => {
        this.cancelReview();
        this.savingReview = false;
        // Показываем успешное сообщение
        this.error = '';
        alert('Отзыв успешно сохранен!');
      },
      error: (error) => {
        console.error('Error saving review:', error);
        this.error = 'Ошибка сохранения отзыва';
        this.savingReview = false;
      }
    });
  }

  private validateReviewForm(): boolean {
    if (this.reviewForm.rating < 1 || this.reviewForm.rating > 5) {
      this.error = 'Рейтинг должен быть от 1 до 5';
      return false;
    }

    this.error = '';
    return true;
  }

  canReviewRestaurant(order: OrderWithReviewOptions): boolean {
    return order.canBeReviewed === true;
  }

  // === ОСТАЛЬНЫЕ МЕТОДЫ (без изменений) ===
  private initializeProfileForm(): void {
    if (this.currentUser) {
      this.profileForm = {
        username: this.currentUser.username || '',
        email: this.currentUser.email || '',
        phoneNumber: this.currentUser.phoneNumber || ''
      };
    }
  }

  setActiveTab(tab: 'profile' | 'orders' | 'settings'): void {
    this.activeTab = tab;
  }

  startEditingProfile(): void {
    this.isEditingProfile = true;
  }

  cancelEditingProfile(): void {
    this.isEditingProfile = false;
    this.initializeProfileForm();
  }

  saveProfile(): void {
    if (!this.validateProfileForm()) return;

    this.authService.updateProfile(this.profileForm).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: () => {
        this.isEditingProfile = false;
        this.loadUserData();
      },
      error: (error: any) => {
        console.error('Error updating profile:', error);
        this.error = 'Ошибка обновления профиля';
      }
    });
  }

  private validateProfileForm(): boolean {
    if (!this.profileForm.username.trim()) {
      this.error = 'Имя пользователя не может быть пустым';
      return false;
    }
    if (!this.profileForm.email.trim()) {
      this.error = 'Email не может быть пустым';
      return false;
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.profileForm.email)) {
      this.error = 'Некорректный формат email';
      return false;
    }
    this.error = '';
    return true;
  }

  startChangingPassword(): void {
    this.isChangingPassword = true;
    this.passwordForm = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    };
  }

  cancelChangingPassword(): void {
    this.isChangingPassword = false;
    this.passwordForm = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    };
  }

  changePassword(): void {
    if (!this.validatePasswordForm()) return;

    this.authService.changePassword(
      this.passwordForm.currentPassword,
      this.passwordForm.newPassword
    ).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: () => {
        this.isChangingPassword = false;
        this.passwordForm = {
          currentPassword: '',
          newPassword: '',
          confirmPassword: ''
        };
        alert('Пароль успешно изменен');
      },
      error: (error: any) => {
        console.error('Error changing password:', error);
        this.error = 'Ошибка смены пароля';
      }
    });
  }

  private validatePasswordForm(): boolean {
    if (!this.passwordForm.currentPassword) {
      this.error = 'Введите текущий пароль';
      return false;
    }
    if (this.passwordForm.newPassword.length < 6) {
      this.error = 'Новый пароль должен содержать минимум 6 символов';
      return false;
    }
    if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
      this.error = 'Пароли не совпадают';
      return false;
    }
    this.error = '';
    return true;
  }

  setOrderFilter(filter: 'all' | 'active' | 'completed' | 'cancelled'): void {
    this.orderFilter = filter;
  }

  get filteredOrders(): OrderWithReviewOptions[] {
    switch (this.orderFilter) {
      case 'active':
        return this.orders.filter(order =>
          !['delivered', 'cancelled'].includes(order.status.toLowerCase())
        );
      case 'completed':
        return this.orders.filter(order =>
          order.status.toLowerCase() === 'delivered'
        );
      case 'cancelled':
        return this.orders.filter(order =>
          order.status.toLowerCase() === 'cancelled'
        );
      default:
        return this.orders;
    }
  }

  getOrderStatusInfo(status: string) {
    return this.orderService.getOrderStatusDisplay(status);
  }

  formatOrderDate(dateString: string): string {
    return this.orderService.formatOrderDate(dateString);
  }

  getUserInitial(): string {
    return this.currentUser?.username?.charAt(0).toUpperCase() || '?';
  }

  getFormattedCreatedDate(): string {
    return this.currentUser?.createdAt ? this.formatOrderDate(this.currentUser.createdAt) : '—';
  }

  viewOrder(orderId: string): void {
    this.router.navigate(['/orders', orderId]);
  }

  trackOrder(orderId: string): void {
    this.router.navigate(['/tracking', orderId]);
  }

  logout(): void {
    this.authService.logout().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: (error: any) => {
        console.error('Error during logout:', error);
        localStorage.clear();
        this.router.navigate(['/login']);
      }
    });
  }
}
