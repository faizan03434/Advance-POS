-- Fix: Assign Supplier + Price for all ExternalBarcode products that have none
-- ProductSupplier needs: SupplierId, ProductId, AddedOn, IsDeleted
-- PriceLog PK = AddedOn only, so each insert must have unique AddedOn

BEGIN TRANSACTION;

-- Step 1: ProductSupplier (only for products that have no supplier yet)
INSERT INTO ProductSupplier (SupplierId, ProductId, AddedOn, IsDeleted)
SELECT 1, p.Id, DATEADD(SECOND, p.Id, GETDATE()), 0
FROM Product p
WHERE p.Id IN (13, 14, 15, 16, 17, 18, 19)
  AND NOT EXISTS (
    SELECT 1 FROM ProductSupplier ps
    WHERE ps.ProductId = p.Id AND ps.IsDeleted = 0
  );

-- Step 2: PriceLog (only for products that have no price yet)
-- Each row gets a unique AddedOn by adding product Id as seconds offset
INSERT INTO PriceLog (ProductId, SupplierId, Price, RetailPrice, DiscountAmount, AddedOn)
SELECT p.Id, 1, 100.00, 150.00, 0.00, DATEADD(SECOND, p.Id * 2, GETDATE())
FROM Product p
WHERE p.Id IN (13, 14, 15, 16, 17, 18, 19)
  AND NOT EXISTS (
    SELECT 1 FROM PriceLog pl
    WHERE pl.ProductId = p.Id AND pl.SupplierId = 1
  );

COMMIT TRANSACTION;

-- Verify: all 7 products should show supplier + price
SELECT p.Id, p.Name, p.ExternalBarcode,
       ps.SupplierId, s.Name AS Supplier,
       pl.Price, pl.RetailPrice
FROM Product p
JOIN ProductSupplier ps ON ps.ProductId = p.Id AND ps.IsDeleted = 0
JOIN Supplier s         ON s.Id = ps.SupplierId
JOIN PriceLog pl        ON pl.ProductId = p.Id AND pl.SupplierId = ps.SupplierId
    AND pl.AddedOn = (SELECT MAX(AddedOn) FROM PriceLog WHERE ProductId = p.Id AND SupplierId = ps.SupplierId)
WHERE p.Id IN (13, 14, 15, 16, 17, 18, 19);
