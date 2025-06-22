import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {LoginRequest } from '../../../core/models';
import {AuthService} from '../../../core/services/auth.service';

@Component({
  selector: 'app-password-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './password-page.component.html',
  styleUrls: ['./password-page.component.css']
})
export class PasswordPageComponent implements OnInit {
  email: string = '';
  password: string = '';
  showPassword: boolean = false;
  loading: boolean = false;
  error: string = '';

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Получаем email из предыдущего шага
    this.email = sessionStorage.getItem('login') || '';
    if (!this.email) {
      this.router.navigate(['/login']);
    }
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (!this.password.trim()) {
      this.error = 'Введите пароль';
      return;
    }

    this.loading = true;
    this.error = '';

    const loginRequest: LoginRequest = {
      login: this.email,
      password: this.password
    };

    this.authService.login(loginRequest).subscribe({
      next: (response) => {
        console.log('Успешный вход:', response);
        sessionStorage.removeItem('login');
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        this.loading = false;
        if (error.status === 401) {
          this.error = 'Неверный пароль';
        } else if (error.status === 400) {
          this.error = 'Пользователь не найден';
        } else {
          this.error = 'Ошибка входа. Попробуйте позже';
        }
        console.error('Ошибка входа:', error);
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/login']);
  }

  forgotPassword(): void {
    console.log('Восстановление пароля для:', this.email);
    // TODO: Реализовать восстановление пароля
  }
}
