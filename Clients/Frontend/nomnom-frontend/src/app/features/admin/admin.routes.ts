// src/app/features/admin/admin.routes.ts

import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './layout/admin-layout.component';

export const adminRoutes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    children: [
      {
        path: '',
        redirectTo: 'restaurants',
        pathMatch: 'full'
      },
      {
        path: 'restaurants',
        loadComponent: () => import('./restaurants/admin-restaurants.component')
          .then(m => m.AdminRestaurantsComponent),
        title: 'Управление ресторанами'
      },
      {
        path: 'categories',
        loadComponent: () => import('./categories/admin-categories.component')
          .then(m => m.AdminCategoriesComponent),
        title: 'Управление категориями'
      },
      {
        path: 'menu-items',
        loadComponent: () => import('./menu-items/admin-menu-items.component')
          .then(m => m.AdminMenuItemsComponent),
        title: 'Управление блюдами'
      },
      {
        path: 'orders',
        loadComponent: () => import('./orders/admin-orders.component')
          .then(m => m.AdminOrdersComponent),
        title: 'Управление заказами'
      },
      {
        path: 'users',
        loadComponent: () => import('./users/admin-users.component')
          .then(m => m.AdminUsersComponent),
        title: 'Управление пользователями'
      },
      {
        path: 'reviews',
        loadComponent: () => import('./reviews/admin-reviews.component')
          .then(m => m.AdminReviewsComponent),
        title: 'Управление отзывами'
      }
    ]
  }
];
