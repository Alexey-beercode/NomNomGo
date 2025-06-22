import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login-form.component.html',
  styleUrls: ['./login-form.component.css']
})
export class LoginFormComponent {
  @Input() type: 'email' | 'phone' = 'email';

  email: string = '';
  phone: string = '';
  loading: boolean = false;
  error: string = '';

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  onSubmit(): void {
    const login = this.type === 'email' ? this.email : this.phone;

    if (!login.trim()) {
      this.error = 'Введите email или телефон';
      return;
    }

    // Сохраняем логин для следующего шага
    sessionStorage.setItem('login', login);
    this.router.navigate(['/password']);
  }
}
