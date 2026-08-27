-- Add DeliveryCost column to Orders table
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Orders' 
    AND COLUMN_NAME = 'DeliveryCost'
)
BEGIN
    ALTER TABLE Orders
    ADD DeliveryCost decimal(18,2) NOT NULL DEFAULT 0
END
GO 