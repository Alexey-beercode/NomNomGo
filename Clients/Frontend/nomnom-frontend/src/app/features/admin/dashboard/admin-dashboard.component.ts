// src/app/features/admin/dashboard/admin-dashboard.component.ts

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Subject, forkJoin, takeUntil, finalize } from 'rxjs';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { OrderService } from '../../../core/services/order.service';
import { AuthService } from '../../../core/services/auth.service';

interface DashboardStats {
  totalRestaurants: number;
  totalOrders: number;
  totalUsers: number;
  totalRevenue: number;
  pendingOrders: number;
  activeRestaurants: number;
  todayOrders: number;
  monthlyGrowth: number;
}

interface RecentActivity {
  id: string;
  icon: string;
  text: string;
  time: string;
  type: 'order' | 'restaurant' | 'review' | 'user';
}

interface PopularRestaurant {
  id: string;
  name: string;
  rating: number;
  orders: number;
  revenue: number;
  growth: number;
}

interface QuickAction {
  icon: string;
  title: string;
  description: string;
  route: string;
  color: string;
  badge?: number;
  urgent?: boolean;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  stats: DashboardStats = {
    totalRestaurants: 0,
    totalOrders: 0,
    totalUsers: 0,
    totalRevenue: 0,
    pendingOrders: 0,
    activeRestaurants: 0,
    todayOrders: 0,
    monthlyGrowth: 0
  };

  recentActivities: RecentActivity[] = [];
  popularRestaurants: PopularRestaurant[] = [];
  quickActions: QuickAction[] = [];

  loading = true;
  error = '';
  lastUpdated = new Date();

  constructor(
    private restaurantService: RestaurantService,
    private orderService: OrderService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.setupQuickActions();
    this.loadDashboardData();

    // Автоматическое обновление каждые 5 минут
    this.setupAutoRefresh();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupQuickActions(): void {
    this.quickActions = [
      {
        icon: '➕',
        title: 'Добавить ресторан',
        description: 'Зарегистрировать новый ресторан в системе',
        route: '/admin/restaurants',
        color: 'success'
      },
      {
        icon: '🍽️',
        title: 'Добавить блюдо',
        description: 'Создать новое блюдо в меню ресторана',
        route: '/admin/menu-items',
        color: 'primary'
      },
      {
        icon: '📂',
        title: 'Добавить категорию',
        description: 'Создать новую категорию блюд',
        route: '/admin/categories',
        color: 'info'
      },
      {
        icon: '🔔',
        title: 'Обработать заказы',
        description: 'заказов ожидают обработки',
        route: '/admin/orders',
        color: 'warning',
        urgent: true,
        badge: this.stats.pendingOrders
      }
    ];
  }

  private setupAutoRefresh(): void {
    // Автообновление каждые 5 минут
    setInterval(() => {
      this.refreshData();
    }, 5 * 60 * 1000);
  }

  loadDashboardData(): void {
    this.loading = true;
    this.error = '';

    // Загружаем данные параллельно
    forkJoin({
      restaurants: this.restaurantService.getRestaurants(),
      // orders: this.orderService.getOrders(), // TODO: Добавить метод
      // users: this.userService.getUsers() // TODO: Добавить сервис пользователей
    })
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.loading = false;
          this.lastUpdated = new Date();
        })
      )
      .subscribe({
        next: (data) => {
          this.processRestaurantData(data.restaurants);
          // this.processOrderData(data.orders);
          // this.processUserData(data.users);

          this.loadMockData(); // Временно используем моковые данные
          this.updateQuickActions();
        },
        error: (error) => {
          console.error('Error loading dashboard data:', error);
          this.error = 'Ошибка при загрузке данных панели управления';
          this.loadMockData(); // Fallback на моковые данные
        }
      });
  }

  private processRestaurantData(restaurants: any[]): void {
    this.stats.totalRestaurants = restaurants.length;
    this.stats.activeRestaurants = restaurants.filter(r => r.isActive).length;

    // Обновляем популярные рестораны
    this.popularRestaurants = restaurants
      .sort((a, b) => b.averageRating - a.averageRating)
      .slice(0, 5)
      .map(restaurant => ({
        id: restaurant.id,
        name: restaurant.name,
        rating: restaurant.averageRating,
        orders: restaurant.reviewCount || 0, // Используем как заглушку
        revenue: 0, // TODO: получать из API статистики
        growth: Math.random() * 20 - 5 // Временная заглушка
      }));
  }

  private loadMockData(): void {
    // Временные данные до подключения всех API
    this.stats = {
      totalRestaurants: this.stats.totalRestaurants || 24,
      totalOrders: 1847,
      totalUsers: 3562,
      totalRevenue: 284750.50,
      pendingOrders: 12,
      activeRestaurants: this.stats.activeRestaurants || 21,
      todayOrders: 47,
      monthlyGrowth: 12.5
    };

  }

  private updateQuickActions(): void {
    const urgentAction = this.quickActions.find(action => action.urgent);
    if (urgentAction) {
      urgentAction.badge = this.stats.pendingOrders;
      urgentAction.description = `${this.stats.pendingOrders} заказов ожидают обработки`;
    }
  }

  refreshData(): void {
    this.loadDashboardData();
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

  formatNumber(num: number): string {
    return new Intl.NumberFormat('ru-RU').format(num);
  }

  getGrowthClass(growth: number): string {
    if (growth > 0) return 'growth-positive';
    if (growth < 0) return 'growth-negative';
    return 'growth-neutral';
  }

  getGrowthIcon(growth: number): string {
    if (growth > 0) return '📈';
    if (growth < 0) return '📉';
    return '➖';
  }

  getActivityTypeClass(type: string): string {
    const typeClasses: { [key: string]: string } = {
      'order': 'activity-order',
      'restaurant': 'activity-restaurant',
      'review': 'activity-review',
      'user': 'activity-user'
    };
    return typeClasses[type] || 'activity-default';
  }

  getRelativeTime(date: Date): string {
    const now = new Date();
    const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60));

    if (diffInMinutes < 1) return 'только что';
    if (diffInMinutes < 60) return `${diffInMinutes} мин назад`;

    const diffInHours = Math.floor(diffInMinutes / 60);
    if (diffInHours < 24) return `${diffInHours} ч назад`;

    const diffInDays = Math.floor(diffInHours / 24);
    if (diffInDays < 7) return `${diffInDays} дн назад`;

    return date.toLocaleDateString('ru-RU');
  }

  getLastUpdatedText(): string {
    return `Обновлено: ${this.getRelativeTime(this.lastUpdated)}`;
  }

  // Dashboard insights
  getRevenueGrowthPercentage(): number {
    return this.stats.monthlyGrowth;
  }

  getOrdersCompletionRate(): number {
    if (this.stats.totalOrders === 0) return 0;
    return ((this.stats.totalOrders - this.stats.pendingOrders) / this.stats.totalOrders) * 100;
  }

  getAverageOrderValue(): number {
    if (this.stats.totalOrders === 0) return 0;
    return this.stats.totalRevenue / this.stats.totalOrders;
  }

  getRestaurantUtilization(): number {
    if (this.stats.totalRestaurants === 0) return 0;
    return (this.stats.activeRestaurants / this.stats.totalRestaurants) * 100;
  }

  // Navigation helpers
  navigateToSection(route: string): void {
    // Можно добавить аналитику или другую логику
    console.log(`Navigating to: ${route}`);
  }

  // TrackBy functions for performance
  trackActivity(index: number, activity: RecentActivity): string {
    return activity.id;
  }

  trackRestaurant(index: number, restaurant: PopularRestaurant): string {
    return restaurant.id;
  }
}
