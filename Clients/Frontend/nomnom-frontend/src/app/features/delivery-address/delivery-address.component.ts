// Файл: src/app/shared/components/delivery-address/delivery-address.component.ts
import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { DeliveryAddressService, FullAddress } from '../../core/services/delivery-address.service';
import { AddressModalComponent } from '../../features/home/address-modal/address-modal.component';

@Component({
  selector: 'app-delivery-address',
  standalone: true,
  imports: [CommonModule, AddressModalComponent],
  template: `
    <div class="delivery-address-container">
      <div class="address-header">
        <div class="address-icon">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
            <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" stroke="currentColor" stroke-width="2"/>
            <circle cx="12" cy="10" r="3" stroke="currentColor" stroke-width="2"/>
          </svg>
        </div>
        <span class="address-label">Адрес доставки</span>
      </div>

      <div class="address-content" *ngIf="currentAddress; else noAddress">
        <div class="selected-address">
          <p class="address-text">{{ currentAddress.formattedAddress }}</p>
          <div class="address-actions">
            <button class="change-button" (click)="openAddressModal()">
              Изменить
            </button>
          </div>
        </div>
      </div>

      <ng-template #noAddress>
        <button class="select-address-button" (click)="openAddressModal()">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
            <path d="M12 5v14M5 12h14" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
          </svg>
          Выбрать адрес доставки
        </button>
      </ng-template>

      <!-- Модальное окно выбора адреса -->
      <app-address-modal
        #addressModal
        (addressSelected)="onAddressSelected($event)">
      </app-address-modal>
    </div>
  `,
  styles: [`
    .delivery-address-container {
      background: white;
      border-radius: var(--radius, 16px);
      padding: 20px;
      margin-bottom: 16px;
      box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
    }

    .address-header {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 16px;
    }

    .address-icon {
      color: var(--primary-color, #2ecc71);
    }

    .address-label {
      font-size: 16px;
      font-weight: 600;
      color: var(--text-color, #333);
    }

    .selected-address {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 16px;
    }

    .address-text {
      flex: 1;
      margin: 0;
      font-size: 14px;
      color: var(--text-color, #333);
      line-height: 1.4;
    }

    .change-button {
      background: none;
      border: 1px solid var(--primary-color, #2ecc71);
      color: var(--primary-color, #2ecc71);
      padding: 8px 16px;
      border-radius: 8px;
      font-size: 14px;
      cursor: pointer;
      transition: all 0.2s;
      flex-shrink: 0;
    }

    .change-button:hover {
      background-color: var(--primary-color, #2ecc71);
      color: white;
    }

    .select-address-button {
      width: 100%;
      background: none;
      border: 2px dashed var(--border-color, #e0e0e0);
      color: var(--text-secondary, #666);
      padding: 16px;
      border-radius: 8px;
      font-size: 14px;
      cursor: pointer;
      transition: all 0.2s;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
    }

    .select-address-button:hover {
      border-color: var(--primary-color, #2ecc71);
      color: var(--primary-color, #2ecc71);
    }

    @media (max-width: 768px) {
      .selected-address {
        flex-direction: column;
        gap: 12px;
      }

      .change-button {
        align-self: flex-start;
      }
    }
  `]
})
export class DeliveryAddressComponent implements OnInit, OnDestroy {
  @ViewChild('addressModal') addressModal!: AddressModalComponent;

  private destroy$ = new Subject<void>();
  currentAddress: FullAddress | null = null;

  constructor(private deliveryAddressService: DeliveryAddressService) {}

  ngOnInit(): void {
    // Подписываемся на изменения адреса
    this.deliveryAddressService.currentAddress$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(address => {
      this.currentAddress = address;
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  openAddressModal(): void {
    this.addressModal.show();
  }

  onAddressSelected(address: string): void {
    // Адрес уже сохранен в сервисе через AddressModalComponent
    console.log('Address selected:', address);
  }

  // Методы для использования в других компонентах
  hasAddress(): boolean {
    return this.deliveryAddressService.hasAddress();
  }

  getAddressForOrder(): string {
    return this.deliveryAddressService.getFormattedAddress();
  }
}
