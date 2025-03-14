-- 📌 1. История заказов пользователей (для ML-модели)
CREATE TABLE UserOrderHistory (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    RestaurantId UUID NOT NULL,
    MenuItemId UUID NOT NULL,
    OrderDate TIMESTAMP DEFAULT NOW()
);

-- 📌 2. Популярность блюд (для рекомендаций)
CREATE TABLE PopularMenuItems (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    MenuItemId UUID NOT NULL,
    OrderCount INT DEFAULT 0,
    LastUpdated TIMESTAMP DEFAULT NOW()
);

-- 📌 3. Сохраненные ML-модели (если потребуется версия модели)
CREATE TABLE MLModels (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ModelName VARCHAR(100) NOT NULL,
    Version VARCHAR(50) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT NOW()
);

-- 📌 4. Индексы для оптимизации
CREATE INDEX idx_user_order_history_userid ON UserOrderHistory(UserId);
CREATE INDEX idx_popular_menu_items_menuitemid ON PopularMenuItems(MenuItemId);
CREATE INDEX idx_mlmodels_modelname ON MLModels(ModelName);
