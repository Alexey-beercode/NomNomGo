// src/app/features/admin/guards/admin.guard.ts

import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const currentUser = authService.getCurrentUserValue();

  if (!currentUser) {
    router.navigate(['/login'], {
      queryParams: { returnUrl: state.url }
    });
    return false;
  }

  // Проверяем права администратора - только ADMIN роль
  const hasAdminRole = currentUser.roles?.some(role =>
    role.toUpperCase() === 'ADMIN'
  );

  if (!hasAdminRole) {
    router.navigate(['/']);
    return false;
  }

  return true;
};
