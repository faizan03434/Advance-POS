-- ============================================================
-- Migration: External Barcode full fix
-- Run this ONCE on your G2DB database (restored from newdb.bacpac)
-- ============================================================
USE G2DB
GO

-- ── 1. Add ExpiryDate column (if not already there) ──────────
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME='Product' AND COLUMN_NAME='ExpiryDate'
)
BEGIN
    ALTER TABLE [dbo].[Product] ADD [ExpiryDate] [date] NULL;
    PRINT 'Added ExpiryDate column.';
END
ELSE
    PRINT 'ExpiryDate already exists.';
GO

-- ── 2. Add ExternalBarcode column (if not already there) ─────
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME='Product' AND COLUMN_NAME='ExternalBarcode'
)
BEGIN
    ALTER TABLE [dbo].[Product] ADD [ExternalBarcode] NVARCHAR(100) NULL;
    PRINT 'Added ExternalBarcode column.';
END
ELSE
    PRINT 'ExternalBarcode already exists.';
GO

-- ── 3. Drop & recreate stpInsertProduct ──────────────────────
IF OBJECT_ID('stpInsertProduct','P') IS NOT NULL
    DROP PROCEDURE stpInsertProduct;
GO
CREATE PROCEDURE stpInsertProduct
    @Name             NVARCHAR(30),
    @Code             NCHAR(5),
    @CompanyId        INT           = NULL,
    @ReorderThreshold INT           = NULL,
    @CategoryId       INT           = NULL,
    @ExpiryDate       DATE          = NULL,
    @ExternalBarcode  NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Product
        (Name, Code, CompanyId, ReorderThreshold, CategoryId,
         ExpiryDate, ExternalBarcode, IsDiscontinued, AddedOn)
    VALUES
        (@Name, @Code, @CompanyId, @ReorderThreshold, @CategoryId,
         @ExpiryDate, @ExternalBarcode, 0, GETDATE());

    -- Return the new product Id so C# can use it immediately
    SELECT SCOPE_IDENTITY() AS NewId;
END
GO
PRINT 'stpInsertProduct updated.';

-- ── 4. Drop & recreate stpUpdateProduct ──────────────────────
IF OBJECT_ID('stpUpdateProduct','P') IS NOT NULL
    DROP PROCEDURE stpUpdateProduct;
GO
CREATE PROCEDURE stpUpdateProduct
    @Id               INT,
    @Name             NVARCHAR(30),
    @Code             NCHAR(5),
    @CompanyId        INT           = NULL,
    @ReorderThreshold INT           = NULL,
    @CategoryId       INT           = NULL,
    @ExpiryDate       DATE          = NULL,
    @ExternalBarcode  NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Product
    SET Name             = @Name,
        Code             = @Code,
        CompanyId        = @CompanyId,
        ReorderThreshold = @ReorderThreshold,
        CategoryId       = @CategoryId,
        ExpiryDate       = @ExpiryDate,
        ExternalBarcode  = @ExternalBarcode,
        UpdatedOn        = GETDATE()
    WHERE Id = @Id;
END
GO
PRINT 'stpUpdateProduct updated.';

-- ── 5. Verify ─────────────────────────────────────────────────
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Product'
ORDER BY ORDINAL_POSITION;
GO

PRINT '=== Migration complete. All done. ===';
