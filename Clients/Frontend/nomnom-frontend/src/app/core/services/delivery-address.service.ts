// Файл: src/app/core/services/delivery-address.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface DeliveryDetails {
  entrance: string;
  floor: string;
  apartment: string;
  comment: string;
}

export interface FullAddress {
  address: string;
  coordinates?: [number, number];
  details?: DeliveryDetails;
  formattedAddress: string; // Полный адрес с деталями
}

@Injectable({
  providedIn: 'root'
})
export class DeliveryAddressService {
  private currentAddressSubject = new BehaviorSubject<FullAddress | null>(null);
  public currentAddress$ = this.currentAddressSubject.asObservable();

  constructor() {
    // Загружаем сохраненный адрес при инициализации
    this.loadSavedAddress();
  }

  // Сохранить адрес доставки
  setDeliveryAddress(addressData: FullAddress): void {
    this.currentAddressSubject.next(addressData);
    // Сохраняем в localStorage для персистентности
    localStorage.setItem('delivery_address', JSON.stringify(addressData));
  }

  // Получить текущий адрес доставки
  getCurrentAddress(): FullAddress | null {
    return this.currentAddressSubject.value;
  }

  // Проверить, есть ли установленный адрес
  hasAddress(): boolean {
    return this.currentAddressSubject.value !== null;
  }

  // Очистить адрес доставки
  clearAddress(): void {
    this.currentAddressSubject.next(null);
    localStorage.removeItem('delivery_address');
  }

  // Получить только строку адреса для отображения
  getFormattedAddress(): string {
    const address = this.getCurrentAddress();
    return address?.formattedAddress || '';
  }

  // Загрузить сохраненный адрес из localStorage
  private loadSavedAddress(): void {
    try {
      const saved = localStorage.getItem('delivery_address');
      if (saved) {
        const addressData = JSON.parse(saved) as FullAddress;
        this.currentAddressSubject.next(addressData);
      }
    } catch (error) {
      console.error('Error loading saved address:', error);
      localStorage.removeItem('delivery_address');
    }
  }
}
