// src/app/core/services/cart.service.ts

import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { MenuItemResponse } from '../models/restaurant.models';

export interface CartItem {
  id: string;
  name: string;
  price: number;
  weight: string;
  quantity: number;
  imageUrl: string;
  restaurantId: string;
  restaurantName?: string;
}

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly key = 'cart_items';
  private readonly restaurantKey = 'cart_restaurant_id';
  private readonly restaurantNameKey = 'cart_restaurant_name';

  private itemsSubject = new BehaviorSubject<CartItem[]>(this.loadCart());
  items$ = this.itemsSubject.asObservable();

  constructor() {}

  getItems(): CartItem[] {
    return this.itemsSubject.value;
  }

  addItem(item: CartItem): void {
    const items = [...this.itemsSubject.value];

    // Проверяем, что добавляем товар из того же ресторана
    const currentRestaurantId = this.getRestaurantId();
    if (currentRestaurantId && currentRestaurantId !== item.restaurantId) {
      // Очищаем корзину если товар из другого ресторана
      this.clear();
    }

    const existing = items.find(i => i.id === item.id);
    if (existing) {
      existing.quantity += item.quantity;
    } else {
      items.push(item);
    }

    this.updateItems(items);
    this.setRestaurantInfo(item.restaurantId, item.restaurantName);
  }

  removeItem(id: string): void {
    const updated = this.itemsSubject.value.filter(i => i.id !== id);
    this.updateItems(updated);

    // Если корзина пуста, очищаем информацию о ресторане
    if (updated.length === 0) {
      this.clearRestaurantInfo();
    }
  }

  updateQuantity(id: string, quantity: number): void {
    if (quantity <= 0) {
      this.removeItem(id);
      return;
    }

    const items = [...this.itemsSubject.value];
    const item = items.find(i => i.id === id);
    if (item) {
      item.quantity = quantity;
      this.updateItems(items);
    }
  }

  clear(): void {
    this.updateItems([]);
    this.clearRestaurantInfo();
  }

  getRestaurantId(): string | null {
    return localStorage.getItem(this.restaurantKey);
  }

  getRestaurantName(): string | null {
    return localStorage.getItem(this.restaurantNameKey);
  }

  getTotalPrice(): number {
    return this.itemsSubject.value.reduce((sum, item) => sum + (item.price * item.quantity), 0);
  }

  getItemCount(): number {
    return this.itemsSubject.value.reduce((sum, item) => sum + item.quantity, 0);
  }

  // Проверка совместимости с рестораном
  canAddFromRestaurant(restaurantId: string): boolean {
    const currentRestaurantId = this.getRestaurantId();
    return !currentRestaurantId || currentRestaurantId === restaurantId || this.getItems().length === 0;
  }

  // Конвертация MenuItem в CartItem
  createCartItemFromMenuItem(menuItem: MenuItemResponse, quantity: number = 1): CartItem {
    return {
      id: menuItem.id,
      name: menuItem.name,
      price: menuItem.price,
      weight: '1 порция', // Можно добавить это поле в MenuItemResponse
      quantity: quantity,
      imageUrl: menuItem.imageUrl || '',
      restaurantId: menuItem.restaurantId,
      restaurantName: menuItem.restaurantName
    };
  }

  // Подготовка данных для заказа
  prepareOrderItems(): Array<{ menuItemId: string; quantity: number }> {
    return this.getItems().map(item => ({
      menuItemId: item.id,
      quantity: item.quantity
    }));
  }

  // Проверка минимальной суммы заказа
  checkMinimumOrder(minimumAmount: number = 500): { isValid: boolean; missing: number } {
    const total = this.getTotalPrice();
    return {
      isValid: total >= minimumAmount,
      missing: Math.max(0, minimumAmount - total)
    };
  }

  // Расчет стоимости доставки
  calculateDeliveryFee(freeDeliveryThreshold: number = 1000): number {
    const total = this.getTotalPrice();
    return total >= freeDeliveryThreshold ? 0 : 99;
  }

  // Получение итоговой суммы с доставкой
  getTotalWithDelivery(serviceCharge: number = 0, deliveryFee?: number): number {
    const itemsTotal = this.getTotalPrice();
    const delivery = deliveryFee !== undefined ? deliveryFee : this.calculateDeliveryFee();
    return itemsTotal + serviceCharge + delivery;
  }

  private loadCart(): CartItem[] {
    const saved = localStorage.getItem(this.key);
    return saved ? JSON.parse(saved) : [];
  }

  private updateItems(items: CartItem[]): void {
    this.itemsSubject.next(items);
    localStorage.setItem(this.key, JSON.stringify(items));
  }

  private setRestaurantInfo(restaurantId: string, restaurantName?: string): void {
    localStorage.setItem(this.restaurantKey, restaurantId);
    if (restaurantName) {
      localStorage.setItem(this.restaurantNameKey, restaurantName);
    }
  }

  private clearRestaurantInfo(): void {
    localStorage.removeItem(this.restaurantKey);
    localStorage.removeItem(this.restaurantNameKey);
  }
}
