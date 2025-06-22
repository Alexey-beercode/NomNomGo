// src/app/core/services/order.service.ts

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { map, catchError, switchMap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  CreateOrderRequest,
  OrderResponse,
  OrderStatusInfo,
  Order
} from '../models/order.models';

export interface TrackingInfo {
  orderId: string;
  status: string;
  estimatedDeliveryTime?: string;
  courierLocation?: {
    latitude: number;
    longitude: number;
  };
  statusHistory: {
    status: string;
    timestamp: string;
    notes?: string;
  }[];
}

export interface OrderFilters {
  status?: string;
  restaurantId?: string;
  userId?: string;
  dateFrom?: string;
  dateTo?: string;
  page?: number;
  limit?: number;
}

export interface OrderStats {
  totalOrders: number;
  pendingOrders: number;
  completedOrders: number;
  cancelledOrders: number;
  todayOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
}

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private readonly apiUrl = `${environment.menuOrderApiUrl}/api/Orders`;
  private ordersSubject = new BehaviorSubject<OrderResponse[]>([]);
  public orders$ = this.ordersSubject.asObservable();

  constructor(private http: HttpClient) {}

  // Обработчик ошибок
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Произошла ошибка';

    console.error('OrderService Error Details:', {
      error: error,
      status: error.status,
      statusText: error.statusText,
      url: error.url,
      errorBody: error.error
    });

    if (error.error instanceof ErrorEvent) {
      // Ошибка на стороне клиента
      errorMessage = `Ошибка: ${error.error.message}`;
    } else {
      // Ошибка на стороне сервера
      switch (error.status) {
        case 0:
          errorMessage = 'Нет подключения к серверу';
          break;
        case 400:
          errorMessage = 'Неверные данные запроса';
          break;
        case 401:
          errorMessage = 'Необходима авторизация';
          break;
        case 403:
          errorMessage = 'Недостаточно прав доступа';
          break;
        case 404:
          errorMessage = 'Ресурс не найден';
          break;
        case 500:
          errorMessage = 'Внутренняя ошибка сервера';
          break;
        default:
          errorMessage = `Ошибка сервера: ${error.status}`;
      }

      if (error.error?.message) {
        errorMessage += `: ${error.error.message}`;
      } else if (error.error && typeof error.error === 'string') {
        errorMessage += `: ${error.error}`;
      }
    }

    return throwError(() => new Error(errorMessage));
  }

  // CRUD операции
  createOrder(request: CreateOrderRequest): Observable<OrderResponse> {
    return this.http.post<OrderResponse>(this.apiUrl, request)
      .pipe(catchError(this.handleError));
  }

  getOrder(orderId: string): Observable<OrderResponse> {
    return this.http.get<OrderResponse>(`${this.apiUrl}/${orderId}`)
      .pipe(catchError(this.handleError));
  }

  getOrders(filters?: OrderFilters): Observable<OrderResponse[]> {
    console.log('Making request to:', this.apiUrl);

    return this.http.get<any>(this.apiUrl)
      .pipe(
        map(response => {
          console.log('Raw orders response:', response);

          // Проверяем различные возможные структуры ответа
          let orders: any[] = [];

          if (Array.isArray(response)) {
            // Если ответ - это просто массив заказов
            orders = response;
          } else if (response && response.items && Array.isArray(response.items)) {
            // Если ответ пагинированный с полем items
            orders = response.items;
          } else if (response && response.data && Array.isArray(response.data)) {
            // Если ответ обернут в объект с полем data
            orders = response.data;
          } else if (response && response.orders && Array.isArray(response.orders)) {
            // Если заказы в поле orders
            orders = response.orders;
          } else if (response && response.result && Array.isArray(response.result)) {
            // Если заказы в поле result
            orders = response.result;
          } else {
            console.error('Unexpected response structure:', response);
            throw new Error('Неожиданная структура ответа от сервера');
          }

          if (!Array.isArray(orders)) {
            console.error('Orders is not an array:', orders);
            throw new Error('Сервер вернул некорректные данные');
          }

          console.log('Processed orders:', orders);
          this.ordersSubject.next(orders);
          return orders;
        }),
        catchError(this.handleError)
      );
  }

  getActiveOrders(): Observable<OrderResponse[]> {
    return this.http.get<OrderResponse[]>(`${this.apiUrl}/active`)
      .pipe(catchError(this.handleError));
  }

  getUserOrders(userId: string, page: number = 1, limit: number = 10): Observable<OrderResponse[]> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('limit', limit.toString());

    return this.http.get<OrderResponse[]>(`${this.apiUrl}/user/${userId}`, { params })
      .pipe(catchError(this.handleError));
  }

  getOrdersByStatus(status: string): Observable<OrderResponse[]> {
    return this.http.get<OrderResponse[]>(`${this.apiUrl}/status/${status}`)
      .pipe(catchError(this.handleError));
  }

  getRestaurantOrders(restaurantId: string, filters?: OrderFilters): Observable<OrderResponse[]> {
    let params = new HttpParams();

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params = params.set(key, value.toString());
        }
      });
    }

    return this.http.get<OrderResponse[]>(`${this.apiUrl}/restaurant/${restaurantId}`, { params })
      .pipe(catchError(this.handleError));
  }

  // Управление статусом заказа - ИСПРАВЛЕНО под ваш C# API
  updateOrderStatus(orderId: string, status: string, comment?: string): Observable<OrderResponse> {
    // Формируем запрос в соответствии с C# DTO
    const body = {
      Status: status,        // C# ожидает Status с большой буквы
      Comment: comment || null   // C# ожидает Comment, а не notes
    };

    console.log('Updating order status:', { orderId, body });

    return this.http.put(`${this.apiUrl}/${orderId}/status`, body)
      .pipe(
        // Поскольку C# возвращает NoContent (204), нужно получить обновленный заказ отдельно
        switchMap(() => this.getOrder(orderId)),
        catchError(this.handleError)
      );
  }

  cancelOrder(orderId: string, reason?: string): Observable<OrderResponse> {
    return this.updateOrderStatus(orderId, 'Cancelled', reason);
  }

  assignCourier(orderId: string, courierId: string): Observable<OrderResponse> {
    return this.http.put(`${this.apiUrl}/${orderId}/assign-courier/${courierId}`, {})
      .pipe(
        // Аналогично, получаем обновленный заказ
        switchMap(() => this.getOrder(orderId)),
        catchError(this.handleError)
      );
  }

  // Статистика
  getOrderStats(filters?: { dateFrom?: string; dateTo?: string; restaurantId?: string }): Observable<OrderStats> {
    let params = new HttpParams();

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params = params.set(key, value.toString());
        }
      });
    }

    return this.http.get<OrderStats>(`${this.apiUrl}/stats`, { params })
      .pipe(catchError(this.handleError));
  }

  getDashboardStats(): Observable<OrderStats> {
    return this.http.get<OrderStats>(`${this.apiUrl}/dashboard-stats`)
      .pipe(catchError(this.handleError));
  }

  // Утилиты для UI
  getOrderStatusDisplay(status: string): OrderStatusInfo {
    const normalizedStatus = status.toUpperCase();
    const statusMap: { [key: string]: OrderStatusInfo } = {
      'PENDING': { text: 'Ожидает обработки', color: '#FFA500', icon: '⏳' },
      'CONFIRMED': { text: 'Подтвержден', color: '#4CAF50', icon: '✅' },
      'PREPARING': { text: 'Готовится', color: '#2196F3', icon: '👨‍🍳' },
      'READY': { text: 'Готов к выдаче', color: '#FF9800', icon: '🍽️' },
      'INDELIVERY': { text: 'В доставке', color: '#9C27B0', icon: '🚚' },
      'DELIVERED': { text: 'Доставлен', color: '#4CAF50', icon: '✅' },
      'CANCELLED': { text: 'Отменен', color: '#F44336', icon: '❌' }
    };

    return statusMap[normalizedStatus] || {
      text: status,
      color: '#757575',
      icon: '❓'
    };
  }

  getStatusOptions(): { value: string; label: string; color: string }[] {
    return [
      { value: 'Pending', label: 'Ожидает обработки', color: '#FFA500' },
      { value: 'Confirmed', label: 'Подтвержден', color: '#4CAF50' },
      { value: 'Preparing', label: 'Готовится', color: '#2196F3' },
      { value: 'Ready', label: 'Готов к выдаче', color: '#FF9800' },
      { value: 'InDelivery', label: 'В доставке', color: '#9C27B0' },
      { value: 'Delivered', label: 'Доставлен', color: '#4CAF50' },
      { value: 'Cancelled', label: 'Отменен', color: '#F44336' }
    ];
  }

  formatOrderDate(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffInHours = (now.getTime() - date.getTime()) / (1000 * 60 * 60);

    if (diffInHours < 1) {
      const diffInMinutes = Math.floor(diffInHours * 60);
      return `${diffInMinutes} мин назад`;
    } else if (diffInHours < 24) {
      return `${Math.floor(diffInHours)} ч назад`;
    } else if (diffInHours < 48) {
      return 'Вчера';
    } else {
      return date.toLocaleDateString('ru-RU', {
        day: 'numeric',
        month: 'short',
        year: date.getFullYear() !== now.getFullYear() ? 'numeric' : undefined
      });
    }
  }

  formatOrderTime(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleTimeString('ru-RU', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
      minimumFractionDigits: 0,
      maximumFractionDigits: 2
    }).format(amount);
  }

  calculateTotalPrice(items: { price: number; quantity: number }[]): number {
    return items.reduce((total, item) => total + (item.price * item.quantity), 0);
  }

  getEstimatedDeliveryTime(orderTime?: string): string {
    const baseTime = orderTime ? new Date(orderTime) : new Date();
    const estimated = new Date(baseTime.getTime() + (30 * 60 * 1000)); // +30 минут
    return estimated.toLocaleTimeString('ru-RU', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getOrderNumber(orderId: string): string {
    // Создаем читаемый номер заказа из ID
    if (!orderId) return '#000000';

    // Берем первые 8 символов ID и делаем uppercase
    const shortId = orderId.substring(0, 8).toUpperCase();
    return `#${shortId}`;
  }

  // Проверки и валидация - ОБНОВЛЕНО под ваши статусы
  canUpdateStatus(currentStatus: string, newStatus: string): boolean {
    const current = currentStatus.toUpperCase();
    const next = newStatus.toUpperCase();

    const statusFlow: { [key: string]: string[] } = {
      'PENDING': ['CONFIRMED', 'CANCELLED'],
      'CONFIRMED': ['PREPARING', 'CANCELLED'],
      'PREPARING': ['READY', 'CANCELLED'],
      'READY': ['INDELIVERY', 'DELIVERED'],
      'INDELIVERY': ['DELIVERED'],
      'DELIVERED': [],
      'CANCELLED': []
    };

    return statusFlow[current]?.includes(next) || false;
  }

  canCancelOrder(status: string): boolean {
    const normalizedStatus = status.toUpperCase();
    const cancellableStatuses = ['PENDING', 'CONFIRMED', 'PREPARING'];
    return cancellableStatuses.includes(normalizedStatus);
  }

  isOrderActive(status: string): boolean {
    const normalizedStatus = status.toUpperCase();
    const activeStatuses = ['PENDING', 'CONFIRMED', 'PREPARING', 'READY', 'INDELIVERY'];
    return activeStatuses.includes(normalizedStatus);
  }

  isOrderCompleted(status: string): boolean {
    return status.toUpperCase() === 'DELIVERED';
  }

  // Фильтрация и поиск
  filterOrders(orders: OrderResponse[], filters: {
    search?: string;
    status?: string;
    dateFrom?: string;
    dateTo?: string;
  }): OrderResponse[] {
    return orders.filter(order => {
      // Поиск по номеру заказа, названию ресторана
      if (filters.search) {
        const searchLower = filters.search.toLowerCase();
        const orderNumber = this.getOrderNumber(order.id).toLowerCase();
        const restaurantName = order.restaurant.name.toLowerCase();
        const deliveryAddress = order.deliveryAddress?.toLowerCase() || '';

        const searchMatch = orderNumber.includes(searchLower) ||
          restaurantName.includes(searchLower) ||
          deliveryAddress.includes(searchLower);

        if (!searchMatch) return false;
      }

      // Фильтр по статусу
      if (filters.status && order.status.toUpperCase() !== filters.status.toUpperCase()) {
        return false;
      }

      // Фильтр по дате
      if (filters.dateFrom) {
        const orderDate = new Date(order.createdAt);
        const fromDate = new Date(filters.dateFrom);
        if (orderDate < fromDate) return false;
      }

      if (filters.dateTo) {
        const orderDate = new Date(order.createdAt);
        const toDate = new Date(filters.dateTo);
        toDate.setHours(23, 59, 59, 999); // End of day
        if (orderDate > toDate) return false;
      }

      return true;
    });
  }

  // Экспорт данных
  exportOrdersToCSV(orders: OrderResponse[]): string {
    const headers = [
      'Номер заказа',
      'Дата',
      'Ресторан',
      'Пользователь',
      'Статус',
      'Сумма',
      'Скидка',
      'Итого'
    ].join(',');

    const rows = orders.map(order => [
      this.getOrderNumber(order.id),
      new Date(order.createdAt).toLocaleDateString('ru-RU'),
      `"${order.restaurant.name}"`,
      `"${order.userId}"`,
      this.getOrderStatusDisplay(order.status).text,
      order.totalPrice.toFixed(2),
      (order.discountAmount || 0).toFixed(2),
      (order.totalPrice - (order.discountAmount || 0)).toFixed(2)
    ].join(','));

    return [headers, ...rows].join('\n');
  }

  // Трекинг заказов
  trackOrder(orderId: string): Observable<TrackingInfo> {
    return this.http.get<TrackingInfo>(`${this.apiUrl}/${orderId}/tracking`)
      .pipe(catchError(this.handleError));
  }
}
