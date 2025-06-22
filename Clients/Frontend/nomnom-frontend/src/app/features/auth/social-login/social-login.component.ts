import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-social-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './social-login.component.html',
  styleUrls: ['./social-login.component.css']
})
export class SocialLoginComponent {
  loginWithSocial(provider: string): void {
    console.log('Social login requested with provider:', provider);
    // Здесь будет логика входа через соцсети
  }

  loginWithQR(): void {
    console.log('QR login requested');
    // Здесь будет логика входа через QR
  }
}
