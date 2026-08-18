-- Migration: Add All SoftDelete Columns to SystemSetting table
-- Date: 2026-05-05
-- Description: Adds DeletedOn, IsDeleted, and DeletedBy columns to support soft-delete functionality
--              for SystemSetting entity as required by FullAuditedEntityBase<int> base class

-- IMPORTANT: Choose the correct database based on your environment
-- For Production: USE [IIROSA_Db]
-- For Development: USE [IIROSA_Db_Dev]
USE [IIROSA_Db_Dev] -- Development database
GO

-- Add DeletedOn column if not exists
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('common.SystemSetting')
    AND name = 'DeletedOn'
)
BEGIN
    ALTER TABLE [common].[SystemSetting]
    ADD [DeletedOn] datetime2 NULL;

    PRINT 'DeletedOn column added successfully to SystemSetting table';
END
ELSE
BEGIN
    PRINT 'DeletedOn column already exists in SystemSetting table';
END
GO

-- Add IsDeleted column if not exists
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('common.SystemSetting')
    AND name = 'IsDeleted'
)
BEGIN
    ALTER TABLE [common].[SystemSetting]
    ADD [IsDeleted] bit NOT NULL DEFAULT 0;

    PRINT 'IsDeleted column added successfully to SystemSetting table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in SystemSetting table';
END
GO

-- Add DeletedBy column if not exists
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

-- Update existing records to set IsDeleted = 0 (active)
UPDATE [common].[SystemSetting]
SET [IsDeleted] = 0
WHERE [IsDeleted] IS NULL;
GO

PRINT 'All soft delete columns migration completed successfully';
GO
