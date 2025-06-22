// src/app/shared/components/header/header.component.ts
import { Component, ViewChild, OnInit, OnDestroy, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { AddressModalComponent } from '../../features/home/address-modal/address-modal.component';
import { CartModalComponent } from '../../shared/components/cart-modal/ cart-modal.component'; // Исправленный путь
import { CartService } from '../../core/services/cart.service';
import { DeliveryAddressService } from '../../core/services/delivery-address.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, FormsModule, AddressModalComponent, CartModalComponent],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  @ViewChild('addressModal') addressModal!: AddressModalComponent;
  @ViewChild('cartModal') cartModal!: CartModalComponent;

  // Выводим событие поиска для родительского компонента
  @Output() searchPerformed = new EventEmitter<string>();

  searchQuery: string = '';

  // Данные корзины из сервиса
  cartTotal: number = 0;
  cartItemCount: number = 0;

  // Данные адреса из сервиса
  currentAddress: string = 'Выберите адрес доставки';

  constructor(
    private router: Router,
    private cartService: CartService,
    private deliveryAddressService: DeliveryAddressService
  ) {}

  ngOnInit() {
    // Подписываемся на изменения корзины
    this.cartService.items$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(items => {
      this.cartTotal = this.cartService.getTotalPrice();
      this.cartItemCount = this.cartService.getItemCount();
    });

    // Подписываемся на изменения адреса
    this.deliveryAddressService.currentAddress$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(address => {
      this.currentAddress = address?.formattedAddress || 'Выберите адрес доставки';
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  openCart() {
    this.cartModal.show();
  }

  onSearch(): void {
    if (this.searchQuery.trim()) {
      console.log('Search query:', this.searchQuery);
      // Передаем событие поиска родительскому компоненту
      this.searchPerformed.emit(this.searchQuery.trim());
    }
  }

  goToProfile(): void {
    console.log('Переход в профиль');
    this.router.navigate(['/profile']);
  }

  showAddressSelect(): void {
    this.addressModal.show();
  }

  onAddressSelected(newAddress: string): void {
    // Этот метод может понадобиться для совместимости
    console.log('Новый адрес выбран:', newAddress);
  }

  // Геттеры для шаблона
  get hasItemsInCart(): boolean {
    return this.cartItemCount > 0;
  }

  get formattedCartTotal(): string {
    return this.cartTotal.toFixed(2);
  }
}
