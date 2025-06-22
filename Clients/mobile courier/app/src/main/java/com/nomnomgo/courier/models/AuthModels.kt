package com.nomnomgo.courier.models

import com.google.gson.annotations.SerializedName

// Модели для авторизации
data class LoginRequest(
    val login: String,
    val password: String
)

data class RegisterRequest(
    val email: String,
    val username: String,
    val password: String,
    val phoneNumber: String? = null,
    val isCourier: Boolean = true
)

data class LoginResponse(
    val userId: String? = null, // Сделать nullable, т.к. сервер не возвращает это поле
    val username: String,
    val accessToken: String,
    val refreshToken: String,
    val expiresAt: String
)

data class RegisterResponse(
    val userId: String? = null, // Сделать nullable, т.к. сервер не возвращает это поле
    val username: String,
    val accessToken: String,
    val refreshToken: String,
    val expiresAt: String
)
data class RefreshTokenRequest(
    val refreshToken: String
)

data class RefreshTokenResponse(
    val accessToken: String,
    val refreshToken: String,
    val expiresAt: String
)

data class CurrentUserResponse(
    val userId: String,
    val username: String,
    val email: String,
    val phoneNumber: String?,
    val roles: List<String>
)

data class OrderItem(
    val name: String,
    val quantity: Int,
    val price: Double
)

// Модели для обновления статуса
data class CourierStatusUpdate(
    val courierId: String,
    val orderId: String,
    val status: String // "Ready", "InDelivery", "Delivered"
)

// Модели для геолокации
data class LocationUpdate(
    val courierId: String,
    val latitude: Double,
    val longitude: Double,
    val timestamp: Long = System.currentTimeMillis()
)

data class ApiError(
    val error: String
)