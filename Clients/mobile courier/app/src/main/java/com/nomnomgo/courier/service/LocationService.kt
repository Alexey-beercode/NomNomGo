package com.nomnomgo.courier.services

import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.Service
import android.content.Intent
import android.content.pm.PackageManager
import android.location.Location
import android.os.Build
import android.os.IBinder
import android.util.Log
import androidx.core.app.ActivityCompat
import androidx.core.app.NotificationCompat
import com.google.android.gms.location.*
import com.nomnomgo.courier.R
import com.nomnomgo.courier.api.ApiClient
import com.nomnomgo.courier.auth.TokenManager
import com.nomnomgo.courier.models.LocationUpdate
import kotlinx.coroutines.*

class LocationService : Service() {

    private lateinit var fusedLocationClient: FusedLocationProviderClient
    private lateinit var locationCallback: LocationCallback
    private lateinit var tokenManager: TokenManager
    private val serviceScope = CoroutineScope(Dispatchers.IO + SupervisorJob())

    companion object {
        private const val TAG = "LocationService"
        private const val NOTIFICATION_ID = 1001
        private const val CHANNEL_ID = "location_updates_channel"
        private const val LOCATION_UPDATE_INTERVAL = 10000L // 10 секунд
    }

    override fun onCreate() {
        super.onCreate()

        tokenManager = TokenManager(this)
        fusedLocationClient = LocationServices.getFusedLocationProviderClient(this)

        createNotificationChannel()

        locationCallback = object : LocationCallback() {
            override fun onLocationResult(locationResult: LocationResult) {
                locationResult.lastLocation?.let { location ->
                    sendLocationToServer(location)
                }
            }
        }
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        createNotificationChannel()

        val notification = createNotification()
        startForeground(NOTIFICATION_ID, notification)

        startLocationUpdates()

        return START_STICKY
    }

    override fun onBind(intent: Intent?): IBinder? = null

    override fun onDestroy() {
        super.onDestroy()
        stopLocationUpdates()
        serviceScope.cancel()
    }

    private fun createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val channel = NotificationChannel(
                CHANNEL_ID,
                "Отслеживание геолокации",
                NotificationManager.IMPORTANCE_LOW
            ).apply {
                description = "Отслеживание местоположения курьера"
                setShowBadge(false)
            }

            val notificationManager = getSystemService(NotificationManager::class.java)
            notificationManager.createNotificationChannel(channel)
        }
    }

    private fun createNotification(): Notification {
        return NotificationCompat.Builder(this, CHANNEL_ID)
            .setContentTitle("NomNomGo Курьер")
            .setContentText("Отслеживание местоположения активно")
            .setSmallIcon(R.drawable.ic_delivery)
            .setOngoing(true)
            .setSilent(true)
            .build()
    }

    private fun startLocationUpdates() {
        if (ActivityCompat.checkSelfPermission(
                this,
                android.Manifest.permission.ACCESS_FINE_LOCATION
            ) != PackageManager.PERMISSION_GRANTED
        ) {
            Log.w(TAG, "Location permission not granted")
            stopSelf()
            return
        }

        val locationRequest = LocationRequest.Builder(
            Priority.PRIORITY_BALANCED_POWER_ACCURACY,
            LOCATION_UPDATE_INTERVAL
        ).build()

        fusedLocationClient.requestLocationUpdates(
            locationRequest,
            locationCallback,
            mainLooper
        )

        Log.d(TAG, "Location updates started")
    }

    private fun stopLocationUpdates() {
        fusedLocationClient.removeLocationUpdates(locationCallback)
        Log.d(TAG, "Location updates stopped")
    }

    private fun sendLocationToServer(location: Location) {
        val userId = tokenManager.getUserId()
        if (userId == null) {
            Log.w(TAG, "User not logged in, stopping service")
            stopSelf()
            return
        }

        serviceScope.launch {
            try {
                val locationUpdate = LocationUpdate(
                    courierId = userId,
                    latitude = location.latitude,
                    longitude = location.longitude
                )

                val response = ApiClient.orderService.sendLocation(locationUpdate)

                if (response.isSuccessful) {
                    Log.d(TAG, "Location sent successfully: ${location.latitude}, ${location.longitude}")
                } else {
                    Log.w(TAG, "Failed to send location: ${response.code()}")
                }

            } catch (e: Exception) {
                Log.e(TAG, "Error sending location", e)
            }
        }
    }
}