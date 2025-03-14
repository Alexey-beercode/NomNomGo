-- Создание таблицы пользователей
CREATE TABLE Users (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Email VARCHAR(255) UNIQUE NOT NULL,
    Username VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash TEXT NOT NULL,
    PhoneNumber VARCHAR(20) UNIQUE,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP DEFAULT NOW(),
    IsBlocked BOOLEAN DEFAULT FALSE,
    BlockedUntil TIMESTAMP NULL
);

-- Создание таблицы ролей
CREATE TABLE Roles (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(50) UNIQUE NOT NULL
);

-- Создание таблицы для связи пользователей и ролей (многие-ко-многим)
CREATE TABLE UserRoles (
    UserId UUID REFERENCES Users(Id) ON DELETE CASCADE,
    RoleId UUID REFERENCES Roles(Id) ON DELETE CASCADE,
    PRIMARY KEY (UserId, RoleId)
);

-- Создание таблицы Refresh-токенов для обновления JWT
CREATE TABLE RefreshTokens (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID REFERENCES Users(Id) ON DELETE CASCADE,
    Token TEXT NOT NULL,
    ExpiresAt TIMESTAMP NOT NULL,
    CreatedAt TIMESTAMP DEFAULT NOW()
);

-- Создание таблицы прав доступа (Permissions)
CREATE TABLE Permissions (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(100) UNIQUE NOT NULL
);

-- Создание таблицы для связи ролей и прав доступа (многие-ко-многим)
CREATE TABLE RolePermissions (
    RoleId UUID REFERENCES Roles(Id) ON DELETE CASCADE,
    PermissionId UUID REFERENCES Permissions(Id) ON DELETE CASCADE,
    PRIMARY KEY (RoleId, PermissionId)
);

-- Индексы для ускорения запросов
CREATE INDEX idx_users_email ON Users(Email);
CREATE INDEX idx_users_username ON Users(Username);
CREATE INDEX idx_users_phonenumber ON Users(PhoneNumber);
CREATE INDEX idx_refresh_tokens_userid ON RefreshTokens(UserId);
CREATE INDEX idx_roles_name ON Roles(Name);
CREATE INDEX idx_permissions_name ON Permissions(Name);
