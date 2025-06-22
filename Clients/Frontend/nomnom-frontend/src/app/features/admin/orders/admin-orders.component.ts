// src/app/features/admin/orders/admin-orders.component.ts

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, takeUntil, finalize } from 'rxjs';
import { OrderService } from '../../../core/services/order.service';
import { OrderResponse } from '../../../core/models/order.models';
import { InputComponent } from '../../../shared/components/input/input.component';

interface OrderFilters {
  search: string;
  status?: string;
  dateFrom?: string;
  dateTo?: string;
  sortBy: 'createdAt' | 'totalPrice' | 'status';
  sortOrder: 'asc' | 'desc';
}

interface ModalState {
  isOpen: boolean;
  title: string;
  type: 'view' | 'status' | 'cancel';
  data: OrderResponse | null;
}

interface StatusUpdateForm {
  newStatus: string;
  notes: string;
}

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, InputComponent],
  templateUrl: './admin-orders.component.html',
  styleUrls: ['./admin-orders.component.css']
})
export class AdminOrdersComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  orders: OrderResponse[] = [];
  filteredOrders: OrderResponse[] = [];
  loading = false;
  error = '';

  // Modal state
  modalState: ModalState = {
    isOpen: false,
    title: '',
    type: 'view',
    data: null
  };

  // Forms
  statusUpdateForm: StatusUpdateForm = {
    newStatus: '',
    notes: ''
  };

  cancelReason = '';

  // Filters
  filters: OrderFilters = {
    search: '',
    status: undefined,
    dateFrom: undefined,
    dateTo: undefined,
    sortBy: 'createdAt',
    sortOrder: 'desc'
  };

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadOrders(): void {
    this.loading = true;
    this.error = '';

    this.orderService.getOrders()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.loading = false)
      )
      .subscribe({
        next: (orders) => {
          console.log('Loaded orders:', orders); // Для отладки
          this.orders = orders;
          this.applyFilters();
        },
        error: (error) => {
          console.error('Error loading orders:', error);
          this.error = 'Ошибка при загрузке заказов: ' + (error.message || 'Неизвестная ошибка');
          this.orders = [];
          this.filteredOrders = [];
        }
      });
  }

  applyFilters(): void {
    let filtered = [...this.orders];

    // Поиск
    if (this.filters.search?.trim()) {
      const searchLower = this.filters.search.toLowerCase();
      filtered = filtered.filter(order =>
        this.getOrderNumber(order.id).toLowerCase().includes(searchLower) ||
        order.restaurant.name.toLowerCase().includes(searchLower) ||
        order.deliveryAddress.toLowerCase().includes(searchLower)
      );
    }

    // Фильтр по статусу
    if (this.filters.status) {
      filtered = filtered.filter(order => order.status === this.filters.status);
    }

    // Фильтр по дате
    if (this.filters.dateFrom) {
      const fromDate = new Date(this.filters.dateFrom);
      filtered = filtered.filter(order => new Date(order.createdAt) >= fromDate);
    }

    if (this.filters.dateTo) {
      const toDate = new Date(this.filters.dateTo);
      toDate.setHours(23, 59, 59, 999);
      filtered = filtered.filter(order => new Date(order.createdAt) <= toDate);
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

    this.filteredOrders = filtered;
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

  clearFilters(): void {
    this.filters = {
      search: '',
      status: undefined,
      dateFrom: undefined,
      dateTo: undefined,
      sortBy: 'createdAt',
      sortOrder: 'desc'
    };
    this.applyFilters();
  }

  refreshData(): void {
    this.loadOrders();
  }

  // Modal methods
  openViewModal(order: OrderResponse): void {
    this.modalState = {
      isOpen: true,
      title: `Заказ ${this.getOrderNumber(order.id)}`,
      type: 'view',
      data: order
    };
  }

  openStatusModal(order: OrderResponse): void {
    this.modalState = {
      isOpen: true,
      title: 'Изменить статус заказа',
      type: 'status',
      data: order
    };
    this.statusUpdateForm = {
      newStatus: order.status,
      notes: ''
    };
  }

  openCancelModal(order: OrderResponse): void {
    this.modalState = {
      isOpen: true,
      title: 'Отменить заказ',
      type: 'cancel',
      data: order
    };
    this.cancelReason = '';
  }

  closeModal(): void {
    this.modalState.isOpen = false;
    this.statusUpdateForm = {
      newStatus: '',
      notes: ''
    };
    this.cancelReason = '';
  }

  // Order actions
  updateOrderStatus(): void {
    if (this.modalState.data && this.statusUpdateForm.newStatus) {
      const orderId = this.modalState.data.id;

      this.orderService.updateOrderStatus(
        orderId,
        this.statusUpdateForm.newStatus,
        this.statusUpdateForm.notes || undefined
      )
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (updatedOrder) => {
            // Обновляем локальные данные
            const index = this.orders.findIndex(o => o.id === orderId);
            if (index !== -1) {
              this.orders[index] = updatedOrder;
              this.applyFilters();
            }
            this.closeModal();
          },
          error: (error) => {
            console.error('Error updating order status:', error);
            this.error = 'Ошибка при обновлении статуса заказа: ' + (error.message || 'Неизвестная ошибка');
          }
        });
    }
  }

  cancelOrder(): void {
    if (this.modalState.data && this.cancelReason?.trim()) {
      const orderId = this.modalState.data.id;

      this.orderService.cancelOrder(orderId, this.cancelReason)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (updatedOrder) => {
            // Обновляем локальные данные
            const index = this.orders.findIndex(o => o.id === orderId);
            if (index !== -1) {
              this.orders[index] = updatedOrder;
              this.applyFilters();
            }
            this.closeModal();
          },
          error: (error) => {
            console.error('Error cancelling order:', error);
            this.error = 'Ошибка при отмене заказа: ' + (error.message || 'Неизвестная ошибка');
          }
        });
    }
  }

  // Utility methods
  getOrderNumber(orderId: string): string {
    return this.orderService.getOrderNumber(orderId);
  }

  getStatusText(status: string): string {
    return this.orderService.getOrderStatusDisplay(status).text;
  }

  getStatusClass(status: string): string {
    const statusInfo = this.orderService.getOrderStatusDisplay(status);
    return `status-${status.toLowerCase().replace('_', '-')}`;
  }

  canChangeStatus(currentStatus: string): boolean {
    // Нельзя изменять статус доставленных и отмененных заказов
    return !['DELIVERED', 'CANCELLED'].includes(currentStatus.toUpperCase());
  }

  getAvailableStatuses(currentStatus: string): { value: string; label: string }[] {
    return this.orderService.getStatusOptions().filter(status =>
      this.orderService.canUpdateStatus(currentStatus, status.value)
    );
  }

  formatCurrency(amount: number): string {
    return this.orderService.formatCurrency(amount);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('ru-RU');
  }

  formatDateTime(dateString: string): string {
    return new Date(dateString).toLocaleString('ru-RU');
  }

  getSortIcon(sortBy: string): string {
    if (this.filters.sortBy !== sortBy) return '↕️';
    return this.filters.sortOrder === 'asc' ? '↑' : '↓';
  }

  // Stats methods
  getTotalOrders(): number {
    return this.orders.length;
  }

  getPendingOrders(): number {
    return this.orders.filter(o => ['PENDING', 'CONFIRMED'].includes(o.status.toUpperCase())).length;
  }

  getActiveOrders(): number {
    return this.orders.filter(o =>
      ['PREPARING', 'READY_FOR_PICKUP', 'OUT_FOR_DELIVERY'].includes(o.status.toUpperCase())
    ).length;
  }

  getCompletedOrders(): number {
    return this.orders.filter(o => o.status.toUpperCase() === 'DELIVERED').length;
  }

  getTotalRevenue(): number {
    return this.orders
      .filter(o => o.status.toUpperCase() === 'DELIVERED')
      .reduce((sum, order) => sum + (order.totalPrice - (order.discountAmount || 0)), 0);
  }

  trackOrder(index: number, order: OrderResponse): string {
    return order.id;
  }

  // Получаем список всех доступных статусов для фильтра
  getAllStatusOptions(): { value: string; label: string }[] {
    return this.orderService.getStatusOptions();
  }
}
