import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return (route: ActivatedRouteSnapshot, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    const currentUser = authService.getCurrentUserValue();

    if (!currentUser?.roles) {
      router.navigate(['/admin/dashboard']);
      return false;
    }

    const hasRole = currentUser.roles.some(role =>
      allowedRoles.includes(role.toUpperCase())
    );

    if (!hasRole) {
      router.navigate(['/admin/dashboard']);
      return false;
    }

    return true;
  };
};
