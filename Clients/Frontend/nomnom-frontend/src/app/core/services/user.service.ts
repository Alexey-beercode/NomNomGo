// src/app/core/services/user.service.ts

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  UserDetailResponse,
  UserListItem,
  UpdateProfileRequest,
  ChangePasswordRequest
} from '../models/user.models';

interface GetUsersParams {
  search?: string;
  role?: string;
  isBlocked?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl = `${environment.apiUrl}/api/Users`;

  constructor(private http: HttpClient) {
  }

  // Обработчик ошибок
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Произошла ошибка';

    console.error('UserService Error Details:', {
      error: error,
      status: error.status,
      statusText: error.statusText,
      url: error.url,
      errorBody: error.error
    });

    if (error.error instanceof ErrorEvent) {
      errorMessage = `Ошибка: ${error.error.message}`;
    } else {
      switch (error.status) {
        case 0:
          errorMessage = 'Нет подключения к серверу';
          break;
        case 400:
          errorMessage = 'Неверные данные запроса';
          break;
        case 401:
          errorMessage = 'Необходима авторизация';
          break;
        case 403:
          errorMessage = 'Недостаточно прав доступа';
          break;
        case 404:
          errorMessage = 'Пользователь не найден';
          break;
        case 500:
          errorMessage = 'Внутренняя ошибка сервера';
          break;
        default:
          errorMessage = `Ошибка сервера: ${error.status}`;
      }

      if (error.error?.error) {
        errorMessage += `: ${error.error.error}`;
      } else if (error.error?.message) {
        errorMessage += `: ${error.error.message}`;
      }
    }

    return throwError(() => new Error(errorMessage));
  }

  // Получение списка пользователей (для админки)
  getUsers(params?: GetUsersParams): Observable<UserDetailResponse[]> {
    let httpParams = new HttpParams();

    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          httpParams = httpParams.set(key, value.toString());
        }
      });
    }

    console.log('Making request to:', `${this.apiUrl}`, 'with params:', httpParams.toString());

    return this.http.get<any>(`${this.apiUrl}`, {params: httpParams})
      .pipe(
        map(response => {
          console.log('Raw API response:', response);

          // Проверяем различные возможные структуры ответа
          let users: any[] = [];

          if (Array.isArray(response)) {
            // Если ответ - это просто массив пользователей
            users = response;
          } else if (response && response.items && Array.isArray(response.items)) {
            // Если ответ пагинированный с полем items
            users = response.items;
          } else if (response && response.data && Array.isArray(response.data)) {
            // Если ответ обернут в объект с полем data
            users = response.data;
          } else if (response && response.users && Array.isArray(response.users)) {
            // Если пользователи в поле users
            users = response.users;
          } else if (response && response.result && Array.isArray(response.result)) {
            // Если пользователи в поле result
            users = response.result;
          } else {
            console.error('Unexpected response structure:', response);
            throw new Error('Неожиданная структура ответа от сервера');
          }

          if (!Array.isArray(users)) {
            console.error('Users is not an array:', users);
            throw new Error('Сервер вернул некорректные данные');
          }

          // Преобразуем UserListItem в UserDetailResponse для совместимости
          return users.map(user => ({
            userId: user.userId || user.UserId || '',
            username: user.username || user.Username || '',
            email: user.email || user.Email || '',
            phoneNumber: user.phoneNumber || user.PhoneNumber || undefined,
            isBlocked: user.isBlocked !== undefined ? user.isBlocked : user.IsBlocked || false,
            blockedUntil: user.blockedUntil || user.BlockedUntil || undefined,
            roles: user.roles || user.Roles || [],
            createdAt: user.createdAt || user.CreatedAt || new Date().toISOString(),
            updatedAt: user.updatedAt || user.UpdatedAt || user.createdAt || user.CreatedAt || new Date().toISOString()
          } as UserDetailResponse));
        }),
        catchError(this.handleError)
      );
  }

  // Получение конкретного пользователя
  getUser(userId: string): Observable<UserDetailResponse> {
    return this.http.get<any>(`${this.apiUrl}/${userId}`)
      .pipe(
        map(response => {
          console.log('Get user response:', response);

          // Проверяем структуру ответа
          let user = response;
          if (response && response.data) {
            user = response.data;
          } else if (response && response.user) {
            user = response.user;
          }

          return this.mapToUserDetailResponse(user);
        }),
        catchError(this.handleError)
      );
  }

  // Блокировка пользователя (без reason)
  blockUser(userId: string, reason?: string, duration?: string): Observable<UserDetailResponse> {
    const body = {duration};
    return this.http.put<any>(`${this.apiUrl}/${userId}/block`, body)
      .pipe(
        map(response => {
          let user = response;
          if (response && response.data) {
            user = response.data;
          }
          return this.mapToUserDetailResponse(user);
        }),
        catchError(this.handleError)
      );
  }

  // Разблокировка пользователя
  unblockUser(userId: string): Observable<UserDetailResponse> {
    return this.http.put<any>(`${this.apiUrl}/${userId}/unblock`, {})
      .pipe(
        map(response => {
          let user = response;
          if (response && response.data) {
            user = response.data;
          }
          return this.mapToUserDetailResponse(user);
        }),
        catchError(this.handleError)
      );
  }

  // Вспомогательный метод для маппинга
  private mapToUserDetailResponse(user: any): UserDetailResponse {
    return {
      userId: user.userId || user.UserId || '',
      username: user.username || user.Username || '',
      email: user.email || user.Email || '',
      phoneNumber: user.phoneNumber || user.PhoneNumber || undefined,
      isBlocked: user.isBlocked !== undefined ? user.isBlocked : user.IsBlocked || false,
      blockedUntil: user.blockedUntil || user.BlockedUntil || undefined,
      roles: user.roles || user.Roles || [],
      createdAt: user.createdAt || user.CreatedAt || new Date().toISOString(),
      updatedAt: user.updatedAt || user.UpdatedAt || user.createdAt || user.CreatedAt || new Date().toISOString()
    };
  }

  // Обновление профиля
  updateProfile(userId: string, profileData: UpdateProfileRequest): Observable<UserDetailResponse> {
    return this.http.put<any>(`${this.apiUrl}/profile`, profileData)
      .pipe(
        map(response => {
          let user = response;
          if (response && response.data) {
            user = response.data;
          }
          return this.mapToUserDetailResponse(user);
        }),
        catchError(this.handleError)
      );
  }

  // Смена пароля
  changePassword(passwordData: ChangePasswordRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/password`, passwordData)
      .pipe(catchError(this.handleError));
  }

  toggleUserBlock(userId: string, isBlocked: boolean, reason?: string, duration?: string): Observable<UserDetailResponse> {
    if (isBlocked) {
      return this.blockUser(userId, reason, duration);
    } else {
      return this.unblockUser(userId);
    }
  }

  // Утилиты
  getAllUsers(): Observable<UserDetailResponse[]> {
    return this.getUsers();
  }

  searchUsers(searchTerm: string): Observable<UserDetailResponse[]> {
    return this.getUsers({search: searchTerm});
  }

  getUsersByRole(role: string): Observable<UserDetailResponse[]> {
    return this.getUsers({role});
  }

  getBlockedUsers(): Observable<UserDetailResponse[]> {
    return this.getUsers({isBlocked: true});
  }
}
