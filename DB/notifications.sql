-- 1. Таблица типов уведомлений
CREATE TABLE NotificationTypes (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(50) UNIQUE NOT NULL -- (OrderStatus, Coupon, Promotion, General)
);

-- 2. Таблица методов доставки уведомлений
CREATE TABLE NotificationDeliveryMethods (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Method VARCHAR(50) UNIQUE NOT NULL -- ('Push', 'Email')
);

-- 3. Таблица отправленных уведомлений
CREATE TABLE Notifications (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    NotificationTypeId UUID NOT NULL REFERENCES NotificationTypes(Id) ON DELETE CASCADE,
    Content TEXT NOT NULL,
    SentAt TIMESTAMP DEFAULT NOW(),
    DeliveryMethodId UUID NOT NULL REFERENCES NotificationDeliveryMethods(Id) ON DELETE CASCADE
);

-- Индексы для оптимизации
CREATE INDEX idx_notifications_userid ON Notifications(UserId);
CREATE INDEX idx_notifications_typeid ON Notifications(NotificationTypeId);
CREATE INDEX idx_notifications_deliverymethodid ON Notifications(DeliveryMethodId);
