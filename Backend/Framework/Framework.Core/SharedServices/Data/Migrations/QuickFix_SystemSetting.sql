-- Quick Fix: Add soft-delete columns to SystemSetting table
-- Schema: common
-- Database: IIROSA_Db (or IIROSA_Db_Dev for development)

USE [IIROSA_Db] -- Change to IIROSA_Db_Dev for development
GO

ALTER TABLE [common].[SystemSetting] ADD [DeletedOn] datetime NULL;
ALTER TABLE [common].[SystemSetting] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
GO

PRINT 'Columns added successfully!';
GO
