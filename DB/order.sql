-- Создание таблицы статусов заказов (справочник)
CREATE TABLE OrderStatuses (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(50) UNIQUE NOT NULL -- (Created, Preparing, Ready for delivery, On the way, Completed, Cancelled)
);

-- Создание таблицы заказов
CREATE TABLE Orders (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL, -- Заказчик
    CourierId UUID NOT NULL, -- Курьер (назначается один раз)
    RestaurantId UUID NOT NULL,
    TotalPrice DECIMAL(10,2) NOT NULL, -- Итоговая сумма
    DiscountAmount DECIMAL(10,2) DEFAULT 0, -- Скидка, полученная от CouponService
    StatusId UUID NOT NULL, -- Ссылка на таблицу OrderStatuses
    EstimatedDeliveryTime TIMESTAMP NULL, -- Ожидаемое время доставки (ETA)
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP DEFAULT NOW()
);

-- Установка связи между заказами и статусами
ALTER TABLE Orders ADD CONSTRAINT fk_orders_status FOREIGN KEY (StatusId) REFERENCES OrderStatuses(Id);

-- Создание таблицы элементов заказа (блюда в заказе)
CREATE TABLE OrderItems (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    OrderId UUID REFERENCES Orders(Id) ON DELETE CASCADE,
    MenuItemId UUID NOT NULL, -- Блюдо
    Quantity INT NOT NULL CHECK (Quantity > 0),
    Price DECIMAL(10,2) NOT NULL -- Цена за единицу
);

-- Создание таблицы истории изменений статусов заказа
CREATE TABLE OrderStatusHistory (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    OrderId UUID REFERENCES Orders(Id) ON DELETE CASCADE,
    StatusId UUID REFERENCES OrderStatuses(Id) ON DELETE CASCADE,
    ChangedAt TIMESTAMP DEFAULT NOW()
);

-- Индексы для ускорения поиска
CREATE INDEX idx_orders_userid ON Orders(UserId);
CREATE INDEX idx_orders_courierid ON Orders(CourierId);
CREATE INDEX idx_orders_restaurantid ON Orders(RestaurantId);
CREATE INDEX idx_orders_statusid ON Orders(StatusId);
CREATE INDEX idx_order_items_orderid ON OrderItems(OrderId);
CREATE INDEX idx_order_status_history_orderid ON OrderStatusHistory(OrderId);
