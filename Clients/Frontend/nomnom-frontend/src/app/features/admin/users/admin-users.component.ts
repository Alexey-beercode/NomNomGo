// src/app/features/admin/users/admin-users.component.ts

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, takeUntil, finalize } from 'rxjs';
import { UserService } from '../../../core/services/user.service';
import { UserDetailResponse } from '../../../core/models/user.models';
import { InputComponent } from '../../../shared/components/input/input.component';

interface UserFilters {
  search: string;
  role?: string;
  isBlocked?: boolean;
  sortBy: 'username' | 'email' | 'createdAt';
  sortOrder: 'asc' | 'desc';
}

interface ModalState {
  isOpen: boolean;
  title: string;
  type: 'view' | 'block' | 'unblock';
  data: UserDetailResponse | null;
}

interface BlockForm {
  duration: string;
}

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, InputComponent],
  templateUrl: './admin-users.component.html',
  styleUrls: ['./admin-users.component.css']
})
export class AdminUsersComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  users: UserDetailResponse[] = [];
  filteredUsers: UserDetailResponse[] = [];
  loading = false;
  error = '';

  availableDurations = [
    { value: '1h', label: '1 час' },
    { value: '1d', label: '1 день' },
    { value: '7d', label: '7 дней' },
    { value: '30d', label: '30 дней' },
    { value: 'permanent', label: 'Навсегда' }
  ];

  // Modal state
  modalState: ModalState = {
    isOpen: false,
    title: '',
    type: 'view',
    data: null
  };

  // Block form
  blockForm: BlockForm = {
    duration: '1d'
  };

  // Filters
  filters: UserFilters = {
    search: '',
    role: undefined,
    isBlocked: undefined,
    sortBy: 'createdAt',
    sortOrder: 'desc'
  };

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadUsers(): void {
    this.loading = true;
    this.error = '';

    this.userService.getAllUsers()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.loading = false)
      )
      .subscribe({
        next: (users) => {
          console.log('Loaded users:', users); // Для отладки
          this.users = users;
          this.applyFilters();
        },
        error: (error) => {
          console.error('Error loading users:', error);
          this.error = 'Ошибка при загрузке пользователей: ' + (error.message || 'Неизвестная ошибка');
          this.users = [];
          this.filteredUsers = [];
        }
      });
  }

  applyFilters(): void {
    let filtered = [...this.users];

    // Поиск
    if (this.filters.search?.trim()) {
      const searchLower = this.filters.search.toLowerCase();
      filtered = filtered.filter(user =>
        user.username.toLowerCase().includes(searchLower) ||
        user.email.toLowerCase().includes(searchLower) ||
        (user.phoneNumber && user.phoneNumber.toLowerCase().includes(searchLower))
      );
    }

    // Фильтр по роли
    if (this.filters.role) {
      filtered = filtered.filter(user => user.roles.includes(this.filters.role!));
    }

    // Фильтр по статусу блокировки
    if (this.filters.isBlocked !== undefined) {
      filtered = filtered.filter(user => user.isBlocked === this.filters.isBlocked);
    }

    // Сортировка
    filtered.sort((a, b) => {
      let aValue: any = a[this.filters.sortBy];
      let bValue: any = b[this.filters.sortBy];

      if (this.filters.sortBy === 'createdAt') {
        aValue = new Date(aValue).getTime();
        bValue = new Date(bValue).getTime();
      }

      if (aValue < bValue) return this.filters.sortOrder === 'asc' ? -1 : 1;
      if (aValue > bValue) return this.filters.sortOrder === 'asc' ? 1 : -1;
      return 0;
    });

    this.filteredUsers = filtered;
  }

  onSearchChange(value: string): void {
    this.filters.search = value;
    this.applyFilters();
  }

  onFilterChange(): void {
    this.applyFilters();
  }

  onSortChange(sortBy: string): void {
    if (this.filters.sortBy === sortBy) {
      this.filters.sortOrder = this.filters.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.filters.sortBy = sortBy as any;
      this.filters.sortOrder = 'asc';
    }
    this.applyFilters();
  }

  clearFilters(): void {
    this.filters = {
      search: '',
      role: undefined,
      isBlocked: undefined,
      sortBy: 'createdAt',
      sortOrder: 'desc'
    };
    this.applyFilters();
  }

  refreshData(): void {
    this.loadUsers();
  }

  // Modal methods
  openViewModal(user: UserDetailResponse): void {
    this.modalState = {
      isOpen: true,
      title: `Пользователь: ${user.username}`,
      type: 'view',
      data: user
    };
  }

  openBlockModal(user: UserDetailResponse): void {
    if (user.isBlocked) {
      this.modalState = {
        isOpen: true,
        title: 'Разблокировать пользователя',
        type: 'unblock',
        data: user
      };
    } else {
      this.modalState = {
        isOpen: true,
        title: 'Заблокировать пользователя',
        type: 'block',
        data: user
      };
      this.blockForm = {
        duration: '1d'
      };
    }
  }

  closeModal(): void {
    this.modalState.isOpen = false;
    this.blockForm = {
      duration: '1d'
    };
  }

  // User actions
  blockUser(): void {
    if (this.modalState.data) {
      const userId = this.modalState.data.userId;

      this.userService.blockUser(userId, undefined, this.blockForm.duration)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (updatedUser) => {
            const index = this.users.findIndex(u => u.userId === userId);
            if (index !== -1) {
              this.users[index] = updatedUser;
              this.applyFilters();
            }
            this.closeModal();
          },
          error: (error) => {
            console.error('Error blocking user:', error);
            this.error = 'Ошибка при блокировке пользователя: ' + (error.message || 'Неизвестная ошибка');
          }
        });
    }
  }

  unblockUser(): void {
    if (this.modalState.data) {
      const userId = this.modalState.data.userId;

      this.userService.unblockUser(userId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (updatedUser) => {
            const index = this.users.findIndex(u => u.userId === userId);
            if (index !== -1) {
              this.users[index] = updatedUser;
              this.applyFilters();
            }
            this.closeModal();
          },
          error: (error) => {
            console.error('Error unblocking user:', error);
            this.error = 'Ошибка при разблокировке пользователя: ' + (error.message || 'Неизвестная ошибка');
          }
        });
    }
  }

  // Utility methods
  getUserInitials(username: string): string {
    return username.substring(0, 2).toUpperCase();
  }

  getRoleClass(role: string): string {
    switch (role.toUpperCase()) {
      case 'ADMIN': return 'role-admin';
      case 'MANAGER': return 'role-manager';
      case 'USER': return 'role-user';
      case 'COURIER': return 'role-courier';
      default: return 'role-default';
    }
  }

  getRoleDisplayName(role: string): string {
    switch (role.toUpperCase()) {
      case 'ADMIN': return 'Администратор';
      case 'MANAGER': return 'Менеджер';
      case 'USER': return 'Пользователь';
      case 'COURIER': return 'Курьер';
      default: return role;
    }
  }

  getStatusClass(isBlocked: boolean): string {
    return isBlocked ? 'status-blocked' : 'status-active';
  }

  getStatusText(isBlocked: boolean): string {
    return isBlocked ? 'Заблокирован' : 'Активен';
  }

  getDurationDisplayName(duration: string): string {
    const found = this.availableDurations.find(d => d.value === duration);
    return found ? found.label : duration;
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('ru-RU');
  }

  formatDateTime(dateString: string): string {
    return new Date(dateString).toLocaleString('ru-RU');
  }

  getSortIcon(sortBy: string): string {
    if (this.filters.sortBy !== sortBy) return '↕️';
    return this.filters.sortOrder === 'asc' ? '↑' : '↓';
  }

  // Stats methods
  getTotalUsers(): number {
    return this.users.length;
  }

  getActiveUsers(): number {
    return this.users.filter(u => !u.isBlocked).length;
  }

  getBlockedUsers(): number {
    return this.users.filter(u => u.isBlocked).length;
  }

  getAdminUsers(): number {
    return this.users.filter(u => u.roles.some(role => role.toUpperCase() === 'ADMIN')).length;
  }

  trackUser(index: number, user: UserDetailResponse): string {
    return user.userId;
  }
}
