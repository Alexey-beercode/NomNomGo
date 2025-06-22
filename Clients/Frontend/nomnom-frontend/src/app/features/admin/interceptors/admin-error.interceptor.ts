import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const adminErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Если 403 ошибка в админке - перенаправляем на главную
      if (error.status === 403 && req.url.includes('/admin')) {
        router.navigate(['/']);
        return throwError(() => new Error('Доступ запрещен'));
      }

      // Если 401 ошибка - перенаправляем на логин
      if (error.status === 401) {
        router.navigate(['/login']);
        return throwError(() => new Error('Требуется авторизация'));
      }

      return throwError(() => error);
    })
  );
};
