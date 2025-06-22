// src/app/features/order-tracking/order-tracking.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil, interval, switchMap, startWith } from 'rxjs';
import { OrderService, TrackingInfo } from '../../core/services/order.service';
import { Order } from '../../core/models'; // Используем существующий тип

@Component({
  selector: 'app-order-tracking',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-tracking.component.html',
  styleUrls: ['./order-tracking.component.css']
})
export class OrderTrackingComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  order: Order | null = null;
  trackingInfo: TrackingInfo | null = null;
  loading = true;
  error = '';
  orderId = '';

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private orderService: OrderService
  ) {}

  ngOnInit(): void {
    this.orderId = this.route.snapshot.params['id'];
    this.loadOrderData();
    this.startTracking();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadOrderData(): void {
    this.loading = true;
    this.error = '';

    this.orderService.getOrder(this.orderId).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (orderResponse) => {
        // Используем OrderResponse как Order (они совместимы)
        this.order = orderResponse;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading order:', error);
        this.error = 'Ошибка загрузки заказа';
        this.loading = false;
      }
    });
  }

  private startTracking(): void {
    // Обновляем информацию о трекинге каждые 30 секунд
    interval(30000).pipe(
      startWith(0),
      switchMap(() => this.orderService.trackOrder(this.orderId)),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (trackingInfo) => {
        this.trackingInfo = trackingInfo;
      },
      error: (error) => {
        console.error('Error tracking order:', error);
      }
    });
  }

  getOrderStatusInfo(status: string) {
    return this.orderService.getOrderStatusDisplay(status);
  }

  formatDeliveryTime(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleString('ru-RU', {
      day: 'numeric',
      month: 'long',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getOrderSteps() {
    const allSteps = [
      { key: 'pending', title: 'Заказ принят', icon: '⏳' },
      { key: 'confirmed', title: 'Подтвержден рестораном', icon: '✅' },
      { key: 'preparing', title: 'Готовится', icon: '👨‍🍳' },
      { key: 'ready', title: 'Готов к выдаче', icon: '📦' },
      { key: 'picked_up', title: 'Передан курьеру', icon: '🚗' },
      { key: 'in_delivery', title: 'В пути к вам', icon: '🚚' },
      { key: 'delivered', title: 'Доставлен', icon: '🎉' }
    ];

    const currentStatus = this.order?.status.toLowerCase();
    const statusOrder = ['pending', 'confirmed', 'preparing', 'ready', 'picked_up', 'in_delivery', 'delivered'];
    const currentIndex = statusOrder.indexOf(currentStatus || '');

    return allSteps.map((step, index) => ({
      ...step,
      completed: index < currentIndex,
      active: index === currentIndex,
      time: index === currentIndex ? 'Сейчас' : undefined
    }));
  }

  canCancelOrder(): boolean {
    if (!this.order) return false;
    const status = this.order.status.toLowerCase();
    return ['pending', 'confirmed'].includes(status);
  }

  cancelOrder(): void {
    if (confirm('Вы уверены, что хотите отменить заказ?')) {
      console.log('Cancel order:', this.orderId);
    }
  }

  goBack(): void {
    this.router.navigate(['/profile'], { queryParams: { tab: 'orders' } });
  }
}
