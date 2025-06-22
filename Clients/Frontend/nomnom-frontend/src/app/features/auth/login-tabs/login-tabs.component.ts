import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login-tabs',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login-tabs.component.html',
  styleUrls: ['./login-tabs.component.css']
})
export class LoginTabsComponent {
  @Input() activeTab: 'email' | 'phone' = 'email';
  @Output() tabChange = new EventEmitter<'email' | 'phone'>();

  setActiveTab(tab: 'email' | 'phone'): void {
    if (this.activeTab !== tab) {
      this.activeTab = tab;
      this.tabChange.emit(tab);
    }
  }
}
