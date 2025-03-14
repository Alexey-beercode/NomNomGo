-- 📌 1. Таблица типов купонов
CREATE TABLE CouponTypes (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(50) UNIQUE NOT NULL -- (Unique, General)
);

-- 📌 2. Таблица купонов
CREATE TABLE Coupons (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Code VARCHAR(50) UNIQUE NOT NULL,
    Discount DECIMAL(10,2) NOT NULL,
    CouponTypeId UUID NOT NULL REFERENCES CouponTypes(Id) ON DELETE CASCADE,
    MaxUses INT DEFAULT NULL, -- Если NULL, то без ограничений
    Uses INT DEFAULT 0,
    UserLimit INT DEFAULT NULL, -- Максимальное число использований одним пользователем
    ExpiryDate TIMESTAMP NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT NOW()
);

-- 📌 3. Таблица истории использования купонов
CREATE TABLE CouponUsage (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    CouponId UUID NOT NULL REFERENCES Coupons(Id) ON DELETE CASCADE,
    UserId UUID NOT NULL,
    OrderId UUID NOT NULL, -- Связка с заказом
    UsedAt TIMESTAMP DEFAULT NOW()
);

-- 📌 4. Индексы для быстрого поиска
CREATE INDEX idx_coupons_code ON Coupons(Code);
CREATE INDEX idx_coupon_usage_couponid ON CouponUsage(CouponId);
CREATE INDEX idx_coupon_usage_userid ON CouponUsage(UserId);
