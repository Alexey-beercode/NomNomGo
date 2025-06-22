import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LoginHeaderComponent } from '../login-header/login-header.component';
import { LoginTabsComponent } from '../login-tabs/login-tabs.component';
import { LoginFormComponent } from '../login-form/login-form.component';
import { BiometricButtonComponent } from '../biometric-button/biometric-button.component';
import { CreateAccountComponent } from '../create-account/create-account.component';
import { SocialLoginComponent } from '../social-login/social-login.component';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    LoginHeaderComponent,
    LoginTabsComponent,
    LoginFormComponent,
    CreateAccountComponent
  ],
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.css']
})
export class LoginPageComponent {
  activeTab: 'email' | 'phone' = 'email';

  onTabChange(tab: 'email' | 'phone'): void {
    this.activeTab = tab;
  }

  goBack(): void {
    window.history.back();
  }
}
