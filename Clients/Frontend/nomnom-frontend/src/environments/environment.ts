// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5200', // Identity Service - теперь localhost
  menuOrderApiUrl: 'http://localhost:5202', // Menu & Order Service
  recommendationApiUrl: 'http://localhost:5201', // Recommendation Service
  emailApiUrl: 'http://localhost:5204' // Email Service
};

// src/environments/environment.prod.ts
export const environmentProd = {
  production: true,
  apiUrl: 'https://api.nomnomgo.com/identity',
  menuOrderApiUrl: 'https://api.nomnomgo.com/orders',
  recommendationApiUrl: 'https://api.nomnomgo.com/recommendations',
  emailApiUrl: 'https://api.nomnomgo.com/email'
};
