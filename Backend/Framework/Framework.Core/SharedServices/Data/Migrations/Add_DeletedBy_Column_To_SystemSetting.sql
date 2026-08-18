-- Migration: Add DeletedBy Column to SystemSetting table
-- Date: 2026-05-05
-- Description: Adds DeletedBy column to support soft-delete functionality
--              for SystemSetting entity as required by FullAuditedEntityBase<int> base class

-- IMPORTANT: Choose the correct database based on your environment
-- For Production: USE [IIROSA_Db]
-- For Development: USE [IIROSA_Db_Dev]
USE [IIROSA_Db] -- Change to IIROSA_Db_Dev for development environment
GO

-- Check if column already exists to avoid errors
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('common.SystemSetting')
    AND name = 'DeletedBy'
)
BEGIN
    ALTER TABLE [common].[SystemSetting]
    ADD [DeletedBy] nvarchar(100) NULL;

    PRINT 'DeletedBy column added successfully to SystemSetting table';
END
ELSE
BEGIN
    PRINT 'DeletedBy column already exists in SystemSetting table';
END
GO

PRINT 'Migration completed successfully';
GO
