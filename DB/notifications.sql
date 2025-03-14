-- 📌 1. Таблица типов уведомлений
CREATE TABLE NotificationTypes (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(50) UNIQUE NOT NULL -- (OrderStatus, Coupon, Promotion, General)
);

-- 📌 2. Таблица отправленных уведомлений
CREATE TABLE Notifications (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    NotificationTypeId UUID NOT NULL REFERENCES NotificationTypes(Id) ON DELETE CASCADE,
    Content TEXT NOT NULL,
    SentAt TIMESTAMP DEFAULT NOW(),
    DeliveryMethod VARCHAR(50) NOT NULL CHECK (DeliveryMethod IN ('Push', 'Email'))
);

-- 📌 3. Индексы для быстрого поиска
CREATE INDEX idx_notifications_userid ON Notifications(UserId);
CREATE INDEX idx_notifications_typeid ON Notifications(NotificationTypeId);
