import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Banner } from '../../../core/models/banner';

@Component({
  selector: 'app-banners',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './banners.component.html',
  styleUrls: ['./banners.component.css']
})
export class BannersComponent {
  @Input() banners: Banner[] = [];
}
