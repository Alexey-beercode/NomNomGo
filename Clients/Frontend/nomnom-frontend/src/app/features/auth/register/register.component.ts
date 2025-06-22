// src/app/features/auth/register/register.component.ts

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { switchMap } from 'rxjs/operators';
import { RegisterRequest } from '../../../core/models';
import { AuthService } from '../../../core/services/auth.service';

interface ErrorResponse {
  message: string;
  errors?: { [key: string]: string[] };
  statusCode?: number;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="register-container">
      <button class="back-button" (click)="goBack()">
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M19 12H5" stroke="black" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
          <path d="M12 19L5 12L12 5" stroke="black" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </button>

      <div class="header">
        <div class="logo">
          <img src="assets/images/logo.png" alt="NomNomGo Logo"
               onerror="this.onerror=null; this.src='https://via.placeholder.com/120x30/2ecc71/ffffff?text=NomNomGo';">
          ID
        </div>
      </div>

      <!-- Шаг 1: Контактные данные -->
      <div *ngIf="step === 'contact'">
        <h1 class="title">Создать аккаунт</h1>
        <p class="subtitle">Введите email и номер телефона</p>

        <div class="form-field" [class.error]="fieldErrors['email']">
          <input
            type="email"
            placeholder="Email"
            [(ngModel)]="email"
            autocomplete="email"
            [class.invalid]="fieldErrors['email']">
          <div *ngIf="fieldErrors['email']" class="field-error">{{ fieldErrors['email'] }}</div>
        </div>

        <div class="phone-field" [class.error]="fieldErrors['phoneNumber']">
          <div class="country-code">
            <span>{{ countryCode }}</span>
          </div>
          <input
            type="tel"
            placeholder="445983720"
            [(ngModel)]="phoneNumber"
            autocomplete="tel"
            [class.invalid]="fieldErrors['phoneNumber']">
        </div>
        <div *ngIf="fieldErrors['phoneNumber']" class="field-error">{{ fieldErrors['phoneNumber'] }}</div>
      </div>

      <!-- Шаг 2: Детали пользователя -->
      <div *ngIf="step === 'details'">
        <h1 class="title">Расскажите о себе</h1>
        <p class="subtitle">Придумайте имя пользователя</p>

        <div class="form-field" [class.error]="fieldErrors['username']">
          <input
            type="text"
            placeholder="Имя пользователя"
            [(ngModel)]="username"
            autocomplete="username"
            [class.invalid]="fieldErrors['username']">
          <div *ngIf="fieldErrors['username']" class="field-error">{{ fieldErrors['username'] }}</div>
        </div>
      </div>

      <!-- Шаг 3: Пароль -->
      <div *ngIf="step === 'password'">
        <h1 class="title">Создайте пароль</h1>
        <p class="subtitle">Минимум 6 символов</p>

        <div class="form-field" [class.error]="fieldErrors['password']">
          <input
            type="password"
            placeholder="Пароль"
            [(ngModel)]="password"
            autocomplete="new-password"
            [class.invalid]="fieldErrors['password']">
          <div *ngIf="fieldErrors['password']" class="field-error">{{ fieldErrors['password'] }}</div>
        </div>

        <div class="form-field" [class.error]="fieldErrors['confirmPassword']">
          <input
            type="password"
            placeholder="Подтвердите пароль"
            [(ngModel)]="confirmPassword"
            autocomplete="new-password"
            [class.invalid]="fieldErrors['confirmPassword']">
          <div *ngIf="fieldErrors['confirmPassword']" class="field-error">{{ fieldErrors['confirmPassword'] }}</div>
        </div>
      </div>

      <!-- Общая ошибка -->
      <div *ngIf="generalError" class="error-message">
        <div class="error-icon">⚠️</div>
        <div class="error-text">{{ generalError }}</div>
      </div>

      <!-- Кнопка отправки -->
      <button
        class="submit-button"
        (click)="onSubmit()"
        [disabled]="loading">
        <span *ngIf="loading" class="loading-spinner"></span>
        <span *ngIf="!loading && step === 'password'">Создать аккаунт</span>
        <span *ngIf="!loading && step !== 'password'">Далее</span>
      </button>

      <a class="login-link" (click)="goToLogin()">Уже есть аккаунт?</a>

      <div class="footer">
        <div class="footer-text">NomNomGo ID — ключ от всех сервисов</div>
      </div>
    </div>
  `,
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  email: string = '';
  phoneNumber: string = '';
  username: string = '';
  password: string = '';
  confirmPassword: string = '';

  countryCode: string = '+375';
  loading: boolean = false;
  generalError: string = '';
  fieldErrors: { [key: string]: string } = {};
  step: 'contact' | 'details' | 'password' = 'contact';

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  onSubmit(): void {
    this.clearErrors();

    if (this.step === 'contact') {
      this.validateContact();
    } else if (this.step === 'details') {
      this.validateDetails();
    } else if (this.step === 'password') {
      this.register();
    }
  }

  private clearErrors(): void {
    this.generalError = '';
    this.fieldErrors = {};
  }

  private validateContact(): void {
    let hasErrors = false;

    if (!this.email.trim()) {
      this.fieldErrors['email'] = 'Email обязателен';
      hasErrors = true;
    } else if (!this.isValidEmail(this.email)) {
      this.fieldErrors['email'] = 'Введите корректный email';
      hasErrors = true;
    }

    if (!this.phoneNumber.trim()) {
      this.fieldErrors['phoneNumber'] = 'Номер телефона обязателен';
      hasErrors = true;
    } else if (!this.isValidPhone(this.phoneNumber)) {
      this.fieldErrors['phoneNumber'] = 'Введите 9 цифр белорусского номера (например: 445983720)';
      hasErrors = true;
    }

    if (!hasErrors) {
      this.step = 'details';
    }
  }

  private validateDetails(): void {
    if (!this.username.trim()) {
      this.fieldErrors['username'] = 'Имя пользователя обязательно';
      return;
    }

    if (this.username.length < 3) {
      this.fieldErrors['username'] = 'Минимум 3 символа';
      return;
    }

    if (this.username.length > 20) {
      this.fieldErrors['username'] = 'Максимум 20 символов';
      return;
    }

    if (!/^[a-zA-Z0-9_]+$/.test(this.username)) {
      this.fieldErrors['username'] = 'Только буквы, цифры и подчеркивание';
      return;
    }

    this.step = 'password';
  }

  private handleRegistrationError(error: any): void {
    console.error('Ошибка регистрации:', error);

    // Сброс на первый шаг при ошибках валидации
    if (error.status === 400) {
      this.step = 'contact';
    }

    if (error.error && typeof error.error === 'object') {
      const errorResponse = error.error as ErrorResponse;

      // Обработка ошибок валидации с сервера
      if (errorResponse.errors) {
        // Маппинг ошибок на поля формы
        Object.keys(errorResponse.errors).forEach(field => {
          const fieldName = field.toLowerCase();
          const messages = errorResponse.errors![field];
          if (messages && messages.length > 0) {
            this.fieldErrors[fieldName] = messages[0];
          }
        });
      } else if (errorResponse.message) {
        // Общая ошибка
        this.generalError = errorResponse.message;
      }
    } else {
      // Обработка специфичных ошибок
      switch (error.status) {
        case 409:
          this.generalError = 'Пользователь с таким email уже существует';
          break;
        case 422:
          this.generalError = 'Некорректные данные. Проверьте введенную информацию';
          break;
        case 500:
          this.generalError = 'Ошибка сервера. Попробуйте позже';
          break;
        case 0:
          this.generalError = 'Нет соединения с сервером';
          break;
        default:
          this.generalError = 'Произошла ошибка при регистрации. Попробуйте позже';
      }
    }
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  private isValidPhone(phone: string): boolean {
    // Убираем все пробелы, дефисы, скобки
    const cleanPhone = phone.replace(/[\s\-\(\)]/g, '');
    // Белорусские номера: 9 цифр после +375 (например: +375445983720)
    // Проверяем что остались только цифры и длина ровно 9 символов
    const phoneRegex = /^[\d]{9}$/;
    return phoneRegex.test(cleanPhone);
  }

  private register(): void {
    let hasErrors = false;

    if (!this.password.trim()) {
      this.fieldErrors['password'] = 'Пароль обязателен';
      hasErrors = true;
    } else if (this.password.length < 6) {
      this.fieldErrors['password'] = 'Минимум 6 символов';
      hasErrors = true;
    }

    if (!this.confirmPassword.trim()) {
      this.fieldErrors['confirmPassword'] = 'Подтвердите пароль';
      hasErrors = true;
    } else if (this.password !== this.confirmPassword) {
      this.fieldErrors['confirmPassword'] = 'Пароли не совпадают';
      hasErrors = true;
    }

    if (hasErrors) return;

    this.loading = true;

    // Формируем полный номер телефона с кодом страны +375
    const cleanPhone = this.phoneNumber.replace(/[\s\-\(\)]/g, '');
    const fullPhoneNumber = this.countryCode + cleanPhone;

    console.log('Отправляемые данные:', {
      email: this.email.trim(),
      username: this.username.trim(),
      phoneNumber: fullPhoneNumber
    });

    const registerRequest: RegisterRequest = {
      email: this.email.trim(),
      username: this.username.trim(),
      password: this.password,
      phoneNumber: fullPhoneNumber // Должно быть: +375445983720
    };

    this.authService.register(registerRequest).subscribe({
      next: (response) => {
        console.log('Успешная регистрация:', response);

        // После успешной регистрации получаем информацию о пользователе с ролями
        this.authService.getCurrentUserInfo().subscribe({
          next: (userInfo) => {
            console.log('Информация о пользователе:', userInfo);

            // Переход в зависимости от роли пользователя
            if (userInfo.roles && userInfo.roles.includes('Admin')) {
              this.router.navigate(['/admin']);
            } else {
              this.router.navigate(['/dashboard']);
            }
          },
          error: (error) => {
            console.error('Ошибка получения информации о пользователе:', error);
            // В случае ошибки просто перенаправляем на dashboard
            this.router.navigate(['/dashboard']);
          },
          complete: () => {
            this.loading = false;
          }
        });
      },
      error: (error) => {
        this.loading = false;
        this.handleRegistrationError(error);
      }
    });
  }

  goBack(): void {
    if (this.step === 'contact') {
      this.router.navigate(['/login']);
    } else if (this.step === 'details') {
      this.step = 'contact';
    } else {
      this.step = 'details';
    }
    this.clearErrors();
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
