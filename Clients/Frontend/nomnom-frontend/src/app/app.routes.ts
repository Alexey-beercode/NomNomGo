// Файл: src/app/app.routes.ts
import { Routes } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './core/services/auth.service';
import { Router } from '@angular/router';
import {adminGuard} from './features/admin/guards/admin.guard';

// В файле app.routes.ts - упростим authGuard
export const authGuard = () => {
  // Простая проверка токена без использования сервиса
  const token = localStorage.getItem('access_token');
  if (token) {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const isValid = payload.exp * 1000 > Date.now();

      if (isValid) {
        return true;
      }
    } catch {
      // Токен поврежден
    }
  }

  // Если токен отсутствует или невалиден
  const router = inject(Router);
  router.navigate(['/login']);
  return false;
};

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login-page/login-page.component').then(m => m.LoginPageComponent)
  },
  {
    path: 'password',
    loadComponent: () => import('./features/auth/password-page/password-page.component').then(m => m.PasswordPageComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'restaurant/:id',
    loadComponent: () => import('./features/restaurants/restaurant-page/restaurant-page.component').then(m => m.RestaurantPageComponent)
  },
  // Новые маршруты для личного кабинета
  {
    path: 'profile',
    loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent),
    canActivate: [authGuard]
  },
  {
    path: 'orders/:id',
    loadComponent: () => import('./features/orders/order-detail/order-detail.component').then(m => m.OrderDetailComponent),
    canActivate: [authGuard]
  },
  {
    path: 'tracking/:id',
    loadComponent: () => import('./features/order-tracking/order-tracking.component').then(m => m.OrderTrackingComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin',
    loadChildren: () => import('./features/admin/admin.routes')
      .then(m => m.adminRoutes),
    canActivate: [adminGuard],
    title: 'Админ-панель'
  },
  {
    path: '**',
    redirectTo: ''
  }
];
