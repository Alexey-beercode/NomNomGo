// Модель заказа согласно реальному ответу сервера
package com.nomnomgo.courier.models

import com.google.gson.annotations.SerializedName

data class CourierOrder(
    @SerializedName("id")
    val orderId: String?, // Guid от сервера

    @SerializedName("userId")
    val userId: String?,

    @SerializedName("courierId")
    val courierId: String?,

    @SerializedName("restaurant")
    val restaurant: RestaurantResponse?,

    @SerializedName("totalPrice")
    val totalPrice: Double = 0.0,

    @SerializedName("discountAmount")
    val discountAmount: Double = 0.0,

    @SerializedName("status")
    val status: String?,

    @SerializedName("estimatedDeliveryTime")
    val estimatedDeliveryTime: String?, // DateTime от сервера

    @SerializedName("deliveryAddress")
    val deliveryAddress: String?,

    @SerializedName("notes")
    val orderNotes: String?,

    @SerializedName("items")
    val items: List<OrderItemResponse> = emptyList(),

    @SerializedName("createdAt")
    val createdAt: String?
) {
    // Вспомогательные свойства для совместимости
    val restaurantName: String?
        get() = restaurant?.name

    val restaurantAddress: String?
        get() = restaurant?.address
}

data class RestaurantResponse(
    @SerializedName("id")
    val id: String,

    @SerializedName("name")
    val name: String,

    @SerializedName("address")
    val address: String,

    @SerializedName("phone")
    val phone: String?
)

data class OrderItemResponse(
    @SerializedName("id")
    val id: String,

    @SerializedName("name")
    val name: String,

    @SerializedName("quantity")
    val quantity: Int,

    @SerializedName("price")
    val price: Double
)