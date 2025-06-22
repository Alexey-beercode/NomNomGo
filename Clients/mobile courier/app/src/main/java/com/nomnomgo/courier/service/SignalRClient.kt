package com.nomnomgo.courier.services

import android.util.Log
import com.microsoft.signalr.HubConnection
import com.microsoft.signalr.HubConnectionBuilder
import com.microsoft.signalr.HubConnectionState
import com.nomnomgo.courier.api.ApiClient
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch

class SignalRClient(private val courierId: String) {

    companion object {
        private const val TAG = "SignalRClient"
        private const val HUB_URL = "http://192.168.87.9:5202/trackingHub"
    }

    private var hubConnection: HubConnection? = null
    private val scope = CoroutineScope(Dispatchers.IO)

    // Callback для получения новых заказов
    var onNewOrderReceived: ((String) -> Unit)? = null

    // Callback для обновления статуса заказа
    var onOrderStatusUpdated: ((String, String) -> Unit)? = null

    // Callback для изменения статуса подключения
    var onConnectionStateChanged: ((Boolean) -> Unit)? = null

    fun connect() {
        val authToken = ApiClient.getAuthToken()

        val builder = HubConnectionBuilder.create(HUB_URL)

        // Если токен есть, добавляем его через заголовки
        if (!authToken.isNullOrEmpty()) {
            builder.withHeader("Authorization", "Bearer $authToken")
        }

        hubConnection = builder.build()

        setupEventHandlers()

        scope.launch {
            try {
                hubConnection?.start()?.blockingAwait()

                if (hubConnection?.connectionState == HubConnectionState.CONNECTED) {
                    Log.d(TAG, "Connected to SignalR hub")
                    onConnectionStateChanged?.invoke(true)

                    // Присоединяемся к группе курьеров
                    hubConnection?.send("JoinCourierGroup", courierId)
                    Log.d(TAG, "Joined courier group: $courierId")
                }

            } catch (e: Exception) {
                Log.e(TAG, "Error connecting to SignalR hub", e)
                onConnectionStateChanged?.invoke(false)
            }
        }
    }

    fun disconnect() {
        scope.launch {
            try {
                hubConnection?.send("LeaveCourierGroup", courierId)
                hubConnection?.stop()?.blockingAwait()
                Log.d(TAG, "Disconnected from SignalR hub")
                onConnectionStateChanged?.invoke(false)
            } catch (e: Exception) {
                Log.e(TAG, "Error disconnecting from SignalR hub", e)
            }
        }
    }

    private fun setupEventHandlers() {
        hubConnection?.on("NewOrderAssigned", { orderId: String ->
            Log.d(TAG, "New order assigned: $orderId")
            onNewOrderReceived?.invoke(orderId)
        }, String::class.java)

        hubConnection?.on("OrderStatusChanged", { orderId: String, newStatus: String ->
            Log.d(TAG, "Order status changed: $orderId -> $newStatus")
            onOrderStatusUpdated?.invoke(orderId, newStatus)
        }, String::class.java, String::class.java)

        hubConnection?.onClosed { error ->
            Log.w(TAG, "SignalR connection closed", error)
            onConnectionStateChanged?.invoke(false)

            // Автоматическое переподключение через 5 секунд
            scope.launch {
                try {
                    Thread.sleep(5000)
                    if (hubConnection?.connectionState != HubConnectionState.CONNECTED) {
                        connect()
                    }
                } catch (e: Exception) {
                    Log.e(TAG, "Error during reconnection", e)
                }
            }
        }
    }

    fun isConnected(): Boolean {
        return hubConnection?.connectionState == HubConnectionState.CONNECTED
    }

    // Отправка обновления геолокации через SignalR (опционально)
    fun sendLocationUpdate(latitude: Double, longitude: Double) {
        scope.launch {
            try {
                if (isConnected()) {
                    hubConnection?.send("UpdateCourierLocation", courierId, latitude, longitude)
                }
            } catch (e: Exception) {
                Log.e(TAG, "Error sending location update via SignalR", e)
            }
        }
    }
}