// src/app/core/services/auth.service.ts

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, tap, switchMap, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  RegisterRequest,
  LoginRequest,
  RegisterResponse,
  LoginResponse,
  CurrentUser,
  CurrentUserResponse,
  RefreshTokenResponse
} from '../models/auth.models';
import { UpdateProfileRequest, ChangePasswordRequest } from '../models/user.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/api/Authentication`;
  private currentUserSubject: BehaviorSubject<CurrentUser | null>;
  public currentUser$: Observable<CurrentUser | null>;

  constructor(private http: HttpClient) {
    const storedUser = localStorage.getItem('current_user');
    this.currentUserSubject = new BehaviorSubject<CurrentUser | null>(
      storedUser ? JSON.parse(storedUser) : null
    );
    this.currentUser$ = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): CurrentUser | null {
    return this.currentUserSubject.value;
  }

  getCurrentUserValue(): CurrentUser | null {
    return this.currentUserValue;
  }

  getCurrentUser(): Observable<CurrentUser | null> {
    // Если пользователь есть в памяти, возвращаем его
    if (this.currentUserValue) {
      return this.currentUser$;
    }

    // Если есть токен, получаем текущего пользователя с сервера
    const token = localStorage.getItem('access_token');
    if (token && this.isAuthenticated()) {
      return this.getCurrentUserInfo().pipe(
        map(() => this.currentUserValue),
        catchError((error) => {
          console.error('Error getting current user:', error);
          // Если не удалось получить пользователя, очищаем токены
          this.logout().subscribe();
          return of(null);
        })
      );
    }

    return this.currentUser$;
  }

  register(request: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.apiUrl}/register`, request)
      .pipe(
        tap(response => {
          // Сохраняем токены
          localStorage.setItem('access_token', response.accessToken);
          localStorage.setItem('refresh_token', response.refreshToken);

          // Создаем объект пользователя
          const user: CurrentUser = {
            userId: response.userId,
            username: response.username,
            email: request.email,
            phoneNumber: request.phoneNumber,
            roles: [], // Роли придут из текущего пользователя
            createdAt: new Date().toISOString()
          };

          localStorage.setItem('current_user', JSON.stringify(user));
          this.currentUserSubject.next(user);
        })
      );
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, request)
      .pipe(
        tap(response => {
          // Сохраняем токены
          localStorage.setItem('access_token', response.accessToken);
          localStorage.setItem('refresh_token', response.refreshToken);
        }),
        // После логина получаем полную информацию о пользователе
        switchMap((loginResponse) =>
          this.getCurrentUserInfo().pipe(
            map(() => loginResponse), // Возвращаем оригинальный ответ логина
            catchError((error) => {
              console.error('Error getting user info after login:', error);
              // Если не удалось получить информацию о пользователе, создаем базовую
              const user: CurrentUser = {
                userId: this.generateGuid(),
                username: loginResponse.username || 'Unknown',
                email: request.login,
                roles: loginResponse.roles || [],
                createdAt: new Date().toISOString()
              };
              localStorage.setItem('current_user', JSON.stringify(user));
              this.currentUserSubject.next(user);
              return of(loginResponse);
            })
          )
        )
      );
  }

  logout(): Observable<any> {
    return new Observable(observer => {
      localStorage.removeItem('access_token');
      localStorage.removeItem('refresh_token');
      localStorage.removeItem('current_user');
      this.currentUserSubject.next(null);
      observer.next(true);
      observer.complete();
    });
  }

  refreshToken(): Observable<RefreshTokenResponse> {
    const refreshToken = localStorage.getItem('refresh_token');
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    return this.http.post<RefreshTokenResponse>(`${this.apiUrl}/refresh`, { refreshToken })
      .pipe(
        tap(response => {
          localStorage.setItem('access_token', response.accessToken);
          localStorage.setItem('refresh_token', response.refreshToken);
        })
      );
  }

  getCurrentUserInfo(): Observable<CurrentUserResponse> {
    // Попробуем разные варианты endpoint'а
    return this.http.get<CurrentUserResponse>(`${this.apiUrl}/current-user`)
      .pipe(
        catchError((error) => {
          console.error('Error with /current-user, trying /me:', error);
          // Если /current-user не работает, пробуем /me
          return this.http.get<CurrentUserResponse>(`${this.apiUrl}/me`);
        }),
        tap(response => {
          const user: CurrentUser = {
            userId: response.userId,
            username: response.username,
            email: response.email,
            phoneNumber: response.phoneNumber,
            roles: response.roles,
            createdAt: response.createdAt
          };
          localStorage.setItem('current_user', JSON.stringify(user));
          this.currentUserSubject.next(user);
        }),
        catchError((error) => {
          console.error('Both endpoints failed:', error);
          throw error;
        })
      );
  }

  updateProfile(profileData: UpdateProfileRequest): Observable<any> {
    // Пробуем разные варианты endpoint'а для обновления профиля
    return this.http.put(`${this.apiUrl}/update-profile`, profileData)
      .pipe(
        catchError((error) => {
          console.error('Error with /update-profile, trying /profile:', error);
          // Если /update-profile не работает, пробуем /profile
          return this.http.put(`${this.apiUrl}/profile`, profileData);
        }),
        // После успешного обновления получаем свежие данные пользователя
        switchMap(() => this.getCurrentUserInfo()),
        tap(() => {
          console.log('Profile updated successfully');
        }),
        catchError((error) => {
          console.error('Profile update failed:', error);
          throw error;
        })
      );
  }

  changePassword(currentPassword: string, newPassword: string): Observable<any> {
    const request: ChangePasswordRequest = {
      currentPassword,
      newPassword
    };
    return this.http.post(`${this.apiUrl}/change-password`, request);
  }

  isAuthenticated(): boolean {
    const token = localStorage.getItem('access_token');
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }

  getToken(): string | null {
    return localStorage.getItem('access_token');
  }

  private generateGuid(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
      const r = Math.random() * 16 | 0;
      const v = c === 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }
}
