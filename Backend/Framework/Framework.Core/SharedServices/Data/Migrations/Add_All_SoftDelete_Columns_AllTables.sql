-- Add soft delete columns to all FullAuditedEntityBase tables in Commons schema
USE [IIROSA]
GO

DECLARE @TableName NVARCHAR(100)
DECLARE @SchemaName NVARCHAR(50) = 'common'
DECLARE @SQL NVARCHAR(MAX)

-- List of tables that inherit from FullAuditedEntityBase and need soft delete columns
DECLARE TableCursor CURSOR FOR
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'common'
AND TABLE_NAME IN ('Attachment', 'AttachmentType', 'NotificationTemplate', 'NotificationType', 'SystemSetting')

OPEN TableCursor
FETCH NEXT FROM TableCursor INTO @TableName

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Add DeletedBy column if it doesn't exist
    SET @SQL = N'
    IF NOT EXISTS (
        SELECT * FROM sys.columns
        WHERE object_id = OBJECT_ID(''' + @SchemaName + '.' + @TableName + ''')
        AND name = ''DeletedBy''
    )
    BEGIN
        PRINT ''Adding DeletedBy to ' + @TableName + '''
        ALTER TABLE ' + @SchemaName + '.' + @TableName + '
        ADD DeletedBy NVARCHAR(MAX) NULL
    END'

    EXEC sp_executesql @SQL

    -- Add DeletedOn column if it doesn't exist
    SET @SQL = N'
    IF NOT EXISTS (
        SELECT * FROM sys.columns
        WHERE object_id = OBJECT_ID(''' + @SchemaName + '.' + @TableName + ''')
        AND name = ''DeletedOn''
    )
    BEGIN
        PRINT ''Adding DeletedOn to ' + @TableName + '''
        ALTER TABLE ' + @SchemaName + '.' + @TableName + '
        ADD DeletedOn DATETIME2 NULL
    END'

    EXEC sp_executesql @SQL

    -- Add IsDeleted column if it doesn't exist
    SET @SQL = N'
    IF NOT EXISTS (
        SELECT * FROM sys.columns
        WHERE object_id = OBJECT_ID(''' + @SchemaName + '.' + @TableName + ''')
        AND name = ''IsDeleted''
    )
    BEGIN
        PRINT ''Adding IsDeleted to ' + @TableName + '''
        ALTER TABLE ' + @SchemaName + '.' + @TableName + '
        ADD IsDeleted BIT NOT NULL CONSTRAINT DF_' + @TableName + '_IsDeleted DEFAULT 0
    END'

    EXEC sp_executesql @SQL

    FETCH NEXT FROM TableCursor INTO @TableName
END

CLOSE TableCursor
DEALLOCATE TableCursor

PRINT 'All soft delete columns added successfully to all tables'
GO

-- Verify the columns were added
SELECT
    t.TABLE_SCHEMA AS SchemaName,
    t.TABLE_NAME AS TableName,
    c.COLUMN_NAME AS ColumnName,
    c.DATA_TYPE AS DataType,
    c.IS_NULLABLE AS IsNullable
FROM INFORMATION_SCHEMA.TABLES t
INNER JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
WHERE t.TABLE_SCHEMA = 'common'
AND c.COLUMN_NAME IN ('DeletedBy', 'DeletedOn', 'IsDeleted')
ORDER BY t.TABLE_NAME, c.COLUMN_NAME
GO
