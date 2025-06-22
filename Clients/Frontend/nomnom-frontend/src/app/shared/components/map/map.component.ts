// map.component.ts
import { Component, Output, EventEmitter, ElementRef, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import * as L from 'leaflet';

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule],
  template: '<div id="map" style="height: 400px; width: 100%;"></div>',
  styles: [`
    :host {
      display: block;
      width: 100%;
      height: 100%;
    }
    #map {
      width: 100%;
      height: 400px;
    }
  `]
})
export class MapComponent implements AfterViewInit, OnDestroy {
  @Output() addressFound = new EventEmitter<{address: string, coordinates: [number, number]}>();
  @Output() mapClicked = new EventEmitter<{coordinates: [number, number], address: string}>();

  private map: L.Map | null = null;
  private marker: L.Marker | null = null;
  private resizeObserver: ResizeObserver | null = null;

  constructor(private el: ElementRef) {}

  ngAfterViewInit() {
    // Отложим инициализацию карты, чтобы DOM успел полностью загрузиться
    setTimeout(() => {
      this.initMap();
    }, 300);
  }

  ngOnDestroy() {
    if (this.resizeObserver) {
      this.resizeObserver.disconnect();
    }

    if (this.map) {
      this.map.remove();
      this.map = null;
    }
  }

  initMap(): void {
    if (this.map) {
      this.map.remove();
    }

    const mapElement = this.el.nativeElement.querySelector('#map');
    if (!mapElement) {
      console.error('Map container not found');
      return;
    }

    // Центр Минска или другого города Беларуси
    this.map = L.map('map', {
      maxZoom: 19,
      minZoom: 3
    }).setView([53.9045, 27.5615], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(this.map);

    // Add click event to map
    this.map.on('click', (e: L.LeafletMouseEvent) => {
      this.handleMapClick(e.latlng.lat, e.latlng.lng);
    });

    // Следим за изменениями размера
    this.watchForResize(mapElement);

    // Принудительно обновим размер карты после инициализации
    setTimeout(() => this.resizeMap(), 500);
  }

  private watchForResize(element: HTMLElement) {
    if (typeof ResizeObserver !== 'undefined') {
      this.resizeObserver = new ResizeObserver(() => {
        if (this.map) {
          this.map.invalidateSize();
        }
      });
      this.resizeObserver.observe(element);
    }
  }

  resizeMap() {
    if (this.map) {
      this.map.invalidateSize(true);
    }
  }

  searchAddress(address: string): void {
    if (!this.map) return;

    // Использование API для геокодирования с указанием User-Agent
    fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(address)}, Беларусь&limit=1`, {
      headers: {
        'User-Agent': 'YourAppName/1.0' // Nominatim требует указания User-Agent
      }
    })
      .then(response => response.json())
      .then(data => {
        if (data && data.length > 0) {
          const lat = parseFloat(data[0].lat);
          const lon = parseFloat(data[0].lon);
          const displayName = data[0].display_name;

          this.setMarker(lat, lon);
          this.addressFound.emit({
            address: displayName,
            coordinates: [lat, lon]
          });
        }
      })
      .catch(error => console.error('Error geocoding address:', error));
  }

  private handleMapClick(lat: number, lng: number): void {
    this.setMarker(lat, lng);

    // Reverse geocoding to get address from coordinates
    fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`, {
      headers: {
        'User-Agent': 'YourAppName/1.0' // Nominatim требует указания User-Agent
      }
    })
      .then(response => response.json())
      .then(data => {
        if (data && data.display_name) {
          this.mapClicked.emit({
            coordinates: [lat, lng],
            address: data.display_name
          });
        }
      })
      .catch(error => console.error('Error reverse geocoding:', error));
  }

  private setMarker(lat: number, lng: number): void {
    if (!this.map) return;

    this.map.setView([lat, lng], 16);

    if (this.marker) {
      this.map.removeLayer(this.marker);
    }

    this.marker = L.marker([lat, lng]).addTo(this.map);
  }
}
