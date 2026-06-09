-- ============================================================
-- BI Dashboard Realistic Test Data Generator (FIXED)
-- Targets: G2DB
-- Purpose: Validates Forecasts, Market Basket, Anomalies, and RFM
-- ============================================================

USE G2DB;
GO

PRINT 'Starting BI Test Data Generation...';

-- 1. Create a second cashier "Zeeshan" correctly
IF NOT EXISTS (SELECT 1 FROM [User] WHERE FirstName = 'Zeeshan')
BEGIN
    -- CNIC is NCHAR(13), so use 13 digits exactly
    INSERT INTO [User] (FirstName, LastName, Gender, CNIC, DateOfBirth, Contact, City, AddedOn)
    VALUES ('Zeeshan', 'Admin', 8, '4210112345673', '1995-05-10', '03001234567', 14, GETDATE());
    
    DECLARE @NewUserId INT = SCOPE_IDENTITY();
    
    IF @NewUserId IS NOT NULL
    BEGIN
        INSERT INTO UserAccount (UserId, Username, Email, PasswordHash, EmailConfirmed, LoginFailedCount, TwoFactorEnabled, LockoutEnabled)
        VALUES (@NewUserId, 'zeeshan', 'zeeshan@test.com', '5E884898DA28047151D0E56F8DC6292773603D0D6AABBDD62A11EF721D1542D8', 1, 0, 0, 0);
        
        -- Role 6 = Cashier, Status 10 = Active, ShopId 1
        INSERT INTO Employee (Id, Role, Status, ShopId) VALUES (@NewUserId, 6, 10, 1);
        
        INSERT INTO EmployeeSalary (EmployeeId, Salary, AddedOn) VALUES (@NewUserId, 25000, GETDATE());
        PRINT 'Added second cashier (Zeeshan) for anomaly detection.';
    END
END
ELSE
BEGIN
    PRINT 'Cashier Zeeshan already exists.';
END

-- Get IDs dynamically to avoid NULL errors
DECLARE @FaizanId INT = (SELECT TOP 1 Id FROM [User] WHERE FirstName = 'faizan');
DECLARE @ZeeshanId INT = (SELECT TOP 1 Id FROM [User] WHERE FirstName = 'Zeeshan');
DECLARE @OrderTypePos INT = (SELECT TOP 1 Id FROM Lookup WHERE Category = 'OrderType' AND Value = 'Pos');
DECLARE @ShopId INT = (SELECT TOP 1 Id FROM Shop);

-- Default to 6 if faizan not found, but we know he is 6
IF @FaizanId IS NULL SET @FaizanId = 6;
-- If Zeeshan still null for some reason, use Faizan as fallback but log it
IF @ZeeshanId IS NULL SET @ZeeshanId = @FaizanId;

PRINT 'Using IDs: Faizan=' + CAST(@FaizanId AS VARCHAR) + ', Zeeshan=' + CAST(@ZeeshanId AS VARCHAR);

-- 3. Loop to generate 90 days of history
DECLARE @CurrentDate DATE = DATEADD(DAY, -90, GETDATE());
DECLARE @Product1 INT = 1;  -- Ball Pen Blue
DECLARE @Product10 INT = 10; -- Puri Dough Pack
DECLARE @SupplierId INT = (SELECT TOP 1 SupplierId FROM PriceLog WHERE ProductId = 1);
IF @SupplierId IS NULL SET @SupplierId = (SELECT TOP 1 Id FROM Supplier);

PRINT 'Generating 90 days of transactions...';

SET NOCOUNT ON; -- Reduces output noise

WHILE @CurrentDate <= CAST(GETDATE() AS DATE)
BEGIN
    -- Generate 3-7 orders per day
    DECLARE @OrdersToday INT = FLOOR(RAND() * 5) + 3;
    DECLARE @o INT = 1;
    
    WHILE @o <= @OrdersToday
    BEGIN
        DECLARE @EmployeeId INT = CASE WHEN RAND() > 0.5 THEN @FaizanId ELSE @ZeeshanId END;
        DECLARE @CustName NVARCHAR(50) = CASE 
            WHEN RAND() < 0.2 THEN 'Customer A' -- Loyal
            WHEN RAND() < 0.4 THEN 'Customer B' -- Lost
            WHEN RAND() < 0.6 THEN 'Customer C' -- At Risk
            ELSE 'Walk-in' END;

        -- Logical skips for RFM testing
        IF @CustName = 'Customer B' AND DATEDIFF(DAY, DATEADD(DAY, -90, GETDATE()), @CurrentDate) > 15
            SET @CustName = 'Walk-in';
            
        IF @CustName = 'Customer C' AND DATEDIFF(DAY, DATEADD(DAY, -90, GETDATE()), @CurrentDate) > 50
            SET @CustName = 'Walk-in';

        -- Create the Order
        INSERT INTO [Order] (EmployeeId, CustomerName, ShopId, Type, Timestamp)
        VALUES (@EmployeeId, @CustName, @ShopId, @OrderTypePos, CAST(@CurrentDate AS DATETIME) + CAST(CAST(FLOOR(RAND()*12)+8 AS VARCHAR)+':00' AS DATETIME));
        
        DECLARE @OrderId INT = SCOPE_IDENTITY();
        
        IF @OrderId IS NOT NULL
        BEGIN
            -- Anomaly Logic: Zeeshan (or 2nd slot) gives discounts 35% of time, Faizan 2%
            DECLARE @Discount DECIMAL(18,2) = 0;
            IF @EmployeeId = @ZeeshanId AND RAND() < 0.35
                SET @Discount = 10.00;
            ELSE IF @EmployeeId = @FaizanId AND RAND() < 0.02
                SET @Discount = 5.00;

            -- Product 1 (Forecasting target)
            INSERT INTO OrderDetail (OrderId, ProductId, SupplierId, Price, DiscountAmount, TaxAmount, Quantity)
            VALUES (@OrderId, @Product1, @SupplierId, 50.00, @Discount, 0, FLOOR(RAND()*3)+1);

            -- Market Basket Logic: Pair Prod 1 and 10
            IF @o % 3 = 0
            BEGIN
                INSERT INTO OrderDetail (OrderId, ProductId, SupplierId, Price, DiscountAmount, TaxAmount, Quantity)
                VALUES (@OrderId, @Product10, @SupplierId, 120.00, 0, 0, 1);
            END

            -- Random others
            IF RAND() > 0.6
            BEGIN
                DECLARE @RandomProd INT = FLOOR(RAND()*8)+2;
                IF EXISTS(SELECT 1 FROM Product WHERE Id = @RandomProd)
                BEGIN
                    INSERT INTO OrderDetail (OrderId, ProductId, SupplierId, Price, DiscountAmount, TaxAmount, Quantity)
                    VALUES (@OrderId, @RandomProd, @SupplierId, 75.00, 0, 0, 1);
                END
            END
        END

        SET @o = @o + 1;
    END

    SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
END

SET NOCOUNT OFF;
PRINT 'Data generation complete!';
GO
