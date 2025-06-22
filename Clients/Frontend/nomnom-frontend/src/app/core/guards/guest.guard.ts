// src/app/core/guards/guest.guard.ts
import {CanActivate, Router} from '@angular/router';
import {Injectable} from '@angular/core';
import {AuthService} from '../services/auth.service';
import {Observable} from 'rxjs';
import {map, take} from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class GuestGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(): Observable<boolean> | Promise<boolean> | boolean {
    return this.authService.getCurrentUser().pipe(
      take(1),
      map(user => {
        if (!user) {
          return true;
        } else {
          // Пользователь уже авторизован, перенаправляем на главную
          this.router.navigate(['/']);
          return false;
        }
      })
    );
  }
}
