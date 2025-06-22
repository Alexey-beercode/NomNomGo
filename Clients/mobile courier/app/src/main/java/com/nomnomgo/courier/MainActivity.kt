package com.nomnomgo.courier.ui

import android.Manifest
import android.content.Intent
import android.content.pm.PackageManager
import android.location.Location
import android.os.Bundle
import android.util.Log
import android.view.Menu
import android.view.MenuItem
import android.view.View
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import androidx.lifecycle.lifecycleScope
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout
import com.google.android.gms.location.*
import com.google.android.material.button.MaterialButton
import com.google.android.material.floatingactionbutton.FloatingActionButton
import com.nomnomgo.courier.R
import com.nomnomgo.courier.adapter.OrdersAdapter
import com.nomnomgo.courier.api.ApiClient
import com.nomnomgo.courier.auth.TokenManager
import com.nomnomgo.courier.models.*
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch

class MainActivity : AppCompatActivity() {

    private lateinit var tokenManager: TokenManager
    private lateinit var ordersAdapter: OrdersAdapter
    private lateinit var swipeRefreshLayout: SwipeRefreshLayout
    private lateinit var ordersRecyclerView: RecyclerView
    private lateinit var emptyStateView: View

    // UI элементы статистики
    private lateinit var statusText: TextView
    private lateinit var connectionStatus: TextView
    private lateinit var todayDeliveries: TextView
    private lateinit var activeOrders: TextView
    private lateinit var totalDeliveries: TextView

    // Геолокация
    private lateinit var fusedLocationClient: FusedLocationProviderClient
    private lateinit var locationCallback: LocationCallback
    private var isLocationUpdatesActive = false

    // Данные
    private var currentOrders = mutableListOf<CourierOrder>()
    private var todayDeliveriesCount = 0
    private var totalDeliveriesCount = 0

    companion object {
        private const val TAG = "MainActivity"
        private const val LOCATION_PERMISSION_REQUEST_CODE = 1001
        private const val LOCATION_UPDATE_INTERVAL = 10000L // 10 секунд
        private const val ORDERS_REFRESH_INTERVAL = 30000L // 30 секунд
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        tokenManager = TokenManager(this)

        // Проверяем авторизацию при запуске
        if (!tokenManager.isLoggedIn()) {
            navigateToAuth()
            return
        }

        // Инициализируем токен в ApiClient
        tokenManager.initializeToken()

        // Проверяем срок действия токена
        if (tokenManager.isTokenExpired()) {
            refreshTokenIfNeeded()
        }

        setContentView(R.layout.activity_main)

        initViews()
        setupRecyclerView()
        setupLocationServices()
        setupRefreshLayout()

        // Настройка toolbar
        setSupportActionBar(findViewById(R.id.toolbar))
        supportActionBar?.title = "NomNomGo Курьер"

        // Обновляем UI с данными пользователя
        updateUserInfo()

        // Загружаем заказы
        loadOrders()

        // Запускаем периодическое обновление
        startPeriodicUpdates()

        // Запрашиваем разрешения на геолокацию
        requestLocationPermissions()
    }

    private fun initViews() {
        statusText = findViewById(R.id.status_text)
        connectionStatus = findViewById(R.id.connection_status)
        todayDeliveries = findViewById(R.id.today_deliveries)
        activeOrders = findViewById(R.id.active_orders)
        totalDeliveries = findViewById(R.id.total_deliveries)

        ordersRecyclerView = findViewById(R.id.orders_recycler_view)
        emptyStateView = findViewById(R.id.empty_state)
        swipeRefreshLayout = findViewById(R.id.swipe_refresh_layout)

        // Кнопка обновления
        findViewById<MaterialButton>(R.id.refresh_button).setOnClickListener {
            loadOrders()
        }

        // FAB для тестирования геолокации
        findViewById<FloatingActionButton>(R.id.test_location_fab).setOnClickListener {
            testLocationUpdate()
        }
    }

    private fun setupRecyclerView() {
        ordersAdapter = OrdersAdapter(currentOrders) { order, action ->
            handleOrderAction(order, action)
        }

        ordersRecyclerView.apply {
            layoutManager = LinearLayoutManager(this@MainActivity)
            adapter = ordersAdapter
        }
    }

    private fun setupRefreshLayout() {
        swipeRefreshLayout.setOnRefreshListener {
            loadOrders()
        }
    }

    private fun setupLocationServices() {
        fusedLocationClient = LocationServices.getFusedLocationProviderClient(this)

        locationCallback = object : LocationCallback() {
            override fun onLocationResult(locationResult: LocationResult) {
                locationResult.lastLocation?.let { location ->
                    sendLocationUpdate(location)
                }
            }
        }
    }

    private fun updateUserInfo() {
        val username = tokenManager.getUsername() ?: "Курьер"
        statusText.text = "Статус: Онлайн ($username)"
        connectionStatus.text = "Подключен к серверу"
    }

    private fun loadOrders() {
        val userId = tokenManager.getUserId()
        if (userId == null) {
            navigateToAuth()
            return
        }

        lifecycleScope.launch {
            try {
                swipeRefreshLayout.isRefreshing = true
                connectionStatus.text = "Загрузка заказов..."

                val response = ApiClient.orderService.getActiveOrders()

                if (response.isSuccessful) {
                    val orders = response.body() ?: emptyList()
                    updateOrdersList(orders)
                    connectionStatus.text = "Подключен к серверу"
                } else {
                    showError("Ошибка загрузки заказов: ${response.code()}")
                    connectionStatus.text = "Ошибка подключения"
                }
            } catch (e: Exception) {
                Log.e(TAG, "Error loading orders", e)
                showError("Ошибка подключения: ${e.message}")
                connectionStatus.text = "Ошибка подключения"
            } finally {
                swipeRefreshLayout.isRefreshing = false
            }
        }
    }

    private fun updateOrdersList(orders: List<CourierOrder>) {
        currentOrders.clear()
        currentOrders.addAll(orders)
        ordersAdapter.updateOrders(currentOrders)

        // Обновляем статистику
        activeOrders.text = orders.size.toString()

        // Показываем/скрываем empty state
        if (orders.isEmpty()) {
            ordersRecyclerView.visibility = View.GONE
            emptyStateView.visibility = View.VISIBLE
        } else {
            ordersRecyclerView.visibility = View.VISIBLE
            emptyStateView.visibility = View.GONE
        }
    }

    private fun handleOrderAction(order: CourierOrder, action: String) {
        Log.d(TAG, "=== handleOrderAction called ===")
        Log.d(TAG, "Action: $action")
        Log.d(TAG, "Order ID: ${order.orderId}")

        val userId = tokenManager.getUserId()
        val orderId = order.orderId

        Log.d(TAG, "User ID: $userId")
        Log.d(TAG, "Order ID: $orderId")

        if (userId == null) {
            Log.e(TAG, "User ID is null - navigating to auth")
            navigateToAuth()
            return
        }

        if (orderId == null) {
            Log.e(TAG, "Order ID is null - cannot proceed")
            showError("Некорректный ID заказа")
            return
        }

        Log.d(TAG, "Starting coroutine for action: $action")

        lifecycleScope.launch {
            try {
                when (action) {
                    "accept" -> {
                        Log.d(TAG, "Processing accept action")

                        val statusRequest = CourierStatusUpdate(
                            courierId = userId,
                            orderId = orderId,
                            status = "Preparing"
                        )

                        Log.d(TAG, "Created status request: $statusRequest")
                        Log.d(TAG, "Auth token present: ${ApiClient.getAuthToken() != null}")
                        Log.d(TAG, "Making API call to updateOrderStatus...")

                        val response = ApiClient.orderService.updateOrderStatus(statusRequest)

                        Log.d(TAG, "Response received")
                        Log.d(TAG, "Response code: ${response.code()}")
                        Log.d(TAG, "Response successful: ${response.isSuccessful}")

                        if (!response.isSuccessful) {
                            val errorBody = response.errorBody()?.string()
                            Log.e(TAG, "Response error body: $errorBody")
                        }

                        if (response.isSuccessful) {
                            Log.d(TAG, "Order accepted successfully")
                            val updatedOrder = order.copy(status = "Preparing")
                            updateLocalOrder(updatedOrder)
                            showSuccess("Заказ принят")
                        } else {
                            Log.e(TAG, "Failed to accept order")
                            showError("Ошибка принятия заказа: ${response.code()}")
                        }
                    }
                    "start_delivery" -> {
                        Log.d(TAG, "Processing start_delivery action")
                        updateOrderStatus(order, "InDelivery", "Доставка началась")
                    }
                    "complete" -> {
                        Log.d(TAG, "Processing complete action")
                        updateOrderStatus(order, "Delivered", "Заказ доставлен")
                        // Увеличиваем счетчики
                        todayDeliveriesCount++
                        totalDeliveriesCount++
                        todayDeliveries.text = todayDeliveriesCount.toString()
                        totalDeliveries.text = totalDeliveriesCount.toString()
                    }
                    else -> {
                        Log.w(TAG, "Unknown action: $action")
                    }
                }
            } catch (e: Exception) {
                Log.e(TAG, "Error handling order action", e)
                showError("Ошибка: ${e.message}")
            }
        }
    }

    private suspend fun updateOrderStatus(order: CourierOrder, newStatus: String, successMessage: String) {
        val userId = tokenManager.getUserId() ?: return
        val orderId = order.orderId ?: return

        val request = CourierStatusUpdate(
            courierId = userId,
            orderId = orderId,
            status = newStatus
        )

        val response = ApiClient.orderService.updateOrderStatus(request)

        if (response.isSuccessful) {
            val updatedOrder = order.copy(status = newStatus)
            updateLocalOrder(updatedOrder)
            showSuccess(successMessage)
        } else {
            showError("Ошибка обновления статуса")
        }
    }

    private fun updateLocalOrder(updatedOrder: CourierOrder) {
        val index = currentOrders.indexOfFirst { it.orderId == updatedOrder.orderId }
        if (index != -1) {
            currentOrders[index] = updatedOrder
            ordersAdapter.updateOrders(currentOrders)
        }
    }

    private fun startPeriodicUpdates() {
        lifecycleScope.launch {
            while (true) {
                delay(ORDERS_REFRESH_INTERVAL)
                if (!swipeRefreshLayout.isRefreshing) {
                    loadOrders()
                }
            }
        }
    }

    private fun requestLocationPermissions() {
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION)
            != PackageManager.PERMISSION_GRANTED) {
            ActivityCompat.requestPermissions(
                this,
                arrayOf(Manifest.permission.ACCESS_FINE_LOCATION),
                LOCATION_PERMISSION_REQUEST_CODE
            )
        } else {
            startLocationUpdates()
        }
    }

    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<out String>,
        grantResults: IntArray
    ) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        if (requestCode == LOCATION_PERMISSION_REQUEST_CODE) {
            if (grantResults.isNotEmpty() && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                startLocationUpdates()
            } else {
                showError("Разрешение на геолокацию необходимо для работы приложения")
            }
        }
    }

    private fun startLocationUpdates() {
        if (ActivityCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION)
            != PackageManager.PERMISSION_GRANTED) {
            return
        }

        val locationRequest = LocationRequest.Builder(
            Priority.PRIORITY_HIGH_ACCURACY,
            LOCATION_UPDATE_INTERVAL
        ).build()

        fusedLocationClient.requestLocationUpdates(
            locationRequest,
            locationCallback,
            mainLooper
        )

        isLocationUpdatesActive = true
    }

    private fun stopLocationUpdates() {
        if (isLocationUpdatesActive) {
            fusedLocationClient.removeLocationUpdates(locationCallback)
            isLocationUpdatesActive = false
        }
    }

    private fun sendLocationUpdate(location: Location) {
        val userId = tokenManager.getUserId() ?: return

        lifecycleScope.launch {
            try {
                val locationUpdate = LocationUpdate(
                    courierId = userId,
                    latitude = location.latitude,
                    longitude = location.longitude
                )

                ApiClient.orderService.sendLocation(locationUpdate)
            } catch (e: Exception) {
                Log.w(TAG, "Failed to send location update", e)
            }
        }
    }

    private fun testLocationUpdate() {
        if (ActivityCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION)
            == PackageManager.PERMISSION_GRANTED) {

            fusedLocationClient.lastLocation.addOnSuccessListener { location ->
                if (location != null) {
                    sendLocationUpdate(location)
                    showSuccess("Геолокация отправлена")
                } else {
                    showError("Не удалось получить геолокацию")
                }
            }
        }
    }

    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        menuInflater.inflate(R.menu.main_menu, menu)
        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        return when (item.itemId) {
            R.id.action_logout -> {
                performLogout()
                true
            }
            R.id.action_profile -> {
                showUserInfo()
                true
            }
            else -> super.onOptionsItemSelected(item)
        }
    }

    override fun onResume() {
        super.onResume()
        if (!isLocationUpdatesActive) {
            startLocationUpdates()
        }
    }

    override fun onPause() {
        super.onPause()
        stopLocationUpdates()
    }

    override fun onDestroy() {
        super.onDestroy()
        stopLocationUpdates()
    }

    private fun refreshTokenIfNeeded() {
        val refreshToken = tokenManager.getRefreshToken()
        if (refreshToken != null) {
            lifecycleScope.launch {
                try {
                    val request = RefreshTokenRequest(refreshToken)
                    val response = ApiClient.authService.refreshToken(request)

                    if (response.isSuccessful) {
                        val refreshResponse = response.body()!!
                        val userId = tokenManager.getUserId() ?: ""
                        val username = tokenManager.getUsername() ?: ""

                        tokenManager.saveTokens(
                            accessToken = refreshResponse.accessToken,
                            refreshToken = refreshResponse.refreshToken,
                            expiresAt = refreshResponse.expiresAt,
                            userId = userId,
                            username = username
                        )
                    } else {
                        navigateToAuth()
                    }
                } catch (e: Exception) {
                    navigateToAuth()
                }
            }
        } else {
            navigateToAuth()
        }
    }

    private fun performLogout() {
        lifecycleScope.launch {
            try {
                val refreshToken = tokenManager.getRefreshToken()
                if (refreshToken != null) {
                    val request = RefreshTokenRequest(refreshToken)
                    ApiClient.authService.logout(request)
                }
            } catch (e: Exception) {
                // Игнорируем ошибки при выходе
            } finally {
                stopLocationUpdates()
                tokenManager.clearTokens()
                navigateToAuth()
            }
        }
    }

    private fun showUserInfo() {
        val username = tokenManager.getUsername() ?: "Unknown"
        val userId = tokenManager.getUserId() ?: "Unknown"

        androidx.appcompat.app.AlertDialog.Builder(this)
            .setTitle("Информация о пользователе")
            .setMessage("Пользователь: $username\nID: $userId\nДоставок сегодня: $todayDeliveriesCount\nВсего доставок: $totalDeliveriesCount")
            .setPositiveButton("OK", null)
            .show()
    }

    private fun navigateToAuth() {
        val intent = Intent(this, AuthActivity::class.java)
        intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
        startActivity(intent)
        finish()
    }

    private fun showError(message: String) {
        Toast.makeText(this, message, Toast.LENGTH_LONG).show()
    }

    private fun showSuccess(message: String) {
        Toast.makeText(this, message, Toast.LENGTH_SHORT).show()
    }
}