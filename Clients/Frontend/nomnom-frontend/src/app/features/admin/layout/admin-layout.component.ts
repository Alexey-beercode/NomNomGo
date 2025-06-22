// src/app/features/admin/layout/admin-layout.component.ts

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { Subject, filter, takeUntil } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { CurrentUser } from '../../../core/models/auth.models';

interface MenuItem {
  icon: string;
  label: string;
  route: string;
  roles?: string[];
}

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.css']
})
export class AdminLayoutComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  currentUser: CurrentUser | null = null;
  sidebarCollapsed = false;
  currentRoute = '';

  menuItems: MenuItem[] = [
    {
      icon: '📊',
      label: 'Дашборд',
      route: '/admin/dashboard',
      roles: ['ADMIN']
    },
    {
      icon: '🏪',
      label: 'Рестораны',
      route: '/admin/restaurants',
      roles: ['ADMIN']
    },
    {
      icon: '📂',
      label: 'Категории',
      route: '/admin/categories',
      roles: ['ADMIN']
    },
    {
      icon: '🍽️',
      label: 'Блюда',
      route: '/admin/menu-items',
      roles: ['ADMIN']
    },
    {
      icon: '📋',
      label: 'Заказы',
      route: '/admin/orders',
      roles: ['ADMIN']
    },
    {
      icon: '👥',
      label: 'Пользователи',
      route: '/admin/users',
      roles: ['ADMIN']
    }
  ];

  filteredMenuItems: MenuItem[] = [];

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCurrentUser();
    this.trackRouteChanges();
    this.checkAdminAccess();
    this.restoreSidebarState();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadCurrentUser(): void {
    this.currentUser = this.authService.getCurrentUserValue();

    this.authService.getCurrentUser()
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
        this.filterMenuByPermissions();
      });

    this.filterMenuByPermissions();
  }

  private filterMenuByPermissions(): void {
    if (!this.currentUser?.roles) {
      this.filteredMenuItems = [];
      return;
    }

    this.filteredMenuItems = this.menuItems.filter(item => {
      if (!item.roles) return true;
      return item.roles.some(role =>
        this.currentUser?.roles?.includes(role)
      );
    });
  }

  private trackRouteChanges(): void {
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe((event: NavigationEnd) => {
        this.currentRoute = event.url;
      });
  }

  private checkAdminAccess(): void {
    if (!this.currentUser || !this.hasAdminRole()) {
      this.router.navigate(['/']);
    }
  }

  private hasAdminRole(): boolean {
    if (!this.currentUser?.roles) return false;
    return this.currentUser.roles.some(role =>
      role.toUpperCase() === 'ADMIN'
    );
  }

  getCurrentPageTitle(): string {
    const currentItem = this.filteredMenuItems.find(item =>
      this.currentRoute.startsWith(item.route)
    );
    return currentItem?.label || 'Админ-панель';
  }

  toggleSidebar(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;
    localStorage.setItem('admin_sidebar_collapsed', this.sidebarCollapsed.toString());
  }

  logout(): void {
    if (confirm('Вы действительно хотите выйти из админ-панели?')) {
      this.authService.logout()
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.router.navigate(['/login']);
          },
          error: (error) => {
            console.error('Logout error:', error);
            this.router.navigate(['/login']);
          }
        });
    }
  }

  getUserInitial(): string {
    if (!this.currentUser?.username) return 'A';
    return this.currentUser.username.charAt(0).toUpperCase();
  }

  getUserDisplayName(): string {
    return this.currentUser?.username || 'Администратор';
  }

  getUserRole(): string {
    if (!this.currentUser?.roles?.length) return 'Пользователь';

    const roleLabels: { [key: string]: string } = {
      'ADMIN': 'Администратор',
      'MANAGER': 'Менеджер',
      'SUPPORT': 'Поддержка'
    };

    const primaryRole = this.currentUser.roles[0];
    return roleLabels[primaryRole] || primaryRole;
  }

  isMenuItemActive(route: string): boolean {
    return this.currentRoute.startsWith(route);
  }

  getMenuItemClass(item: MenuItem): string {
    const baseClass = 'nav-link';
    const activeClass = this.isMenuItemActive(item.route) ? ' active' : '';
    return baseClass + activeClass;
  }

  private restoreSidebarState(): void {
    const savedState = localStorage.getItem('admin_sidebar_collapsed');
    if (savedState !== null) {
      this.sidebarCollapsed = savedState === 'true';
    }
  }
}
