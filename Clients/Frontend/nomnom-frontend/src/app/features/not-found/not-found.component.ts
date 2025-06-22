// Файл: src/app/features/not-found/not-found.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="not-found-container">
      <div class="not-found-content">
        <div class="error-illustration">
          <div class="error-number">404</div>
          <div class="error-icon">🍕</div>
        </div>

        <div class="error-text">
          <h1>Страница не найдена</h1>
          <p>Кажется, эта страница улетела быстрее нашей доставки!</p>
        </div>

        <div class="error-actions">
          <button class="primary-button" (click)="goHome()">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none">
              <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" stroke="currentColor" stroke-width="2"/>
              <polyline points="9,22 9,12 15,12 15,22" stroke="currentColor" stroke-width="2"/>
            </svg>
            На главную
          </button>

          <button class="secondary-button" (click)="goBack()">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none">
              <path d="M19 12H5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
              <path d="M12 19L5 12L12 5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
            Назад
          </button>
        </div>

        <div class="suggestions">
          <h3>Может быть, вас заинтересует:</h3>
          <div class="suggestion-links">
            <a (click)="navigateTo('/restaurants')" class="suggestion-link">
              🍽️ Рестораны
            </a>
            <a (click)="navigateTo('/profile')" class="suggestion-link">
              👤 Мой профиль
            </a>
            <a (click)="navigateTo('/cart')" class="suggestion-link">
              🛒 Корзина
            </a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      min-height: 100vh;
      background: linear-gradient(135deg, var(--primary-light, #e8f5e1) 0%, var(--background-color, #f8f9fa) 100%);
      font-family: var(--font-main, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif);
    }

    .not-found-container {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      padding: 20px;
    }

    .not-found-content {
      text-align: center;
      max-width: 500px;
      width: 100%;
      background: white;
      border-radius: var(--radius, 16px);
      padding: 48px 32px;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
    }

    .error-illustration {
      position: relative;
      margin-bottom: 32px;
    }

    .error-number {
      font-size: 120px;
      font-weight: 800;
      color: var(--primary-color, #2ecc71);
      opacity: 0.1;
      line-height: 1;
      margin-bottom: -60px;
    }

    .error-icon {
      font-size: 64px;
      position: relative;
      z-index: 1;
      animation: bounce 2s infinite;
    }

    @keyframes bounce {
      0%, 20%, 50%, 80%, 100% {
        transform: translateY(0);
      }
      40% {
        transform: translateY(-10px);
      }
      60% {
        transform: translateY(-5px);
      }
    }

    .error-text {
      margin-bottom: 32px;
    }

    .error-text h1 {
      font-size: 32px;
      font-weight: 700;
      color: var(--text-color, #333);
      margin: 0 0 16px 0;
    }

    .error-text p {
      font-size: 16px;
      color: var(--text-secondary, #666);
      margin: 0;
      line-height: 1.5;
    }

    .error-actions {
      display: flex;
      gap: 16px;
      justify-content: center;
      margin-bottom: 32px;
      flex-wrap: wrap;
    }

    .primary-button,
    .secondary-button {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px 24px;
      border-radius: 8px;
      font-size: 14px;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.2s;
      border: none;
      text-decoration: none;
    }

    .primary-button {
      background-color: var(--primary-color, #2ecc71);
      color: white;
    }

    .primary-button:hover {
      background-color: var(--primary-dark, #27ae60);
      transform: translateY(-1px);
    }

    .secondary-button {
      background: none;
      border: 1px solid var(--border-color, #e0e0e0);
      color: var(--text-secondary, #666);
    }

    .secondary-button:hover {
      background-color: var(--light-gray, #f5f5f5);
      border-color: var(--text-secondary, #666);
    }

    .suggestions {
      padding-top: 24px;
      border-top: 1px solid var(--border-color, #e0e0e0);
    }

    .suggestions h3 {
      font-size: 16px;
      font-weight: 600;
      color: var(--text-color, #333);
      margin: 0 0 16px 0;
    }

    .suggestion-links {
      display: flex;
      gap: 16px;
      justify-content: center;
      flex-wrap: wrap;
    }

    .suggestion-link {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 16px;
      background-color: var(--light-gray, #f5f5f5);
      border-radius: 20px;
      color: var(--text-secondary, #666);
      text-decoration: none;
      font-size: 14px;
      cursor: pointer;
      transition: all 0.2s;
    }

    .suggestion-link:hover {
      background-color: var(--primary-light, #e8f5e1);
      color: var(--primary-color, #2ecc71);
      transform: translateY(-1px);
    }

    @media (max-width: 768px) {
      .not-found-content {
        padding: 32px 24px;
      }

      .error-number {
        font-size: 80px;
        margin-bottom: -40px;
      }

      .error-icon {
        font-size: 48px;
      }

      .error-text h1 {
        font-size: 24px;
      }

      .error-actions {
        flex-direction: column;
        align-items: center;
      }

      .primary-button,
      .secondary-button {
        width: 100%;
        max-width: 200px;
        justify-content: center;
      }

      .suggestion-links {
        flex-direction: column;
        align-items: center;
      }

      .suggestion-link {
        width: 100%;
        max-width: 200px;
        justify-content: center;
      }
    }
  `]
})
export class NotFoundComponent {
  constructor(private router: Router) {}

  goHome(): void {
    this.router.navigate(['/']);
  }

  goBack(): void {
    window.history.back();
  }

  navigateTo(path: string): void {
    this.router.navigate([path]);
  }
}
