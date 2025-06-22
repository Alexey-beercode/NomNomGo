package com.example.mobilenomnomgo.model

data class Restaurant(
    val id: Int,
    val name: String,
    val rating: Float,
    val deliveryTime: String,
    val imageRes: Int,
    var isFavorite: Boolean = false
)