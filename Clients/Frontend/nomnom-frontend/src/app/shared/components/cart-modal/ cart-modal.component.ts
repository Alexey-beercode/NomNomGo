// src/app/shared/components/cart-modal/cart-modal.component.ts
import { Component, EventEmitter, Output, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrderService } from '../../../core/services/order.service';
import { AuthService } from '../../../core/services/auth.service';
import { DeliveryAddressService } from '../../../core/services/delivery-address.service';
import { Router } from '@angular/router';
import { CartItem, CartService } from '../../../core/services/cart.service';
import { Subject, takeUntil, finalize } from 'rxjs';

@Component({
  selector: 'app-cart-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cart-modal.component.html',
  styleUrls: ['./cart-modal.component.css']
})
export class CartModalComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  @Output() closeModal = new EventEmitter<void>();

  isVisible: boolean = false;
  isLoading: boolean = false;
  serviceCharge: number = 0.99;
  deliveryFree: boolean = true;
  deliveryMinOrder: number = 15;
  deliveryTime: string = '40 мин'; // Статичное время доставки
  error: string = '';

  cartItems: CartItem[] = [];

  // Дефолтное изображение для товаров
  private defaultFoodImage = 'https://foni.papik.pro/uploads/posts/2024-09/foni-papik-pro-l126-p-kartinki-yeda-na-prozrachnom-fone-2.png';

  constructor(
    private orderService: OrderService,
    private authService: AuthService,
    private deliveryService: DeliveryAddressService,
    private router: Router,
    private cartService: CartService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    // Подписываемся на изменения корзины
    this.cartService.items$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(items => {
      this.cartItems = items;
      this.cdr.detectChanges(); // Принудительно обновляем детекцию изменений
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get restaurantName(): string {
    return this.cartService.getRestaurantName() || 'Ресторан';
  }

  get subtotal(): number {
    return this.cartService.getTotalPrice();
  }

  get totalPrice(): number {
    return this.cartService.getTotalWithDelivery(this.serviceCharge);
  }

  get canProceedToCheckout(): boolean {
    return this.cartItems.length > 0 &&
      this.subtotal >= this.deliveryMinOrder &&
      !this.isLoading;
  }

  show() {
    this.isVisible = true;
    this.error = '';
    document.body.style.overflow = 'hidden';
  }

  hide() {
    this.isVisible = false;
    this.error = '';
    document.body.style.overflow = '';
    this.closeModal.emit();
  }

  // Добавляем обработчик клика по backdrop
  onBackdropClick(event: MouseEvent) {
    if (event.target === event.currentTarget) {
      this.hide();
    }
  }

  clearCart() {
    if (confirm('Вы уверены, что хотите очистить корзину?')) {
      this.cartService.clear();
    }
  }

  incrementQuantity(item: CartItem) {
    this.cartService.updateQuantity(item.id, item.quantity + 1);
  }

  decrementQuantity(item: CartItem) {
    if (item.quantity > 1) {
      this.cartService.updateQuantity(item.id, item.quantity - 1);
    } else {
      this.removeItem(item);
    }
  }

  removeItem(item: CartItem) {
    this.cartService.removeItem(item.id);
  }

  // Получение изображения с fallback
  getItemImage(item: CartItem): string {
    return item.imageUrl || this.defaultFoodImage;
  }

  // Обработчик ошибки загрузки изображения
  onImageError(event: Event): void {
    const target = event.target as HTMLImageElement;
    if (target) {
      target.src = this.defaultFoodImage;
    }
  }

  proceedToCheckout() {
    this.error = '';

    const currentUser = this.authService.getCurrentUserValue();
    if (!currentUser) {
      this.error = 'Необходимо войти в систему';
      setTimeout(() => {
        this.router.navigate(['/login']);
      }, 2000);
      return;
    }

    const address = this.deliveryService.getCurrentAddress();
    if (!address) {
      this.error = 'Необходимо выбрать адрес доставки';
      return;
    }

    const restaurantId = this.cartService.getRestaurantId();
    if (!restaurantId) {
      this.error = 'Ошибка: не выбран ресторан';
      return;
    }

    if (this.cartItems.length === 0) {
      this.error = 'Корзина пуста';
      return;
    }

    if (this.subtotal < this.deliveryMinOrder) {
      this.error = `Минимальная сумма заказа: ${this.deliveryMinOrder}₽`;
      return;
    }

    this.isLoading = true;

    const payload = {
      userId: currentUser.userId,
      restaurantId: restaurantId,
      deliveryAddress: address.formattedAddress,
      items: this.cartItems.map(item => ({
        menuItemId: item.id,
        quantity: item.quantity
      }))
    };

    console.log('Creating order with payload:', payload);

    this.orderService.createOrder(payload).pipe(
      takeUntil(this.destroy$),
      finalize(() => {
        this.isLoading = false;
      })
    ).subscribe({
      next: (order) => {
        console.log('Order created successfully:', order);
        this.cartService.clear();
        this.hide();
        this.router.navigate(['/tracking', order.id]);
      },
      error: (err) => {
        console.error('Ошибка при создании заказа:', err);
        this.error = err.message || 'Не удалось оформить заказ. Попробуйте еще раз.';
      }
    });
  }

  // Утилиты для форматирования
  formatPrice(price: number): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
      minimumFractionDigits: 0,
      maximumFractionDigits: 2
    }).format(price);
  }

  getItemTotal(item: CartItem): number {
    return item.price * item.quantity;
  }

  // Закрытие модального окна по Escape
  onKeyDown(event: KeyboardEvent) {
    if (event.key === 'Escape') {
      this.hide();
    }
  }

  // TrackBy функция для оптимизации списка
  trackByItemId(index: number, item: CartItem): string {
    return item.id;
  }
}
