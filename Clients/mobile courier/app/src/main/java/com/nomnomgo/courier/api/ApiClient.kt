package com.nomnomgo.courier.api

import com.nomnomgo.courier.models.*
import okhttp3.Interceptor
import okhttp3.OkHttpClient
import retrofit2.Response
import retrofit2.http.*
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

object ApiClient {
    private const val AUTH_BASE_URL = "http://192.168.238.9:5200/"
    private const val ORDER_BASE_URL = "http://192.168.238.9:5202/"

    private var authToken: String? = null

    fun setAuthToken(token: String?) {
        authToken = token
        recreateRetrofit()
    }

    fun getAuthToken(): String? = authToken

    private val loggingInterceptor = HttpLoggingInterceptor().apply {
        level = HttpLoggingInterceptor.Level.BODY
    }

    private val authInterceptor = Interceptor { chain ->
        val originalRequest = chain.request()
        val requestBuilder = originalRequest.newBuilder()

        authToken?.let { token ->
            requestBuilder.addHeader("Authorization", "Bearer $token")
        }

        chain.proceed(requestBuilder.build())
    }

    private var authHttpClient = OkHttpClient.Builder()
        .addInterceptor(loggingInterceptor)
        .connectTimeout(10, TimeUnit.SECONDS)
        .readTimeout(10, TimeUnit.SECONDS)
        .writeTimeout(10, TimeUnit.SECONDS)
        .build()

    private var orderHttpClient = OkHttpClient.Builder()
        .addInterceptor(loggingInterceptor)
        .addInterceptor(authInterceptor)
        .connectTimeout(10, TimeUnit.SECONDS)
        .readTimeout(10, TimeUnit.SECONDS)
        .writeTimeout(10, TimeUnit.SECONDS)
        .build()

    private var authRetrofit = Retrofit.Builder()
        .baseUrl(AUTH_BASE_URL)
        .client(authHttpClient)
        .addConverterFactory(GsonConverterFactory.create())
        .build()

    private var orderRetrofit = Retrofit.Builder()
        .baseUrl(ORDER_BASE_URL)
        .client(orderHttpClient)
        .addConverterFactory(GsonConverterFactory.create())
        .build()

    val authService: AuthService = authRetrofit.create(AuthService::class.java)
    val orderService: OrderService = orderRetrofit.create(OrderService::class.java)

    private fun recreateRetrofit() {
        orderHttpClient = OkHttpClient.Builder()
            .addInterceptor(loggingInterceptor)
            .addInterceptor(authInterceptor)
            .connectTimeout(10, TimeUnit.SECONDS)
            .readTimeout(10, TimeUnit.SECONDS)
            .writeTimeout(10, TimeUnit.SECONDS)
            .build()

        orderRetrofit = Retrofit.Builder()
            .baseUrl(ORDER_BASE_URL)
            .client(orderHttpClient)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
    }

    fun getAuthBaseUrl(): String = AUTH_BASE_URL
    fun getOrderBaseUrl(): String = ORDER_BASE_URL
}

// Сервис авторизации (порт 5200)
interface AuthService {
    @POST("api/Authentication/login")
    suspend fun login(@Body request: LoginRequest): retrofit2.Response<LoginResponse>

    @POST("api/Authentication/register")
    suspend fun register(@Body request: RegisterRequest): retrofit2.Response<RegisterResponse>

    @POST("api/Authentication/refresh")
    suspend fun refreshToken(@Body request: RefreshTokenRequest): retrofit2.Response<RefreshTokenResponse>

    @POST("api/Authentication/logout")
    suspend fun logout(@Body request: RefreshTokenRequest): retrofit2.Response<Void>

    @GET("api/Authentication/me")
    suspend fun getCurrentUser(): retrofit2.Response<CurrentUserResponse>
}

// Сервис заказов (порт 5202)
interface OrderService {
    // Получение активных заказов для всех курьеров (не назначенных)
    @GET("api/Orders/active")
    suspend fun getActiveOrders(): Response<List<CourierOrder>>

    // Получение заказов конкретного курьера
    @GET("api/Courier/active-orders/{courierId}")
    suspend fun getCourierOrders(@Path("courierId") courierId: String): Response<List<CourierOrder>>

    // Назначение курьера на заказ
    @PUT("api/Orders/{orderId}/assign-courier/{courierId}")
    suspend fun assignCourier(
        @Path("orderId") orderId: String,
        @Path("courierId") courierId: String
    ): Response<Void>

    // Обновление статуса заказа через CourierController
    @POST("api/Courier/update-status")
    suspend fun updateOrderStatus(@Body request: CourierStatusUpdate): Response<Void>

    // Обновление статуса заказа напрямую через OrdersController
    @PUT("api/Orders/{orderId}/status")
    suspend fun updateOrderStatusDirect(
        @Path("orderId") orderId: String,
        @Body request: UpdateOrderStatusRequest
    ): Response<Void>

    // Отправка геолокации
    @POST("api/tracking/location")
    suspend fun sendLocation(@Body locationUpdate: LocationUpdate): Response<Void>
}

// Дополнительная модель для прямого обновления статуса
data class UpdateOrderStatusRequest(
    val status: String,
    val comment: String? = null
)

data class CurrentUserResponse(
    val userId: String,
    val username: String,
    val email: String,
    val phoneNumber: String?,
    val roles: List<String>
)