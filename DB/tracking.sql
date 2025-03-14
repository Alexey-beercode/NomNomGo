
CREATE TABLE CourierLocations (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    CourierId UUID NOT NULL,
    Latitude DECIMAL(9,6) NOT NULL,
    Longitude DECIMAL(9,6) NOT NULL,
    RecordedAt TIMESTAMP DEFAULT NOW()
);

CREATE TABLE ActiveDeliveries (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    OrderId UUID UNIQUE NOT NULL REFERENCES Orders(Id) ON DELETE CASCADE,
    CourierId UUID UNIQUE NOT NULL, -- Курьер может вести только один заказ
    EstimatedDeliveryTime TIMESTAMP NOT NULL, -- Прогноз ETA
    AssignedAt TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_courierlocations_courierid ON CourierLocations(CourierId);
CREATE INDEX idx_activedeliveries_courierid ON ActiveDeliveries(CourierId);
CREATE INDEX idx_activedeliveries_orderid ON ActiveDeliveries(OrderId);
