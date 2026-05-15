-- Migration: Decouple Order from Customer entity for POS walk-in checkout
-- This script removes the Customer FK from Order and replaces it with a plain CustomerName string.

USE G2DB;
GO

-- 1. Drop the foreign key constraint
ALTER TABLE [dbo].[Order] DROP CONSTRAINT [FK_Order_User];
GO

-- 2. Drop the CustomerId column and add CustomerName
ALTER TABLE [dbo].[Order] DROP COLUMN [CustomerId];
GO

ALTER TABLE [dbo].[Order] ADD [CustomerName] NVARCHAR(50) NULL;
GO

-- 3. Recreate stpInsertOrder without CustomerId / PaymentDues
DROP PROCEDURE IF EXISTS [dbo].[stpInsertOrder];
GO

CREATE PROCEDURE stpInsertOrder
    @OrderProducts udtt_OrderProducts READONLY,
    @EmployeeId INT,
    @CustomerName NVARCHAR(50) = NULL
AS
BEGIN
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
    BEGIN TRAN

    DECLARE @ShopId INT = (SELECT TOP(1) Id FROM Shop)
    DECLARE @Type INT = (SELECT Id FROM Lookup WHERE Lookup.Category = 'OrderType' AND Lookup.Value = 'Pos')

    INSERT INTO [Order](EmployeeId, CustomerName, ShopId, Type, Timestamp)
    VALUES (@EmployeeId, @CustomerName, @ShopId, @Type, CURRENT_TIMESTAMP)

    DECLARE @Identity INT = SCOPE_IDENTITY()

    INSERT INTO [OrderDetail](OrderId, ProductId, SupplierId, Price, DiscountAmount, TaxAmount, Quantity)
    SELECT @Identity, op.ProductId, op.SupplierId, op.Price, op.DiscountAmount, op.TaxAmount, op.Quantity
    FROM @OrderProducts op

    COMMIT TRAN
    (SELECT @Identity)
END
GO
