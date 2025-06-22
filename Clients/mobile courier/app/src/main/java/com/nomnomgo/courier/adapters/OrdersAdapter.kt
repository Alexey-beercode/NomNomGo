package com.nomnomgo.courier.adapter

import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.core.content.ContextCompat
import androidx.recyclerview.widget.RecyclerView
import com.google.android.material.button.MaterialButton
import com.nomnomgo.courier.R
import com.nomnomgo.courier.models.CourierOrder

class OrdersAdapter(
    private var orders: List<CourierOrder>,
    private val onOrderAction: (CourierOrder, String) -> Unit
) : RecyclerView.Adapter<OrdersAdapter.OrderViewHolder>() {

    companion object {
        private const val TAG = "OrdersAdapter"
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): OrderViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_order, parent, false)
        return OrderViewHolder(view)
    }

    override fun onBindViewHolder(holder: OrderViewHolder, position: Int) {
        holder.bind(orders[position])
    }

    override fun getItemCount(): Int = orders.size

    fun updateOrders(newOrders: List<CourierOrder>) {
        orders = newOrders
        notifyDataSetChanged()
    }

    inner class OrderViewHolder(itemView: View) : RecyclerView.ViewHolder(itemView) {
        private val orderIdText: TextView = itemView.findViewById(R.id.order_id)
        private val orderStatusText: TextView = itemView.findViewById(R.id.order_status)
        private val restaurantNameText: TextView = itemView.findViewById(R.id.restaurant_name)
        private val restaurantAddressText: TextView = itemView.findViewById(R.id.restaurant_address)
        private val deliveryAddressText: TextView = itemView.findViewById(R.id.delivery_address)
        private val totalPriceText: TextView = itemView.findViewById(R.id.total_price)
        private val estimatedTimeText: TextView = itemView.findViewById(R.id.estimated_time)
        private val actionButton: MaterialButton = itemView.findViewById(R.id.action_button)
        private val orderNotesText: TextView = itemView.findViewById(R.id.order_notes)

        fun bind(order: CourierOrder) {
            Log.d(TAG, "=== Binding order ===")
            Log.d(TAG, "Order ID: ${order.orderId}")
            Log.d(TAG, "Restaurant object: ${order.restaurant}")
            Log.d(TAG, "Restaurant name: ${order.restaurant?.name}")
            Log.d(TAG, "Restaurant address: ${order.restaurant?.address}")
            Log.d(TAG, "Delivery address: ${order.deliveryAddress}")
            Log.d(TAG, "Status: ${order.status}")

            // ИСПРАВЛЕНИЕ: Правильная работа с nullable объектами
            orderIdText.text = "Заказ #${order.orderId ?: "Неизвестно"}"

            // ИСПРАВЛЕНИЕ: Правильное обращение к restaurant
            restaurantNameText.text = order.restaurant?.name ?: "Ресторан не указан"

            // ИСПРАВЛЕНИЕ: Используем правильный путь к адресу ресторана
            restaurantAddressText.text = order.restaurant?.address ?: "Адрес ресторана не указан"

            // ИСПРАВЛЕНИЕ: Правильное использование deliveryAddress
            deliveryAddressText.text = order.deliveryAddress ?: "Адрес доставки не указан"

            totalPriceText.text = "₽${String.format("%.0f", order.totalPrice)}"

            // ИСПРАВЛЕНИЕ: Правильная обработка estimatedDeliveryTime
            estimatedTimeText.text = order.estimatedDeliveryTime ?: "30 мин"

            // Показываем заметки если есть
            if (!order.orderNotes.isNullOrEmpty()) {
                orderNotesText.text = "Примечание: ${order.orderNotes}"
                orderNotesText.visibility = View.VISIBLE
            } else {
                orderNotesText.visibility = View.GONE
            }

            // Настраиваем статус и кнопку в зависимости от статуса заказа
            val statusValue = order.status ?: "Unknown"
            Log.d(TAG, "Setting status: $statusValue")

            when (statusValue) {
                "Pending" -> {
                    setStatus("Ожидает", R.drawable.status_bg_pending)
                    setActionButton("Принять заказ", "accept")
                }
                "Preparing" -> {
                    setStatus("Готовится", R.drawable.status_bg_preparing)
                    setActionButton("Заказ готовится...", "")
                    actionButton.isEnabled = false
                }
                "Ready" -> {
                    setStatus("Готов", R.drawable.status_bg_ready)
                    setActionButton("Начать доставку", "start_delivery")
                }
                "InDelivery" -> {
                    setStatus("В доставке", R.drawable.status_bg_in_delivery)
                    setActionButton("Доставлен", "complete")
                }
                "Delivered" -> {
                    setStatus("Доставлен", R.drawable.status_bg_ready)
                    actionButton.visibility = View.GONE
                }
                else -> {
                    setStatus(statusValue, R.drawable.status_bg_pending)
                    setActionButton("Принять заказ", "accept")
                }
            }
        }

        private fun setStatus(text: String, backgroundRes: Int) {
            orderStatusText.text = text
            try {
                orderStatusText.background = ContextCompat.getDrawable(itemView.context, backgroundRes)
            } catch (e: Exception) {
                // Если drawable не найден, используем цвет по умолчанию
                orderStatusText.setBackgroundColor(ContextCompat.getColor(itemView.context, R.color.orange_primary))
            }
        }

        private fun setActionButton(text: String, action: String) {
            actionButton.text = text
            actionButton.visibility = View.VISIBLE
            actionButton.isEnabled = true

            if (action.isNotEmpty()) {
                actionButton.setOnClickListener {
                    Log.d(TAG, "Button clicked: $action for order ${orders[adapterPosition].orderId}")
                    onOrderAction(orders[adapterPosition], action)
                }
            } else {
                actionButton.setOnClickListener(null)
            }
        }
    }
}