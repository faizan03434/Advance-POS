-- Test Data: Seed a Company, Category, Supplier, Product, ProductSupplier, PriceLog, TaxLog, and Stock
-- so you can scan/type a Product ID in Process Order and test the full POS flow.
-- Run this against your G2DB database AFTER running Migration_RemoveCustomerFromOrder.sql.

USE G2DB;
GO

-- 1. Company
SET IDENTITY_INSERT [dbo].[Company] ON;
INSERT INTO [dbo].[Company] ([Id], [Name], [AddedOn], [IsDeleted])
VALUES (1, N'Dollar Industries', GETDATE(), 0);
SET IDENTITY_INSERT [dbo].[Company] OFF;
GO

-- 2. Category
SET IDENTITY_INSERT [dbo].[Category] ON;
INSERT INTO [dbo].[Category] ([Id], [Name], [Picture], [IsDeleted], [AddedOn])
VALUES (1, N'Writing', NULL, 0, GETDATE());
SET IDENTITY_INSERT [dbo].[Category] OFF;
GO

-- 3. Tax rate for the category (18% GST)
INSERT INTO [dbo].[TaxLog] ([CategoryId], [GST], [AddedOn])
VALUES (1, 18.0, GETDATE());
GO

-- 4. Supplier (Code must be exactly 3 chars)
SET IDENTITY_INSERT [dbo].[Supplier] ON;
INSERT INTO [dbo].[Supplier] ([Id], [Name], [Code], [Contact], [IsDeleted], [Email], [Country], [City], [StreetAddress], [PostalCode], [AddedOn], [Town])
VALUES (1, N'ABC Distributors', N'ABC', N'03001234567', 0, N'abc@dist.com', 12, 9, N'Main Market', N'54000', GETDATE(), N'Gulberg');
SET IDENTITY_INSERT [dbo].[Supplier] OFF;
GO

-- 5. Product (Code must be exactly 5 chars)
SET IDENTITY_INSERT [dbo].[Product] ON;
INSERT INTO [dbo].[Product] ([Id], [Name], [Code], [CompanyId], [ReorderThreshold], [IsDiscontinued], [CategoryId], [AddedOn])
VALUES (1, N'Ball Pen Blue', N'BP001', 1, 10, 0, 1, GETDATE());
SET IDENTITY_INSERT [dbo].[Product] OFF;
GO

-- 6. Link product to supplier
INSERT INTO [dbo].[ProductSupplier] ([SupplierId], [ProductId], [AddedOn], [IsDeleted])
VALUES (1, 1, GETDATE(), 0);
GO

-- 7. Price log (Cost=15, Retail=25, Discount=2)
INSERT INTO [dbo].[PriceLog] ([ProductId], [SupplierId], [Price], [RetailPrice], [DiscountAmount], [AddedOn])
VALUES (1, 1, 15.00, 25.00, 2.00, GETDATE());
GO

-- 8. Stock (50 units)
INSERT INTO [dbo].[SupplierStock] ([SupplierId], [ProductId], [Stock], [Description], [AddedOn], [ShopId], [IsShipment])
VALUES (1, 1, 50, N'Initial stock', GETDATE(), (SELECT TOP(1) Id FROM Shop), 0);
GO

-- =============================================
-- HOW TO USE:
-- The Product ID to enter in the POS is: BP001ABC
--   (5-char product code + 3-char supplier code)
-- Retail Price: 25.00 Rs, Discount: 2.00 Rs
-- So 1 unit = 23.00 Rs (25 - 2)
-- Enter 23 or more in "Received" to confirm.
-- =============================================
