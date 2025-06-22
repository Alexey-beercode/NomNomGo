// src/app/features/admin/models/admin.models.ts

// Базовые интерфейсы для админки
export interface AdminRestaurant {
  id: string;
  name: string;
  address: string;
  phoneNumber: string;
  isActive: boolean;
  averageRating: number;
  reviewCount: number;
  totalOrders: number;
  totalRevenue: number;
  createdAt: string;
  updatedAt: string;
  imageUrl?: string;
  description?: string;
}

export interface AdminCategory {
  id: string;
  name: string;
  description?: string;
  itemsCount: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface AdminMenuItem {
  id: string;
  restaurantId: string;
  restaurantName: string;
  categoryId: string;
  categoryName: string;
  name: string;
  description: string;
  price: number;
  isAvailable: boolean;
  imageUrl?: string;
  averageRating: number;
  reviewCount: number;
  totalOrders: number;
  createdAt: string;
  updatedAt: string;
}

export interface AdminUser {
  userId: string;
  username: string;
  email: string;
  phoneNumber?: string;
  isBlocked: boolean;
  blockedUntil?: string;
  roles: string[];
  totalOrders: number;
  totalSpent: number;
  lastOrderDate?: string;
  createdAt: string;
  updatedAt: string;
}

export interface AdminOrder {
  id: string;
  userId: string;
  username: string;
  userEmail: string;
  restaurantId: string;
  restaurantName: string;
  courierId?: string;
  courierName?: string;
  totalPrice: number;
  discountAmount: number;
  status: OrderStatus;
  estimatedDeliveryTime?: string;
  actualDeliveryTime?: string;
  deliveryAddress: string;
  notes?: string;
  items: AdminOrderItem[];
  createdAt: string;
  updatedAt: string;
}

export interface AdminOrderItem {
  id: string;
  menuItemId: string;
  menuItemName: string;
  quantity: number;
  price: number;
  totalPrice: number;
}

export interface AdminReview {
  id: string;
  userId: string;
  username: string;
  targetId: string;
  targetType: ReviewTargetType;
  targetName: string;
  rating: number;
  comment?: string;
  sentiment?: ReviewSentiment;
  isModerated: boolean;
  isVisible: boolean;
  createdAt: string;
  updatedAt: string;
}

// Енумы
export enum OrderStatus {
  PENDING = 'PENDING',
  CONFIRMED = 'CONFIRMED',
  PREPARING = 'PREPARING',
  READY_FOR_PICKUP = 'READY_FOR_PICKUP',
  OUT_FOR_DELIVERY = 'OUT_FOR_DELIVERY',
  DELIVERED = 'DELIVERED',
  CANCELLED = 'CANCELLED'
}

export enum ReviewTargetType {
  RESTAURANT = 'RESTAURANT',
  MENU_ITEM = 'MENU_ITEM',
  COURIER = 'COURIER'
}

export enum ReviewSentiment {
  POSITIVE = 'POSITIVE',
  NEUTRAL = 'NEUTRAL',
  NEGATIVE = 'NEGATIVE'
}

// Формы
export interface RestaurantFormData {
  name: string;
  address: string;
  phoneNumber: string;
  description: string;
  imageUrl: string;
}

export interface CategoryFormData {
  name: string;
  description: string;
}

export interface MenuItemFormData {
  restaurantId: string;
  categoryId: string;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
}

export interface UserFormData {
  username: string;
  email: string;
  phoneNumber: string;
  roles: string[];
}

// Фильтры
export interface RestaurantFilters {
  search: string;
  isActive?: boolean;
  minRating?: number;
  sortBy: RestaurantSortBy;
  sortOrder: SortOrder;
}

export interface CategoryFilters {
  search: string;
  isActive?: boolean;
  sortBy: CategorySortBy;
  sortOrder: SortOrder;
}

export interface MenuItemFilters {
  search: string;
  restaurantId?: string;
  categoryId?: string;
  isAvailable?: boolean;
  minPrice?: number;
  maxPrice?: number;
  minRating?: number;
  sortBy: MenuItemSortBy;
  sortOrder: SortOrder;
}

export interface UserFilters {
  search: string;
  isBlocked?: boolean;
  role?: string;
  minTotalSpent?: number;
  sortBy: UserSortBy;
  sortOrder: SortOrder;
}

export interface OrderFilters {
  search: string;
  status?: OrderStatus;
  restaurantId?: string;
  dateFrom?: string;
  dateTo?: string;
  minAmount?: number;
  maxAmount?: number;
  sortBy: OrderSortBy;
  sortOrder: SortOrder;
}

export interface ReviewFilters {
  search: string;
  targetType?: ReviewTargetType;
  targetId?: string;
  minRating?: number;
  maxRating?: number;
  sentiment?: ReviewSentiment;
  isModerated?: boolean;
  isVisible?: boolean;
  sortBy: ReviewSortBy;
  sortOrder: SortOrder;
}

// Сортировка
export type SortOrder = 'asc' | 'desc';

export type RestaurantSortBy = 'name' | 'rating' | 'orders' | 'revenue' | 'createdAt';
export type CategorySortBy = 'name' | 'itemsCount' | 'createdAt';
export type MenuItemSortBy = 'name' | 'price' | 'rating' | 'orders' | 'createdAt';
export type UserSortBy = 'username' | 'email' | 'totalSpent' | 'totalOrders' | 'createdAt';
export type OrderSortBy = 'createdAt' | 'totalPrice' | 'status' | 'updatedAt';
export type ReviewSortBy = 'createdAt' | 'rating' | 'updatedAt';

// Пагинация
export interface PaginationParams {
  page: number;
  limit: number;
  total: number;
}

// Состояния
export interface FormState {
  isLoading: boolean;
  errors: { [key: string]: string };
  isDirty: boolean;
  isValid: boolean;
}

export interface ModalState {
  isOpen: boolean;
  title: string;
  type: ModalType;
  data: any;
}

export type ModalType = 'create' | 'edit' | 'delete' | 'view';

// Статистика для дашборда
export interface AdminDashboardStats {
  totalRestaurants: number;
  activeRestaurants: number;
  totalUsers: number;
  activeUsers: number;
  totalOrders: number;
  todayOrders: number;
  totalRevenue: number;
  todayRevenue: number;
  averageOrderValue: number;
  popularRestaurants: PopularRestaurant[];
  recentOrders: AdminOrder[];
  orderStatusDistribution: OrderStatusStats[];
  revenueChart: RevenueChartData[];
}

export interface PopularRestaurant {
  id: string;
  name: string;
  totalOrders: number;
  totalRevenue: number;
  averageRating: number;
}

export interface OrderStatusStats {
  status: OrderStatus;
  count: number;
  percentage: number;
}

export interface RevenueChartData {
  date: string;
  revenue: number;
  orders: number;
}

// Конфигурация таблиц
export interface TableColumn {
  key: string;
  label: string;
  sortable: boolean;
  width?: string;
  type?: 'text' | 'number' | 'date' | 'currency' | 'status' | 'rating' | 'actions';
}

export interface TableConfig {
  columns: TableColumn[];
  sortBy: string;
  sortOrder: SortOrder;
  pageSize: number;
}

// Экспорт данных
export interface ExportRequest {
  format: 'csv' | 'excel' | 'pdf';
  filters: any;
  columns: string[];
}

export interface BulkActionRequest {
  action: 'activate' | 'deactivate' | 'delete' | 'block' | 'unblock';
  ids: string[];
}

// Роли и права доступа
export enum UserRole {
  ADMIN = 'ADMIN',
  MANAGER = 'MANAGER',
  SUPPORT = 'SUPPORT',
  USER = 'USER'
}

export interface Permission {
  resource: string;
  actions: string[];
}

export interface RolePermissions {
  role: UserRole;
  permissions: Permission[];
}
