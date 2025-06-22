// src/app/features/orders/order-detail/order-detail.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { OrderService } from '../../../core/services/order.service';
import { Order } from '../../../core/models'; // Используем существующий тип

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.css']
})
export class OrderDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  order: Order | null = null;
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
    this.loadOrder();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadOrder(): void {
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

  getOrderStatusInfo(status: string) {
    return this.orderService.getOrderStatusDisplay(status);
  }

  formatOrderDate(dateString: string): string {
    return this.orderService.formatOrderDate(dateString);
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

  getStars(rating: number): string[] {
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 >= 0.5;
    const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

    return [
      ...Array(fullStars).fill('★'),
      ...(hasHalfStar ? ['☆'] : []),
      ...Array(emptyStars).fill('☆')
    ];
  }

  getItemsTotal(): number {
    if (!this.order) return 0;
    return this.order.items.reduce((total: number, item: any) => total + (item.price * item.quantity), 0);
  }

  trackOrder(): void {
    this.router.navigate(['/tracking', this.orderId]);
  }

  reorderItems(): void {
    console.log('Reorder items from order:', this.orderId);
    alert('Товары добавлены в корзину');
  }

  leaveReview(): void {
    console.log('Leave review for order:', this.orderId);
    alert('Функция оставления отзыва будет доступна в следующем обновлении');
  }

  goBack(): void {
    this.router.navigate(['/profile'], { queryParams: { tab: 'orders' } });
  }
}
