package com.nomnomgo.courier.models

import android.os.Parcelable
import kotlinx.parcelize.Parcelize
import java.time.LocalDateTime

@Parcelize
data class CourierOrder(
    val orderId: String,
    val restaurantName: String,
    val restaurantAddress: String,
    val deliveryAddress: String,
    val status: String,
    val totalPrice: Double,
    val estimatedDeliveryTime: String?,
    val assignedAt: String,
    val notes: String?
) : Parcelable

@Parcelize
data class CourierStats(
    val todayDeliveries: Int,
    val activeDeliveries: Int,
    val totalDeliveries: Int
) : Parcelable

data class LocationUpdate(
    val courierId: String,
    val latitude: Double,
    val longitude: Double
)

data class StatusUpdate(
    val courierId: String,
    val orderId: String,
    val status: String
)

data class AcceptOrderRequest(
    val courierId: String,
    val orderId: String
)
