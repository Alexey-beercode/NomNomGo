-- 📌 1. Таблица ресторанов
CREATE TABLE Restaurants (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(255) NOT NULL,
    Address TEXT NOT NULL,
    PhoneNumber VARCHAR(20) UNIQUE NOT NULL,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP DEFAULT NOW()
);

-- 📌 2. Категории блюд (например, "Пицца", "Суши")
CREATE TABLE Categories (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(100) UNIQUE NOT NULL
);

-- 📌 3. Блюда ресторана
CREATE TABLE MenuItems (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    RestaurantId UUID NOT NULL REFERENCES Restaurants(Id) ON DELETE CASCADE,
    CategoryId UUID NOT NULL REFERENCES Categories(Id) ON DELETE CASCADE,
    Name VARCHAR(255) NOT NULL,
    Description TEXT,
    Price DECIMAL(10,2) NOT NULL,
    IsAvailable BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP DEFAULT NOW()
);

-- 📌 4. История изменений меню (например, изменение цены)
CREATE TABLE MenuItemChanges (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    MenuItemId UUID REFERENCES MenuItems(Id) ON DELETE CASCADE,
    OldPrice DECIMAL(10,2),
    NewPrice DECIMAL(10,2),
    ChangedAt TIMESTAMP DEFAULT NOW()
);

-- 📌 5. Таблица статусов парсинга (справочник)
CREATE TABLE ParsingStatuses (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(50) UNIQUE NOT NULL -- (Pending, In Progress, Completed, Failed)
);

-- 📌 6. Очередь парсинга ресторанов
CREATE TABLE ParsingQueue (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    RestaurantId UUID NOT NULL REFERENCES Restaurants(Id) ON DELETE CASCADE,
    StatusId UUID NOT NULL REFERENCES ParsingStatuses(Id) ON DELETE RESTRICT,
    StartedAt TIMESTAMP NULL,
    CompletedAt TIMESTAMP NULL
);

-- 📌 7. Индексы для быстрого поиска
CREATE INDEX idx_restaurants_name ON Restaurants(Name);
CREATE INDEX idx_menuitems_restaurantid ON MenuItems(RestaurantId);
CREATE INDEX idx_menuitems_categoryid ON MenuItems(CategoryId);
CREATE INDEX idx_menuitemchanges_menuitemid ON MenuItemChanges(MenuItemId);
CREATE INDEX idx_parsingqueue_status ON ParsingQueue(StatusId);
