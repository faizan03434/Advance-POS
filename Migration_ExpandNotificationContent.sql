-- Migration to support long AI-generated Business Intelligence briefings
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notification') AND name = 'Content')
BEGIN
    ALTER TABLE Notification ALTER COLUMN Content NVARCHAR(MAX);
END
GO
