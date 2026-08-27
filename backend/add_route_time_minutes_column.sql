-- Add RouteTimeMinutes column to Orders table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'RouteTimeMinutes')
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [RouteTimeMinutes] INT NULL;
    
    PRINT 'RouteTimeMinutes column added to Orders table successfully.';
END
ELSE
BEGIN
    PRINT 'RouteTimeMinutes column already exists in Orders table.';
END 