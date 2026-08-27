-- Check if View column exists, if not add it
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'OrderItems' 
    AND COLUMN_NAME = 'View'
)
BEGIN
    -- Add the View column with default value 'Pending'
    ALTER TABLE OrderItems
    ADD [View] NVARCHAR(MAX) NOT NULL 
    CONSTRAINT DF_OrderItems_View DEFAULT 'Pending';
    
    PRINT 'Added View column to OrderItems table';
END
ELSE
BEGIN
    PRINT 'View column already exists in OrderItems table';
END

-- Remove RestaurantOrderId column if it exists but is causing issues
IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'OrderItems' 
    AND COLUMN_NAME = 'RestaurantOrderId'
)
BEGIN
    -- This might fail if foreign key constraints exist, but worth a try
    ALTER TABLE OrderItems
    DROP COLUMN RestaurantOrderId;
    
    PRINT 'Dropped RestaurantOrderId column from OrderItems table';
END
ELSE
BEGIN
    PRINT 'RestaurantOrderId column does not exist in OrderItems table';
END 