package com.nomnomgo.courier.ui

import android.content.Intent
import android.os.Bundle
import android.util.Log
import android.view.View
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.lifecycle.lifecycleScope
import com.google.android.material.button.MaterialButton
import com.google.android.material.checkbox.MaterialCheckBox
import com.google.android.material.textfield.TextInputEditText
import com.google.android.material.textfield.TextInputLayout
import com.nomnomgo.courier.R
import com.nomnomgo.courier.api.ApiClient
import com.nomnomgo.courier.auth.TokenManager
import com.nomnomgo.courier.models.LoginRequest
import com.nomnomgo.courier.models.RegisterRequest
import kotlinx.coroutines.launch

class AuthActivity : AppCompatActivity() {

    private lateinit var tokenManager: TokenManager
    private var isLoginMode = true

    // Views
    private lateinit var tilEmail: TextInputLayout
    private lateinit var etEmail: TextInputEditText
    private lateinit var tilUsername: TextInputLayout
    private lateinit var etUsername: TextInputEditText
    private lateinit var tilPassword: TextInputLayout
    private lateinit var etPassword: TextInputEditText
    private lateinit var tilPhone: TextInputLayout
    private lateinit var etPhone: TextInputEditText
    private lateinit var cbIsCourier: MaterialCheckBox
    private lateinit var btnAuth: MaterialButton
    private lateinit var btnSwitchMode: MaterialButton

    companion object {
        private const val TAG = "AuthActivity"
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        try {
            setContentView(R.layout.activity_auth)

            tokenManager = TokenManager(this)

            // Проверяем, не авторизован ли уже пользователь
            if (tokenManager.isLoggedIn() && !tokenManager.isTokenExpired()) {
                navigateToMain()
                return
            }

            initViews()
            setupClickListeners()
            updateUIForMode()

        } catch (e: Exception) {
            Log.e(TAG, "Error in onCreate", e)
            showError("Ошибка инициализации приложения")
        }
    }

    private fun initViews() {
        tilEmail = findViewById(R.id.til_email)
        etEmail = findViewById(R.id.et_email)
        tilUsername = findViewById(R.id.til_username)
        etUsername = findViewById(R.id.et_username)
        tilPassword = findViewById(R.id.til_password)
        etPassword = findViewById(R.id.et_password)
        tilPhone = findViewById(R.id.til_phone)
        etPhone = findViewById(R.id.et_phone)
        cbIsCourier = findViewById(R.id.cb_is_courier)
        btnAuth = findViewById(R.id.btn_auth)
        btnSwitchMode = findViewById(R.id.btn_switch_mode)
    }

    private fun setupClickListeners() {
        btnAuth.setOnClickListener {
            if (isLoginMode) {
                performLogin()
            } else {
                performRegister()
            }
        }

        btnSwitchMode.setOnClickListener {
            isLoginMode = !isLoginMode
            updateUIForMode()
        }
    }

    private fun updateUIForMode() {
        if (isLoginMode) {
            // Режим входа
            tilUsername.visibility = View.GONE
            tilPhone.visibility = View.GONE
            cbIsCourier.visibility = View.GONE

            btnAuth.text = "Войти"
            btnSwitchMode.text = "Нет аккаунта? Зарегистрироваться"
        } else {
            // Режим регистрации
            tilUsername.visibility = View.VISIBLE
            tilPhone.visibility = View.VISIBLE
            cbIsCourier.visibility = View.VISIBLE

            btnAuth.text = "Зарегистрироваться"
            btnSwitchMode.text = "Уже есть аккаунт? Войти"
        }

        clearErrors()
        clearFields()
    }

    private fun clearErrors() {
        tilEmail.error = null
        tilUsername.error = null
        tilPassword.error = null
        tilPhone.error = null
    }

    private fun clearFields() {
        etEmail.text?.clear()
        etUsername.text?.clear()
        etPassword.text?.clear()
        etPhone.text?.clear()
        cbIsCourier.isChecked = false
    }

    private fun performLogin() {
        clearErrors()

        val email = etEmail.text.toString().trim()
        val password = etPassword.text.toString()

        // Валидация
        var hasErrors = false

        if (email.isEmpty()) {
            tilEmail.error = "Введите email"
            hasErrors = true
        }

        if (password.isEmpty()) {
            tilPassword.error = "Введите пароль"
            hasErrors = true
        }

        if (hasErrors) return

        // Отключаем кнопку во время запроса
        btnAuth.isEnabled = false
        btnAuth.text = "Вход..."

        lifecycleScope.launch {
            try {
                Log.d(TAG, "Attempting login with email: $email")

                val request = LoginRequest(login = email, password = password)
                val response = ApiClient.authService.login(request)

                Log.d(TAG, "Login response code: ${response.code()}")
                Log.d(TAG, "Login response successful: ${response.isSuccessful}")

                if (response.isSuccessful) {
                    val loginResponse = response.body()!!
                    Log.d(TAG, "Login successful, username: ${loginResponse.username}")

                    // ИСПРАВЛЕНИЕ: НЕ передаем userId, он будет извлечен из токена
                    tokenManager.saveTokens(
                        accessToken = loginResponse.accessToken,
                        refreshToken = loginResponse.refreshToken,
                        expiresAt = loginResponse.expiresAt,
                        userId = null, // Будет извлечен из JWT токена в TokenManager
                        username = loginResponse.username
                    )

                    Log.d(TAG, "Tokens saved successfully")
                    navigateToMain()
                } else {
                    val errorBody = response.errorBody()?.string()
                    Log.e(TAG, "Login failed: $errorBody")
                    showError("Неверный email или пароль")
                }
            } catch (e: Exception) {
                Log.e(TAG, "Login error", e)
                showError("Ошибка подключения: ${e.message}")
            } finally {
                btnAuth.isEnabled = true
                btnAuth.text = "Войти"
            }
        }
    }

    private fun performRegister() {
        clearErrors()

        val email = etEmail.text.toString().trim()
        val username = etUsername.text.toString().trim()
        val password = etPassword.text.toString()
        val phone = etPhone.text.toString().trim()
        val isCourier = cbIsCourier.isChecked

        // Валидация
        var hasErrors = false

        if (email.isEmpty()) {
            tilEmail.error = "Введите email"
            hasErrors = true
        }

        if (username.isEmpty()) {
            tilUsername.error = "Введите имя пользователя"
            hasErrors = true
        }

        if (password.isEmpty()) {
            tilPassword.error = "Введите пароль"
            hasErrors = true
        } else if (password.length < 6) {
            tilPassword.error = "Пароль должен содержать минимум 6 символов"
            hasErrors = true
        }

        if (hasErrors) return

        // Отключаем кнопку во время запроса
        btnAuth.isEnabled = false
        btnAuth.text = "Регистрация..."

        lifecycleScope.launch {
            try {
                Log.d(TAG, "Attempting registration with email: $email, username: $username")

                val request = RegisterRequest(
                    email = email,
                    username = username,
                    password = password,
                    phoneNumber = phone.ifEmpty { null },
                    isCourier = isCourier
                )

                val response = ApiClient.authService.register(request)

                Log.d(TAG, "Register response code: ${response.code()}")
                Log.d(TAG, "Register response successful: ${response.isSuccessful}")

                if (response.isSuccessful) {
                    val registerResponse = response.body()!!
                    Log.d(TAG, "Registration successful, username: ${registerResponse.username}")

                    // ИСПРАВЛЕНИЕ: НЕ передаем userId, он будет извлечен из токена
                    tokenManager.saveTokens(
                        accessToken = registerResponse.accessToken,
                        refreshToken = registerResponse.refreshToken,
                        expiresAt = registerResponse.expiresAt,
                        userId = null, // Будет извлечен из JWT токена в TokenManager
                        username = registerResponse.username
                    )

                    Log.d(TAG, "Tokens saved successfully")
                    navigateToMain()
                } else {
                    val errorBody = response.errorBody()?.string()
                    Log.e(TAG, "Registration failed: $errorBody")
                    showError("Ошибка регистрации")
                }
            } catch (e: Exception) {
                Log.e(TAG, "Registration error", e)
                showError("Ошибка подключения: ${e.message}")
            } finally {
                btnAuth.isEnabled = true
                btnAuth.text = "Зарегистрироваться"
            }
        }
    }

    private fun showError(message: String) {
        Toast.makeText(this, message, Toast.LENGTH_LONG).show()
    }

    private fun navigateToMain() {
        try {
            Log.d(TAG, "Navigating to MainActivity")
            val intent = Intent(this, MainActivity::class.java)
            intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
            startActivity(intent)
            finish()
        } catch (e: Exception) {
            Log.e(TAG, "Error navigating to MainActivity", e)
            showError("Ошибка перехода к главному экрану")
        }
    }
}