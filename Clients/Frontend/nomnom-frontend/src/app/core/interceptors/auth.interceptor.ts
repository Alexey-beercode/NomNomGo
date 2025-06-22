// src/app/core/interceptors/auth.interceptor.ts

import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn, HttpEvent } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError, switchMap, filter, take, Observable } from 'rxjs';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;
let refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Получаем токен
  const token = authService.getToken();

  // Добавляем токен к запросу если он есть
  let authReq = req;
  if (token && authService.isAuthenticated()) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {

      // Логируем ошибки для отладки
      console.error('HTTP Error:', {
        status: error.status,
        url: error.url,
        message: error.message
      });

      // Обрабатываем 401 ошибку
      if (error.status === 401) {
        return handle401Error(authReq, next, authService, router);
      }

      // Обрабатываем 403 ошибку в админке
      if (error.status === 403 && req.url.includes('/admin')) {
        router.navigate(['/']);
        return throwError(() => new Error('Доступ запрещен'));
      }

      return throwError(() => error);
    })
  );
};

function handle401Error(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
  router: Router
): Observable<HttpEvent<unknown>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    const refreshToken = localStorage.getItem('refresh_token');

    if (refreshToken) {
      return authService.refreshToken().pipe(
        switchMap((tokenResponse: any) => {
          isRefreshing = false;
          refreshTokenSubject.next(tokenResponse.accessToken);

          // Повторяем запрос с новым токеном
          const newAuthReq = request.clone({
            setHeaders: {
              Authorization: `Bearer ${tokenResponse.accessToken}`
            }
          });

          return next(newAuthReq);
        }),
        catchError((error) => {
          isRefreshing = false;
          refreshTokenSubject.next(null);

          // Очищаем токены и перенаправляем на логин
          authService.logout().subscribe();
          router.navigate(['/login']);

          return throwError(() => error);
        })
      );
    } else {
      isRefreshing = false;
      authService.logout().subscribe();
      router.navigate(['/login']);
      return throwError(() => new Error('No refresh token'));
    }
  } else {
    // Ждем завершения обновления токена
    return refreshTokenSubject.pipe(
      filter(token => token != null),
      take(1),
      switchMap(jwt => {
        const newAuthReq = request.clone({
          setHeaders: {
            Authorization: `Bearer ${jwt}`
          }
        });
        return next(newAuthReq);
      })
    );
  }
}
