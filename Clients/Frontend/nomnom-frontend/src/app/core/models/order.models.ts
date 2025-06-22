// src/app/core/models/order.models.ts

import { RestaurantResponse, MenuItemResponse } from './restaurant.models';

export interface CreateOrderRequest {
  userId: string;
  restaurantId: string;
  deliveryAddress: string;
  notes?: string;
  items: OrderItemRequest[];
}

export interface OrderItemRequest {
  menuItemId: string;
  quantity: number;
}

export interface OrderResponse {
  id: string;
  userId: string;
  courierId?: string;
  restaurant: RestaurantResponse;
  totalPrice: number;
  discountAmount: number;
  status: string;
  estimatedDeliveryTime?: string;
  deliveryAddress: string;
  notes?: string;
  items: OrderItemResponse[];
  createdAt: string;
}

export interface OrderItemResponse {
  id: string;
  menuItem: MenuItemResponse;
  quantity: number;
  price: number;
}

// Дополнительные интерфейсы для UI
export interface Order {
  id: string;
  userId: string;
  courierId?: string;
  restaurant: RestaurantResponse;
  totalPrice: number;
  discountAmount: number;
  status: string;
  estimatedDeliveryTime?: string;
  deliveryAddress: string;
  notes?: string;
  items: OrderItemResponse[];
  createdAt: string;
}

export interface OrderStatusInfo {
  text: string;
  color: string;
  icon: string;
}
