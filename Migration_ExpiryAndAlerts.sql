-- ==========================================================================
-- Migration: Add ExpiryDate to Product + update stored procedures
-- Run this on your existing G2DB database ONCE
-- ==========================================================================

USE G2DB
GO

-- 1. Add ExpiryDate column to Product (nullable)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME='Product' AND COLUMN_NAME='ExpiryDate'
)
BEGIN
    ALTER TABLE [dbo].[Product]
    ADD [ExpiryDate] [date] NULL;
    PRINT 'ExpiryDate column added to Product table.';
END
ELSE
    PRINT 'ExpiryDate already exists, skipping.';
GO

-- 2. Update stpInsertProduct to accept ExpiryDate
IF OBJECT_ID('stpInsertProduct','P') IS NOT NULL
    DROP PROCEDURE stpInsertProduct;
GO
CREATE PROCEDURE stpInsertProduct
    @Name        NVARCHAR(30),
    @Code        NCHAR(5),
    @CompanyId   INT = NULL,
    @ReorderThreshold INT = NULL,
    @CategoryId  INT = NULL,
    @ExpiryDate  DATE = NULL
AS
BEGIN
    INSERT INTO Product(Name, Code, CompanyId, ReorderThreshold, CategoryId,
                        ExpiryDate, IsDiscontinued, AddedOn)
    VALUES (@Name, @Code, @CompanyId, @ReorderThreshold, @CategoryId,
            @ExpiryDate, 0, GETDATE());
END
GO

-- 3. Update stpUpdateProduct to accept ExpiryDate
IF OBJECT_ID('stpUpdateProduct','P') IS NOT NULL
    DROP PROCEDURE stpUpdateProduct;
GO
CREATE PROCEDURE stpUpdateProduct
    @Id          INT,
    @Name        NVARCHAR(30),
    @Code        NCHAR(5),
    @CompanyId   INT = NULL,
    @ReorderThreshold INT = NULL,
    @CategoryId  INT = NULL,
    @ExpiryDate  DATE = NULL
AS
BEGIN
    UPDATE Product
    SET Name=@Name, Code=@Code, CompanyId=@CompanyId,
        ReorderThreshold=@ReorderThreshold, CategoryId=@CategoryId,
        ExpiryDate=@ExpiryDate, UpdatedOn=GETDATE()
    WHERE Id=@Id;
END
GO

-- 4. Low stock helper view
IF OBJECT_ID('GetLowStockProducts_View','V') IS NOT NULL
    DROP VIEW GetLowStockProducts_View;
GO
CREATE VIEW GetLowStockProducts_View AS
SELECT
    P.Id,
    P.Name,
    P.Code,
    P.ReorderThreshold,
    ISNULL(SUM(SS.Stock), 0) - ISNULL((
        SELECT SUM(OD.Quantity) FROM OrderDetail OD WHERE OD.ProductId = P.Id
    ), 0) AS CurrentStock
FROM Product P
LEFT JOIN SupplierStock SS ON P.Id = SS.ProductId
WHERE P.IsDiscontinued = 0
  AND P.ReorderThreshold IS NOT NULL
GROUP BY P.Id, P.Name, P.Code, P.ReorderThreshold;
GO

-- 5. Expiring products helper view
IF OBJECT_ID('GetExpiringProducts_View','V') IS NOT NULL
    DROP VIEW GetExpiringProducts_View;
GO
CREATE VIEW GetExpiringProducts_View AS
SELECT
    Id, Name, Code, ExpiryDate,
    DATEDIFF(DAY, GETDATE(), ExpiryDate) AS DaysUntilExpiry
FROM Product
WHERE IsDiscontinued = 0
  AND ExpiryDate IS NOT NULL
  AND ExpiryDate >= GETDATE();
GO

PRINT 'Migration complete. ExpiryDate support added.';
