import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-biometric-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './biometric-button.component.html',
  styleUrls: ['./biometric-button.component.css']
})
export class BiometricButtonComponent {
  onBiometricLogin(): void {
    console.log('Biometric login requested');
    // Здесь будет логика для биометрии
  }
}
