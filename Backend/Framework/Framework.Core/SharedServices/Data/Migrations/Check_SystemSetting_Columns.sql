-- Diagnostic Script: Check SystemSetting table structure
-- This script helps verify if the columns exist and shows the current database

-- Show current database context
SELECT DB_NAME() AS CurrentDatabase;
GO

-- Check if SystemSetting table exists
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'SystemSetting';
GO

-- Show all columns in SystemSetting table
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SystemSetting' AND TABLE_SCHEMA = 'common'
ORDER BY ORDINAL_POSITION;
GO

-- Specifically check for soft-delete columns
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SystemSetting' AND TABLE_SCHEMA = 'common'
AND COLUMN_NAME IN ('DeletedOn', 'IsDeleted');
GO

-- Show sample data to understand the current state
SELECT TOP 5 * FROM [common].[SystemSetting];
GO
