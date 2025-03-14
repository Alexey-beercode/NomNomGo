-- 📌 1. Таблица статусов сентимент-анализа (справочник)
CREATE TABLE ReviewSentiments (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(50) UNIQUE NOT NULL -- (Positive, Neutral, Negative)
);

-- 📌 2. Таблица отзывов о ресторанах
CREATE TABLE RestaurantReviews (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    RestaurantId UUID NOT NULL REFERENCES Restaurants(Id) ON DELETE CASCADE,
    Rating INT CHECK (Rating BETWEEN 1 AND 5) NOT NULL,
    Comment TEXT,
    SentimentId UUID REFERENCES ReviewSentiments(Id) ON DELETE SET NULL,
    CreatedAt TIMESTAMP DEFAULT NOW()
);

-- 📌 3. Таблица отзывов о блюдах
CREATE TABLE MenuItemReviews (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    MenuItemId UUID NOT NULL REFERENCES MenuItems(Id) ON DELETE CASCADE,
    Rating INT CHECK (Rating BETWEEN 1 AND 5) NOT NULL,
    Comment TEXT,
    SentimentId UUID REFERENCES ReviewSentiments(Id) ON DELETE SET NULL,
    CreatedAt TIMESTAMP DEFAULT NOW()
);

-- 📌 4. Таблица отзывов о курьерах
CREATE TABLE CourierReviews (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    CourierId UUID NOT NULL,
    Rating INT CHECK (Rating BETWEEN 1 AND 5) NOT NULL,
    Comment TEXT,
    SentimentId UUID REFERENCES ReviewSentiments(Id) ON DELETE SET NULL,
    CreatedAt TIMESTAMP DEFAULT NOW()
);

-- 📌 5. Средние рейтинги ресторанов (кешируется)
CREATE TABLE RestaurantRatings (
    RestaurantId UUID PRIMARY KEY REFERENCES Restaurants(Id) ON DELETE CASCADE,
    AverageRating DECIMAL(3,2) NOT NULL DEFAULT 0,
    ReviewCount INT NOT NULL DEFAULT 0,
    LastUpdated TIMESTAMP DEFAULT NOW()
);

-- 📌 6. Средние рейтинги блюд (кешируется)
CREATE TABLE MenuItemRatings (
    MenuItemId UUID PRIMARY KEY REFERENCES MenuItems(Id) ON DELETE CASCADE,
    AverageRating DECIMAL(3,2) NOT NULL DEFAULT 0,
    ReviewCount INT NOT NULL DEFAULT 0,
    LastUpdated TIMESTAMP DEFAULT NOW()
);

-- 📌 7. Средние рейтинги курьеров (кешируется)
CREATE TABLE CourierRatings (
    CourierId UUID PRIMARY KEY,
    AverageRating DECIMAL(3,2) NOT NULL DEFAULT 0,
    ReviewCount INT NOT NULL DEFAULT 0,
    LastUpdated TIMESTAMP DEFAULT NOW()
);

-- 📌 8. Индексы для быстрого поиска
CREATE INDEX idx_restaurant_reviews_restaurantid ON RestaurantReviews(RestaurantId);
CREATE INDEX idx_menuitem_reviews_menuitemid ON MenuItemReviews(MenuItemId);
CREATE INDEX idx_courier_reviews_courierid ON CourierReviews(CourierId);
CREATE INDEX idx_restaurant_ratings ON RestaurantRatings(AverageRating);
CREATE INDEX idx_menuitem_ratings ON MenuItemRatings(AverageRating);
CREATE INDEX idx_courier_ratings ON CourierRatings(AverageRating);
