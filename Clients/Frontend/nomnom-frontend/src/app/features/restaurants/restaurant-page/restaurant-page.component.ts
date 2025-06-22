// src/app/features/restaurants/restaurant-page/restaurant-page.component.ts

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { HeaderComponent } from '../../../layouts/header/header.component';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { CartService, CartItem } from '../../../core/services/cart.service';
import { AuthService } from '../../../core/services/auth.service';
import { DeliveryAddressService } from '../../../core/services/delivery-address.service';
import { OrderService } from '../../../core/services/order.service';
import { RestaurantResponse, MenuItemResponse } from '../../../core/models/restaurant.models';

@Component({
  selector: 'app-restaurant-page',
  standalone: true,
  imports: [CommonModule, HeaderComponent],
  templateUrl: './restaurant-page.component.html',
  styleUrls: ['./restaurant-page.component.css']
})
export class RestaurantPageComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  restaurant: RestaurantResponse | null = null;
  menuItems: MenuItemResponse[] = [];
  filteredMenuItems: MenuItemResponse[] = [];
  categories: string[] = [];
  activeCategory: string = 'Все';
  loading = true;
  error = '';

  // Корзина
  cartItems: CartItem[] = [];

  // Дефолтные изображения
  private defaultRestaurantImage = 'https://foni.papik.pro/uploads/posts/2024-09/foni-papik-pro-l126-p-kartinki-yeda-na-prozrachnom-fone-2.png';
  private defaultFoodImage = 'https://foni.papik.pro/uploads/posts/2024-09/foni-papik-pro-l126-p-kartinki-yeda-na-prozrachnom-fone-2.png';

  constructor(
    private restaurantService: RestaurantService,
    private route: ActivatedRoute,
    private router: Router,
    private cartService: CartService,
    private authService: AuthService,
    private deliveryAddressService: DeliveryAddressService,
    private orderService: OrderService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    this.loadRestaurantData(id);
    this.subscribeToCart();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private subscribeToCart(): void {
    this.cartService.items$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(items => {
      this.cartItems = items;
    });
  }

  private loadRestaurantData(id: string): void {
    this.loading = true;
    this.error = '';

    // Загружаем данные ресторана
    this.restaurantService.getRestaurant(id).subscribe({
      next: (data) => {
        this.restaurant = data;
      },
      error: (error) => {
        console.error('Error loading restaurant:', error);
        this.error = 'Ошибка загрузки ресторана';
        this.loading = false;
      }
    });

    // Загружаем меню
    this.restaurantService.getMenu(id).subscribe({
      next: (items) => {
        this.menuItems = items;
        this.filteredMenuItems = items;
        this.extractCategories(items);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading menu:', error);
        this.error = 'Ошибка загрузки меню';
        this.loading = false;
      }
    });
  }

  private extractCategories(items: MenuItemResponse[]): void {
    const categorySet = new Set(['Все']);
    items.forEach(item => {
      if (item.categoryName) {
        categorySet.add(item.categoryName);
      }
    });
    this.categories = Array.from(categorySet);
  }

  selectedCategory(category: string): void {
    this.activeCategory = category;

    if (category === 'Все') {
      this.filteredMenuItems = this.menuItems;
    } else {
      this.filteredMenuItems = this.menuItems.filter(
        item => item.categoryName === category
      );
    }
  }

  addToCart(item: MenuItemResponse): void {
    if (!this.restaurant) return;

    // Проверяем, можно ли добавить товар из этого ресторана
    if (!this.cartService.canAddFromRestaurant(item.restaurantId)) {
      const confirmSwitch = confirm(
        'В корзине есть товары из другого ресторана. Очистить корзину и добавить этот товар?'
      );
      if (!confirmSwitch) return;
    }

    const cartItem = this.cartService.createCartItemFromMenuItem(item);
    cartItem.restaurantName = this.restaurant.name;
    this.cartService.addItem(cartItem);

    // Показываем уведомление
    this.showAddedToCartNotification(item.name);
  }

  // Управление количеством в корзине
  incrementQuantity(item: CartItem): void {
    this.cartService.updateQuantity(item.id, item.quantity + 1);
  }

  decrementQuantity(item: CartItem): void {
    if (item.quantity > 1) {
      this.cartService.updateQuantity(item.id, item.quantity - 1);
    } else {
      this.cartService.removeItem(item.id);
    }
  }

  // Оформление заказа
  proceedToCheckout(): void {
    const currentUser = this.authService.getCurrentUserValue();
    const address = this.deliveryAddressService.getCurrentAddress();
    const restaurantId = this.cartService.getRestaurantId();

    // Проверяем авторизацию
    if (!currentUser) {
      alert('Для оформления заказа необходимо войти в систему');
      this.router.navigate(['/login']);
      return;
    }

    // Проверяем адрес доставки
    if (!address) {
      alert('Необходимо выбрать адрес доставки');
      // Можно открыть модальное окно выбора адреса
      return;
    }

    // Проверяем корзину
    if (!restaurantId || this.cartItems.length === 0) {
      alert('Корзина пуста');
      return;
    }

    // Создаем заказ
    const orderData = {
      userId: currentUser.userId,
      restaurantId: restaurantId,
      deliveryAddress: address.formattedAddress,
      notes: '', // Можно добавить поле для комментариев
      items: this.cartItems.map(item => ({
        menuItemId: item.id,
        quantity: item.quantity
      }))
    };

    this.orderService.createOrder(orderData).subscribe({
      next: (order) => {
        // Очищаем корзину
        this.cartService.clear();

        // Показываем успешное сообщение
        alert('Заказ успешно оформлен!');

        // Перенаправляем на страницу отслеживания
        this.router.navigate(['/tracking', order.id]);
      },
      error: (error) => {
        console.error('Ошибка при создании заказа:', error);
        alert('Не удалось оформить заказ. Попробуйте еще раз.');
      }
    });
  }

  private showAddedToCartNotification(itemName: string): void {
    // Простое уведомление, можно заменить на toast или snackbar
    console.log(`${itemName} добавлен в корзину`);

    // Можно добавить временное уведомление в UI
    const notification = document.createElement('div');
    notification.textContent = `${itemName} добавлен в корзину`;
    notification.style.cssText = `
      position: fixed;
      top: 20px;
      right: 20px;
      background: #4CAF50;
      color: white;
      padding: 12px 20px;
      border-radius: 6px;
      z-index: 10000;
      animation: slideIn 0.3s ease-out;
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
      notification.style.animation = 'slideOut 0.3s ease-out';
      setTimeout(() => {
        document.body.removeChild(notification);
      }, 300);
    }, 2000);
  }

  // Утилиты для шаблона
  getItemImage(item: MenuItemResponse | CartItem): string {
    return item.imageUrl || this.defaultFoodImage;
  }

  getRestaurantImage(): string {
    return this.restaurant?.imageUrl || this.defaultRestaurantImage;
  }

  getDeliveryTime(): string {
    if (this.restaurant) {
      return this.restaurantService.getDeliveryTime(this.restaurant);
    }
    return '30-45 мин';
  }

  getRestaurantRating(): number {
    return this.restaurant?.averageRating || 0;
  }

  getRestaurantReviewCount(): number {
    return this.restaurant?.reviewCount || 0;
  }

  formatPrice(price: number): string {
    return this.restaurantService.formatPrice(price);
  }

  isRestaurantOpen(): boolean {
    return this.restaurant ? this.restaurantService.isRestaurantOpen(this.restaurant) : false;
  }

  // Проверки для отображения UI элементов
  hasDiscount(item: MenuItemResponse): boolean {
    return !!(item as any).discount && (item as any).discount > 0;
  }

  getDiscountPercent(item: MenuItemResponse): number {
    return (item as any).discount || 0;
  }

  hasOldPrice(item: MenuItemResponse): boolean {
    return !!(item as any).oldPrice;
  }

  getOldPrice(item: MenuItemResponse): number {
    return (item as any).oldPrice || 0;
  }

  // Геттеры для корзины
  get cartItemCount(): number {
    return this.cartService.getItemCount();
  }

  get cartTotal(): number {
    return this.cartService.getTotalPrice();
  }

  get hasItemsInCart(): boolean {
    return this.cartItemCount > 0;
  }
}
