package com.nomnomgo.courier.auth

import android.content.Context
import android.content.SharedPreferences
import androidx.security.crypto.EncryptedSharedPreferences
import androidx.security.crypto.MasterKey
import com.nomnomgo.courier.api.ApiClient
import java.text.SimpleDateFormat
import java.util.*

class TokenManager(private val context: Context) {

    companion object {
        private const val PREFS_NAME = "auth_prefs"
        private const val KEY_ACCESS_TOKEN = "access_token"
        private const val KEY_REFRESH_TOKEN = "refresh_token"
        private const val KEY_TOKEN_EXPIRY = "token_expiry"
        private const val KEY_USER_ID = "user_id"
        private const val KEY_USERNAME = "username"
        private const val KEY_IS_LOGGED_IN = "is_logged_in"
    }

    private val masterKey = MasterKey.Builder(context)
        .setKeyScheme(MasterKey.KeyScheme.AES256_GCM)
        .build()

    private val encryptedPrefs: SharedPreferences = EncryptedSharedPreferences.create(
        context,
        PREFS_NAME,
        masterKey,
        EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV,
        EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM
    )

    private val dateFormat = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", Locale.getDefault()).apply {
        timeZone = TimeZone.getTimeZone("UTC")
    }

    fun saveTokens(
        accessToken: String,
        refreshToken: String,
        expiresAt: String,
        userId: String? = null,  // Nullable с дефолтом
        username: String? = null // Nullable с дефолтом
    ) {
        try {
            encryptedPrefs.edit().apply {
                putString(KEY_ACCESS_TOKEN, accessToken)
                putString(KEY_REFRESH_TOKEN, refreshToken)
                putString(KEY_TOKEN_EXPIRY, expiresAt)

                // Пытаемся извлечь userId из JWT токена если не передан
                val finalUserId = userId ?: extractUserIdFromJWT(accessToken)
                finalUserId?.let { putString(KEY_USER_ID, it) }

                username?.let { putString(KEY_USERNAME, it) }
                putBoolean(KEY_IS_LOGGED_IN, true)
                apply()
            }

            // Обновляем токен в ApiClient
            ApiClient.setAuthToken(accessToken)
        } catch (e: Exception) {
            android.util.Log.e("TokenManager", "Error saving tokens", e)
            // В случае ошибки очищаем данные
            clearTokens()
        }
    }

    // Добавьте метод для извлечения userId из JWT
    private fun extractUserIdFromJWT(accessToken: String): String? {
        return try {
            val parts = accessToken.split(".")
            if (parts.size >= 2) {
                val payload = String(android.util.Base64.decode(parts[1], android.util.Base64.URL_SAFE or android.util.Base64.NO_PADDING))
                // Ищем "sub" в JSON payload
                val subMatch = Regex("\"sub\":\"([^\"]+)\"").find(payload)
                subMatch?.groupValues?.get(1)
            } else null
        } catch (e: Exception) {
            android.util.Log.e("TokenManager", "Error extracting userId from JWT", e)
            null
        }
    }

    fun getAccessToken(): String? {
        return encryptedPrefs.getString(KEY_ACCESS_TOKEN, null)
    }

    fun getRefreshToken(): String? {
        return encryptedPrefs.getString(KEY_REFRESH_TOKEN, null)
    }

    fun getUserId(): String? {
        return encryptedPrefs.getString(KEY_USER_ID, null)
    }

    fun getUsername(): String? {
        return encryptedPrefs.getString(KEY_USERNAME, null)
    }

    fun isLoggedIn(): Boolean {
        return encryptedPrefs.getBoolean(KEY_IS_LOGGED_IN, false)
    }

    fun isTokenExpired(): Boolean {
        val expiryString = encryptedPrefs.getString(KEY_TOKEN_EXPIRY, null) ?: return true
        return try {
            val expiryDate = dateFormat.parse(expiryString)
            val currentDate = Date()
            expiryDate?.before(currentDate) ?: true
        } catch (e: Exception) {
            true
        }
    }

    fun clearTokens() {
        encryptedPrefs.edit().apply {
            remove(KEY_ACCESS_TOKEN)
            remove(KEY_REFRESH_TOKEN)
            remove(KEY_TOKEN_EXPIRY)
            remove(KEY_USER_ID)
            remove(KEY_USERNAME)
            putBoolean(KEY_IS_LOGGED_IN, false)
            apply()
        }

        // Удаляем токен из ApiClient
        ApiClient.setAuthToken(null)
    }

    fun initializeToken() {
        val token = getAccessToken()
        if (token != null && !isTokenExpired()) {
            ApiClient.setAuthToken(token)
        } else {
            clearTokens()
        }
    }
}