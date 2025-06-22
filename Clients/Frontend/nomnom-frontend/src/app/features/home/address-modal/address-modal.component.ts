// Файл: src/app/features/address-modal/address-modal.component.ts
import { Component, EventEmitter, Output, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MapComponent } from '../../../shared/components/map/map.component';
import { DeliveryAddressService, DeliveryDetails, FullAddress } from '../../../core/services/delivery-address.service';

@Component({
  selector: 'app-address-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, MapComponent],
  templateUrl: './address-modal.component.html',
  styleUrls: ['./address-modal.component.css']
})
export class AddressModalComponent implements AfterViewInit {
  @Output() addressSelected = new EventEmitter<string>();
  @ViewChild(MapComponent) mapComponent!: MapComponent;

  searchText: string = '';
  selectedAddress: string = '';
  selectedCoordinates?: [number, number];
  isVisible: boolean = false;

  // Детали доставки
  deliveryDetails: DeliveryDetails = {
    entrance: '',
    floor: '',
    apartment: '',
    comment: ''
  };

  constructor(private deliveryAddressService: DeliveryAddressService) {}

  ngAfterViewInit() {
    // Загружаем сохраненный адрес если есть
    const savedAddress = this.deliveryAddressService.getCurrentAddress();
    if (savedAddress) {
      this.selectedAddress = savedAddress.address;
      this.selectedCoordinates = savedAddress.coordinates;
      this.searchText = savedAddress.address;
      if (savedAddress.details) {
        this.deliveryDetails = { ...savedAddress.details };
      }
    }
  }

  show() {
    this.isVisible = true;
    document.body.style.overflow = 'hidden';

    setTimeout(() => {
      if (this.mapComponent) {
        if (this.mapComponent.initMap) {
          this.mapComponent.initMap();
        }
        setTimeout(() => {
          if (this.mapComponent) {
            this.mapComponent.resizeMap();
          }
        }, 500);
      }
    }, 500);
  }

  hide() {
    this.isVisible = false;
    document.body.style.overflow = '';
  }

  search() {
    if (this.searchText && this.mapComponent) {
      this.mapComponent.searchAddress(this.searchText);
    }
  }

  onAddressFound(event: {address: string, coordinates: [number, number]}) {
    this.selectedAddress = event.address;
    this.selectedCoordinates = event.coordinates;
  }

  onMapClick(event: {coordinates: [number, number], address: string}) {
    this.selectedCoordinates = event.coordinates;
    this.selectedAddress = event.address;
    this.searchText = event.address;
  }

  // Форматирование полного адреса с деталями
  private formatDeliveryDetails(): string {
    const details = [];

    if (this.deliveryDetails.entrance) {
      details.push(`подъезд ${this.deliveryDetails.entrance}`);
    }

    if (this.deliveryDetails.floor) {
      details.push(`этаж ${this.deliveryDetails.floor}`);
    }

    if (this.deliveryDetails.apartment) {
      details.push(`кв. ${this.deliveryDetails.apartment}`);
    }

    let result = this.selectedAddress || this.searchText;

    if (details.length > 0) {
      result += `, ${details.join(', ')}`;
    }

    if (this.deliveryDetails.comment) {
      result += `. Комментарий: ${this.deliveryDetails.comment}`;
    }

    return result;
  }

  confirmAddress() {
    if (this.selectedAddress || this.searchText) {
      const formattedAddress = this.formatDeliveryDetails();

      // Создаем полный объект адреса
      const fullAddressData: FullAddress = {
        address: this.selectedAddress || this.searchText,
        coordinates: this.selectedCoordinates,
        details: { ...this.deliveryDetails },
        formattedAddress: formattedAddress
      };

      // Сохраняем в сервисе
      this.deliveryAddressService.setDeliveryAddress(fullAddressData);

      // Эмитим событие для совместимости
      this.addressSelected.emit(formattedAddress);
    }

    this.hide();
  }

  // Очистить все поля
  clearAll() {
    this.searchText = '';
    this.selectedAddress = '';
    this.selectedCoordinates = undefined;
    this.deliveryDetails = {
      entrance: '',
      floor: '',
      apartment: '',
      comment: ''
    };
    this.deliveryAddressService.clearAddress();
  }
}
