package com.example.mobilenomnomgo.model

data class Filter(
    val id: Int,
    val name: String,
    val isSelected: Boolean = false
)