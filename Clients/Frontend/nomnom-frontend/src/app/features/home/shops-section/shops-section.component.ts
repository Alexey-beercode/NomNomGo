import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Shop } from '../../../core/models/shop';

@Component({
  selector: 'app-shops-section',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './shops-section.component.html',
  styleUrls: ['./shops-section.component.css']
})
export class ShopsSectionComponent {
  @Input() shops: Shop[] = [];
}
