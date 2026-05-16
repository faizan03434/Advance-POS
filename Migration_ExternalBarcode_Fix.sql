-- ============================================================
-- Migration: External Barcode full fix + GetProducts_View fix
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

-- ── 5. Drop & recreate GetProducts_View ──────────────────────
-- This view feeds the Manage Products grid.
-- Price/RetailPrice/DiscountAmount come from the LATEST PriceLog row
-- per product (averaged across suppliers if multiple).
-- Stock = total SupplierStock minus total OrderDetail quantity.
IF OBJECT_ID('GetProducts_View','V') IS NOT NULL
    DROP VIEW GetProducts_View;
GO
CREATE VIEW GetProducts_View AS
SELECT
    P.Id,
    P.Name,
    P.Code,
    P.ExternalBarcode,

    -- Latest price: average across all suppliers for this product
    -- (uses the most recent PriceLog row per supplier)
    ISNULL((
        SELECT AVG(pl.Price)
        FROM PriceLog pl
        WHERE pl.ProductId = P.Id
          AND pl.AddedOn = (
              SELECT MAX(pl2.AddedOn)
              FROM PriceLog pl2
              WHERE pl2.ProductId = pl.ProductId
                AND pl2.SupplierId = pl.SupplierId
          )
    ), 0)                                           AS Price,

    ISNULL((
        SELECT AVG(pl.RetailPrice)
        FROM PriceLog pl
        WHERE pl.ProductId = P.Id
          AND pl.AddedOn = (
              SELECT MAX(pl2.AddedOn)
              FROM PriceLog pl2
              WHERE pl2.ProductId = pl.ProductId
                AND pl2.SupplierId = pl.SupplierId
          )
    ), 0)                                           AS RetailPrice,

    ISNULL((
        SELECT AVG(pl.DiscountAmount)
        FROM PriceLog pl
        WHERE pl.ProductId = P.Id
          AND pl.AddedOn = (
              SELECT MAX(pl2.AddedOn)
              FROM PriceLog pl2
              WHERE pl2.ProductId = pl.ProductId
                AND pl2.SupplierId = pl.SupplierId
          )
    ), 0)                                           AS DiscountAmount,

    -- Company and Category names
    ISNULL(C.Name,  '')                             AS Company,
    ISNULL(Cat.Name,'')                             AS Category,

    -- Number of active suppliers
    (
        SELECT COUNT(*)
        FROM ProductSupplier PS
        WHERE PS.ProductId = P.Id
          AND PS.isDeleted  = 0
    )                                               AS [No. Suppliers],

    -- Current stock = total received - total sold
    ISNULL((
        SELECT SUM(SS.Stock)
        FROM SupplierStock SS
        WHERE SS.ProductId = P.Id
    ), 0)
    -
    ISNULL((
        SELECT SUM(OD.Quantity)
        FROM OrderDetail OD
        WHERE OD.ProductId = P.Id
    ), 0)                                           AS Stock,

    P.ReorderThreshold,
    P.ExpiryDate,
    P.AddedOn

FROM Product P
LEFT JOIN Company  C   ON C.Id   = P.CompanyId
LEFT JOIN Category Cat ON Cat.Id = P.CategoryId
WHERE P.IsDiscontinued = 0;
GO
PRINT 'GetProducts_View recreated with Price/RetailPrice from PriceLog.';

-- ── 6. Verify ─────────────────────────────────────────────────
SELECT TOP 5
    Id, Name, Code, Price, RetailPrice, DiscountAmount,
    Company, Category, [No. Suppliers], Stock
FROM GetProducts_View
ORDER BY Id DESC;
GO

PRINT '=== Migration complete. All done. ===';
