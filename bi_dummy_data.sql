-- BI Dummy Data: 60 days of realistic sales orders
-- Covers: product performance, seasonal trends, market basket, cashier anomalies, dead stock
-- Employees: 5=Faraz, 6=faizan, 8=Zeeshan

SET NOCOUNT ON;

-- SupplierStock ensure karo (so dead stock detection works for non-selling items)
-- Rice (Id=2) has no supplier/price so skip it
-- Add stock for all products that have PriceLog
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=1  AND SupplierId=1) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(1,1,500,GETDATE());
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=3  AND SupplierId=2) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(3,2,200,GETDATE());
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=4  AND SupplierId=3) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(4,3,150,GETDATE());
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=5  AND SupplierId=2) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(5,2,300,GETDATE());
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=6  AND SupplierId=3) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(6,3,80,GETDATE());
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=7  AND SupplierId=4) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(7,4,250,GETDATE());
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=8  AND SupplierId=5) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(8,5,180,GETDATE());
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=9  AND SupplierId=3) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(9,3,120,GETDATE());
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=10 AND SupplierId=4) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(10,4,90,GETDATE());
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=13 AND SupplierId=1) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(13,1,400,GETDATE());
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=14 AND SupplierId=1) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(14,1,350,GETDATE());
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=15 AND SupplierId=1) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(15,1,200,GETDATE());
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=16 AND SupplierId=1) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(16,1,150,GETDATE());
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=17 AND SupplierId=1) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(17,1,100,GETDATE());
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=18 AND SupplierId=1) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(18,1,120,GETDATE());
IF NOT EXISTS (SELECT 1 FROM SupplierStock WHERE ProductId=19 AND SupplierId=1) INSERT INTO SupplierStock(ProductId,SupplierId,Stock,AddedOn) VALUES(19,1,60,GETDATE());

-- Helper: insert one order with multiple products
-- Order table: EmployeeId, Timestamp, Type=13, ShopId=1, CustomerName
-- OrderDetail: OrderId, ProductId, SupplierId, Price, DiscountAmount, TaxAmount, Quantity

DECLARE @oid INT;

-- ===== DAY -60 to -31: Month 1 =====

-- Order 1: Faraz, Ball Pen + Lays together (basket pair)
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-60,GETDATE()), 13, 1, 'Ali');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,2.00,4.50,3);
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,0.00,0.00,2);

-- Order 2: Faizan with discount anomaly (discount on everything)
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, DATEADD(day,-59,GETDATE()), 13, 1, 'Sara');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,3,2,70.00,10.50,0.00,2);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,10.00,0.00,3);
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,10.00,0.00,2);

-- Order 3: Zeeshan
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, DATEADD(day,-58,GETDATE()), 13, 1, 'Umar');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,4,3,250.00,50.00,0.00,1);
INSERT INTO OrderDetail VALUES(@oid,9,3,220.00,0.00,0.00,2);

-- Order 4: Faraz, Ball Pen + Lays (basket pair again)
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-57,GETDATE()), 13, 1, 'Hina');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,2.00,4.50,5);
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,0.00,0.00,1);

-- Order 5: Faizan discount anomaly
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, DATEADD(day,-56,GETDATE()), 13, 1, 'Bilal');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,17,1,250.00,10.00,0.00,2);
INSERT INTO OrderDetail VALUES(@oid,18,1,130.00,26.00,0.00,3);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,10.00,0.00,2);

-- Order 6
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-55,GETDATE()), 13, 1, 'Nadia');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,5,2,50.00,15.00,0.00,4);
INSERT INTO OrderDetail VALUES(@oid,7,4,130.00,0.00,0.00,2);

-- Order 7: Ball Pen + Lays basket
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, DATEADD(day,-54,GETDATE()), 13, 1, 'Kamran');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,0.00,4.50,2);
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,0.00,0.00,2);

-- Order 8
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, DATEADD(day,-53,GETDATE()), 13, 1, 'Asma');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,16,1,150.00,5.00,0.00,3);
INSERT INTO OrderDetail VALUES(@oid,15,1,150.00,5.00,0.00,2);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,10.00,0.00,4);

-- Order 9
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-52,GETDATE()), 13, 1, 'Tariq');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,8,5,40.00,0.00,0.00,6);
INSERT INTO OrderDetail VALUES(@oid,10,4,90.00,0.00,0.00,2);

-- Order 10: Roohafza + Dairy Milk basket
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, DATEADD(day,-51,GETDATE()), 13, 1, 'Zara');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,17,1,250.00,10.00,0.00,3);
INSERT INTO OrderDetail VALUES(@oid,18,1,130.00,26.00,0.00,2);

-- Order 11
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-50,GETDATE()), 13, 1, 'Imran');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,2.00,4.50,10);
INSERT INTO OrderDetail VALUES(@oid,3,2,70.00,0.00,0.00,3);

-- Order 12: Faizan discount
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, DATEADD(day,-49,GETDATE()), 13, 1, 'Mehwish');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,4,3,250.00,50.00,0.00,2);
INSERT INTO OrderDetail VALUES(@oid,6,3,600.00,120.00,0.00,1);
INSERT INTO OrderDetail VALUES(@oid,17,1,250.00,10.00,0.00,2);

-- Order 13
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-48,GETDATE()), 13, 1, 'Shoaib');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,0.00,0.00,4);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,0.00,0.00,3);

-- Order 14: Ball Pen + Lays basket
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, DATEADD(day,-47,GETDATE()), 13, 1, 'Rabia');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,0.00,4.50,4);
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,0.00,0.00,3);
INSERT INTO OrderDetail VALUES(@oid,18,1,130.00,26.00,0.00,1);

-- Order 15
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, DATEADD(day,-46,GETDATE()), 13, 1, 'Hassan');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,5,2,50.00,15.00,0.00,5);
INSERT INTO OrderDetail VALUES(@oid,9,3,220.00,0.00,0.00,1);
INSERT INTO OrderDetail VALUES(@oid,15,1,150.00,8.00,0.00,2);

-- Order 16
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-45,GETDATE()), 13, 1, 'Amna');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,8,5,40.00,0.00,0.00,8);
INSERT INTO OrderDetail VALUES(@oid,16,1,150.00,0.00,0.00,2);

-- Order 17: Roohafza + Dairy Milk basket
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, DATEADD(day,-44,GETDATE()), 13, 1, 'Fahad');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,17,1,250.00,10.00,0.00,4);
INSERT INTO OrderDetail VALUES(@oid,18,1,130.00,26.00,0.00,3);

-- Order 18: Faizan discount anomaly
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, DATEADD(day,-43,GETDATE()), 13, 1, 'Sadia');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,5.00,4.50,6);
INSERT INTO OrderDetail VALUES(@oid,7,4,130.00,10.00,0.00,2);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,15.00,0.00,3);

-- Order 19
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-42,GETDATE()), 13, 1, 'Waqar');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,4,3,250.00,50.00,0.00,3);
INSERT INTO OrderDetail VALUES(@oid,10,4,90.00,0.00,0.00,4);

-- Order 20
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, DATEADD(day,-41,GETDATE()), 13, 1, 'Lubna');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,0.00,0.00,5);
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,0.00,4.50,3);

-- ===== DAY -30 to -1: Month 2 (higher volume) =====

-- Order 21
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-30,GETDATE()), 13, 1, 'Omer');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,2.00,4.50,8);
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,0.00,0.00,4);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,0.00,0.00,3);

-- Order 22: Faizan discount
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, DATEADD(day,-29,GETDATE()), 13, 1, 'Maira');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,17,1,250.00,20.00,0.00,3);
INSERT INTO OrderDetail VALUES(@oid,18,1,130.00,26.00,0.00,4);
INSERT INTO OrderDetail VALUES(@oid,16,1,150.00,15.00,0.00,2);

-- Order 23
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, DATEADD(day,-28,GETDATE()), 13, 1, 'Raza');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,5,2,50.00,0.00,0.00,6);
INSERT INTO OrderDetail VALUES(@oid,3,2,70.00,0.00,0.00,4);
INSERT INTO OrderDetail VALUES(@oid,9,3,220.00,0.00,0.00,2);

-- Order 24: Ball Pen + Lays basket
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-27,GETDATE()), 13, 1, 'Komal');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,0.00,4.50,12);
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,0.00,0.00,5);

-- Order 25
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, DATEADD(day,-26,GETDATE()), 13, 1, 'Danish');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,6,3,600.00,120.00,0.00,1);
INSERT INTO OrderDetail VALUES(@oid,4,3,250.00,50.00,0.00,2);
INSERT INTO OrderDetail VALUES(@oid,8,5,40.00,5.00,0.00,5);

-- Order 26: Roohafza + Dairy Milk basket
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, DATEADD(day,-25,GETDATE()), 13, 1, 'Saima');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,17,1,250.00,10.00,0.00,5);
INSERT INTO OrderDetail VALUES(@oid,18,1,130.00,26.00,0.00,4);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,0.00,0.00,3);

-- Order 27
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-24,GETDATE()), 13, 1, 'Junaid');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,7,4,130.00,0.00,0.00,6);
INSERT INTO OrderDetail VALUES(@oid,10,4,90.00,0.00,0.00,3);
INSERT INTO OrderDetail VALUES(@oid,15,1,150.00,0.00,0.00,2);

-- Order 28: Faizan heavy discount
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, DATEADD(day,-23,GETDATE()), 13, 1, 'Noor');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,5.00,4.50,10);
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,15.00,0.00,4);
INSERT INTO OrderDetail VALUES(@oid,16,1,150.00,20.00,0.00,3);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,15.00,0.00,5);

-- Order 29
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, DATEADD(day,-22,GETDATE()), 13, 1, 'Aqsa');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,9,3,220.00,0.00,0.00,3);
INSERT INTO OrderDetail VALUES(@oid,6,3,600.00,120.00,0.00,1);

-- Order 30: Ball Pen + Lays basket
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-21,GETDATE()), 13, 1, 'Hamid');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,0.00,4.50,15);
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,0.00,0.00,6);
INSERT INTO OrderDetail VALUES(@oid,18,1,130.00,26.00,0.00,2);

-- Order 31
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, DATEADD(day,-20,GETDATE()), 13, 1, 'Fizza');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,5,2,50.00,15.00,0.00,8);
INSERT INTO OrderDetail VALUES(@oid,3,2,70.00,10.50,0.00,5);
INSERT INTO OrderDetail VALUES(@oid,17,1,250.00,10.00,0.00,3);

-- Order 32
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, DATEADD(day,-19,GETDATE()), 13, 1, 'Shahid');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,8,5,40.00,0.00,0.00,10);
INSERT INTO OrderDetail VALUES(@oid,10,4,90.00,0.00,0.00,5);
INSERT INTO OrderDetail VALUES(@oid,15,1,150.00,0.00,0.00,3);

-- Order 33: Faizan discount
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, DATEADD(day,-18,GETDATE()), 13, 1, 'Huma');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,4,3,250.00,50.00,0.00,3);
INSERT INTO OrderDetail VALUES(@oid,7,4,130.00,15.00,0.00,4);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,20.00,0.00,6);

-- Order 34: Roohafza + Dairy Milk basket
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-17,GETDATE()), 13, 1, 'Rizwan');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,17,1,250.00,10.00,0.00,6);
INSERT INTO OrderDetail VALUES(@oid,18,1,130.00,26.00,0.00,5);

-- Order 35
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, DATEADD(day,-16,GETDATE()), 13, 1, 'Naila');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,0.00,4.50,20);
INSERT INTO OrderDetail VALUES(@oid,16,1,150.00,0.00,0.00,4);

-- Order 36
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-15,GETDATE()), 13, 1, 'Adnan');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,0.00,0.00,8);
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,2.00,4.50,7);
INSERT INTO OrderDetail VALUES(@oid,9,3,220.00,0.00,0.00,2);

-- Order 37: Faizan discount anomaly
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, DATEADD(day,-14,GETDATE()), 13, 1, 'Samina');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,6,3,600.00,120.00,0.00,2);
INSERT INTO OrderDetail VALUES(@oid,17,1,250.00,25.00,0.00,3);
INSERT INTO OrderDetail VALUES(@oid,18,1,130.00,26.00,0.00,4);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,20.00,0.00,5);

-- Order 38
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, DATEADD(day,-13,GETDATE()), 13, 1, 'Pervez');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,5,2,50.00,0.00,0.00,10);
INSERT INTO OrderDetail VALUES(@oid,3,2,70.00,0.00,0.00,6);

-- Order 39: Ball Pen + Lays basket
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-12,GETDATE()), 13, 1, 'Mariam');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,0.00,4.50,18);
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,0.00,0.00,9);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,0.00,0.00,4);

-- Order 40
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, DATEADD(day,-11,GETDATE()), 13, 1, 'Jamal');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,7,4,130.00,0.00,0.00,7);
INSERT INTO OrderDetail VALUES(@oid,8,5,40.00,0.00,0.00,12);
INSERT INTO OrderDetail VALUES(@oid,15,1,150.00,0.00,0.00,4);

-- Order 41: Faizan discount
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, DATEADD(day,-10,GETDATE()), 13, 1, 'Rukhsar');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,16,1,150.00,20.00,0.00,5);
INSERT INTO OrderDetail VALUES(@oid,10,4,90.00,10.00,0.00,4);
INSERT INTO OrderDetail VALUES(@oid,4,3,250.00,50.00,0.00,2);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,15.00,0.00,6);

-- Order 42
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-9,GETDATE()), 13, 1, 'Irfan');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,17,1,250.00,10.00,0.00,7);
INSERT INTO OrderDetail VALUES(@oid,18,1,130.00,26.00,0.00,6);
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,0.00,4.50,10);

-- Order 43
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, DATEADD(day,-8,GETDATE()), 13, 1, 'Shaheen');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,9,3,220.00,0.00,0.00,4);
INSERT INTO OrderDetail VALUES(@oid,6,3,600.00,120.00,0.00,1);
INSERT INTO OrderDetail VALUES(@oid,3,2,70.00,0.00,0.00,7);

-- Order 44: Ball Pen + Lays basket
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-7,GETDATE()), 13, 1, 'Bashir');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,2.00,4.50,25);
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,0.00,0.00,10);

-- Order 45: Faizan discount anomaly
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, DATEADD(day,-6,GETDATE()), 13, 1, 'Gulshan');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,5,2,50.00,15.00,0.00,6);
INSERT INTO OrderDetail VALUES(@oid,7,4,130.00,20.00,0.00,5);
INSERT INTO OrderDetail VALUES(@oid,17,1,250.00,30.00,0.00,4);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,20.00,0.00,7);

-- Order 46
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, DATEADD(day,-5,GETDATE()), 13, 1, 'Tahir');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,8,5,40.00,0.00,0.00,15);
INSERT INTO OrderDetail VALUES(@oid,16,1,150.00,0.00,0.00,5);
INSERT INTO OrderDetail VALUES(@oid,15,1,150.00,0.00,0.00,4);

-- Order 47: Roohafza + Dairy Milk basket
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-4,GETDATE()), 13, 1, 'Uzma');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,17,1,250.00,10.00,0.00,8);
INSERT INTO OrderDetail VALUES(@oid,18,1,130.00,26.00,0.00,7);
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,0.00,0.00,5);

-- Order 48
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, DATEADD(day,-3,GETDATE()), 13, 1, 'Khalid');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,4,3,250.00,50.00,0.00,4);
INSERT INTO OrderDetail VALUES(@oid,9,3,220.00,0.00,0.00,3);
INSERT INTO OrderDetail VALUES(@oid,10,4,90.00,0.00,0.00,6);

-- Order 49: Ball Pen + Lays basket (today-2)
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, DATEADD(day,-2,GETDATE()), 13, 1, 'Nasreen');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,0.00,4.50,30);
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,0.00,0.00,12);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,0.00,0.00,6);

-- Order 50: Today's orders for dashboard KPIs
INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(5, GETDATE(), 13, 1, 'Walk-in');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,1,1,25.00,2.00,4.50,10);
INSERT INTO OrderDetail VALUES(@oid,14,1,150.00,0.00,0.00,5);
INSERT INTO OrderDetail VALUES(@oid,17,1,250.00,10.00,0.00,3);

INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(8, GETDATE(), 13, 1, 'Walk-in 2');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,18,1,130.00,26.00,0.00,4);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,0.00,0.00,8);
INSERT INTO OrderDetail VALUES(@oid,7,4,130.00,0.00,0.00,3);

INSERT INTO [Order](EmployeeId,Timestamp,Type,ShopId,CustomerName) VALUES(6, GETDATE(), 13, 1, 'Walk-in 3');
SET @oid = SCOPE_IDENTITY();
INSERT INTO OrderDetail VALUES(@oid,4,3,250.00,50.00,0.00,2);
INSERT INTO OrderDetail VALUES(@oid,9,3,220.00,0.00,0.00,1);
INSERT INTO OrderDetail VALUES(@oid,16,1,150.00,15.00,0.00,3);
INSERT INTO OrderDetail VALUES(@oid,13,1,150.00,15.00,0.00,4);

PRINT 'BI dummy data inserted successfully.';
PRINT 'Total new orders: 53';
PRINT 'Features covered:';
PRINT '  - Product Performance: Ball Pen, Lays, Roohafza are top sellers';
PRINT '  - Market Basket: Ball Pen+Lays, Roohafza+DairyMilk appear together frequently';
PRINT '  - Cashier Anomaly: faizan (Id=6) applies discounts on almost every item';
PRINT '  - Seasonal Trends: 2 months of data visible';
PRINT '  - Dead Stock: Product 19 (Lotion) has stock but no recent orders';
