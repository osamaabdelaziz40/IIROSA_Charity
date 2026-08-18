-- Rollback Migration: Remove SoftDelete Columns from SystemSetting table
-- Date: 2026-04-27
-- Description: Removes DeletedOn and IsDeleted columns from SystemSetting table
--              WARNING: This will permanently delete these columns and any data they contain

-- IMPORTANT: Choose the correct database based on your environment
-- For Production: USE [IIROSA_Db]
-- For Development: USE [IIROSA_Db_Dev]
USE [IIROSA_Db] -- Change to IIROSA_Db_Dev for development environment
GO

-- Drop IsDeleted column
IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('common.SystemSetting')
    AND name = 'IsDeleted'
)
BEGIN
    ALTER TABLE [common].[SystemSetting]
    DROP COLUMN [IsDeleted];

    PRINT 'IsDeleted column removed successfully from SystemSetting table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column does not exist in SystemSetting table';
END
GO

-- Drop DeletedOn column
IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('common.SystemSetting')
    AND name = 'DeletedOn'
)
BEGIN
    ALTER TABLE [common].[SystemSetting]
    DROP COLUMN [DeletedOn];

    PRINT 'DeletedOn column removed successfully from SystemSetting table';
END
ELSE
BEGIN
    PRINT 'DeletedOn column does not exist in SystemSetting table';
END
GO

PRINT 'Rollback completed successfully';
GO
