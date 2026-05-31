# Stationery Store Management System

A comprehensive desktop Point-of-Sale (POS) application for stationery shops, built with C# / WPF / .NET 6 / SQL Server.

## 🚀 Quick Start

### Prerequisites
- **OS:** Windows 10/11
- **Runtime:** .NET 6 Desktop Runtime
- **Database:** SQL Server 2019+ (or SQL Server Express)
- **IDE:** Visual Studio 2022

### Database Setup
1. **Restore:** Restore the database from `newdb.bacpac` or run `G2DB.sql` (if available) to create the schema.
2. **Migrate:** Run the following scripts in order:
   - `Migration_ExpiryAndAlerts.sql`
   - `Migration_ExternalBarcode_Fix.sql`
   - `Migration_RemoveCustomerFromOrder.sql`
3. **Seed (Optional):** Run `TestData_SeedItems.sql` for sample data.

### Configuration
1. Open `Configuration.cs` and update the `ConnectionStr` with your SQL Server instance details.
2. Run the application; it will generate a `Settings.xml` file for local preferences.

## 🛠 Building and Running
- **Solution:** `StationeryStoreManagementSystem.sln`
- **Build:** `dotnet build` or F6 in Visual Studio.
- **Run:** `dotnet run` or F5 in Visual Studio.
- **Reports:** Build `Subprojects/CrystalReportApp/` and ensure the output `.exe` is in the same directory as the main application.

## 🏗 Architecture
The project follows a layered architecture:
- **BL (Business Logic):** Domain models (`Product`, `Order`, `Employee`, etc.).
- **DL (Data Layer):** SQL interactions and Data Transfer Objects. `DataHandler.cs` provides a lightweight ORM-like utility.
- **UI (WPF):** Views and user controls. Uses XAML for styling and layout.
- **Services:** Background tasks like `AlertService.cs` for monitoring stock and expiry dates.

## ⌨️ Development Conventions
- **Database First:** The system relies heavily on stored procedures (`stp...`) and views (`..._View`).
- **Singleton Pattern:** Used for database connections (`Configuration.getInstance()`).
- **Shortcuts:** Standardized function keys (F1-F7) for navigation.
- **Barcodes:** Supports both internal (Code 128) and external (EAN-13, etc.) barcodes.

## 📁 Key Files
- `Configuration.cs`: Database connection management.
- `GlobalSettings.cs`: Application-wide settings (Theme, Camera, Email).
- `Utils.cs`: Common utility functions, including barcode generation.
- `UI/ProcessOrder.xaml.cs`: Core POS logic, including barcode scanning (Local & IP Webcam).
- `BL/Product.cs`: Core product domain model with barcode logic.
