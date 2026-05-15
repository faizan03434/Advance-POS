# Software Engineering Documentation
## Advanced POS System — Stationery Store Management System
**Version:** 2.0 Enhanced  
**Technology Stack:** C# / WPF / .NET 6 / SQL Server  
**Author:** SE Project Team  
**Date:** May 2026

---

## Overview

This document maps every Software Engineering topic covered in the SE course (Weeks 1–9) to concrete implementations within this project. The system is a full-featured **Point-of-Sale (POS) desktop application** for a stationery shop. It handles product management, barcode scanning, order processing, inventory tracking, employee management, automated alerts, and comprehensive reporting.

---

## Week 1 — Introduction to Software Engineering

### Concepts Covered
Software engineering principles: problem solving, abstraction, separation of concerns, professional software development.

### Implementation in This Project

**Problem Being Solved:**  
Small stationery shops use manual systems (paper records, mental inventory tracking) that are error-prone, slow, and unscalable. This system replaces that with a professional digital solution.

**Software Goals Achieved:**
- **Correctability** — All data stored in SQL Server with transaction safety via stored procedures.
- **Efficiency** — Barcode scanning reduces checkout time from ~30 seconds to under 3 seconds per product.
- **Reliability** — Connection managed as a singleton (`Configuration.cs`), preventing duplicate connection overhead.
- **Maintainability** — Code organized in BL / DL / UI layers so any layer can change independently.

**Abstraction Levels:**
1. UI layer — user sees buttons and grids, never SQL.
2. BL layer — business rules (e.g. price = retail − discount, grand total = sum of items).
3. DL layer — raw SQL queries, stored procedures, bulk insert.

---

## Week 2 — Requirements Engineering

### Concepts Covered
Functional vs non-functional requirements, use cases, user stories, system requirements.

### Functional Requirements Implemented

| ID | Requirement | Implementation |
|----|-------------|----------------|
| FR-01 | System must allow products to be added with barcode | `ProductForm.xaml`, `stpInsertProduct` SP |
| FR-02 | System must support barcode scanning at checkout | `ProcessOrder.xaml.cs` — ZXing + camera timer |
| FR-03 | Mobile phone can be used as barcode scanner for testing | IP Webcam mode in `Settings.xaml` |
| FR-04 | System must generate invoices / receipts on confirm | `PrintBill()` in `ProcessOrder.xaml.cs` |
| FR-05 | Admin must receive alerts for low stock | `AlertService.cs` — background timer |
| FR-06 | Admin must be notified before product expiry | `AlertService.cs` + `ExpiryDate` in DB |
| FR-07 | Admin can apply discount to expiring products | `ApplyExpiryDiscountAsync()` + confirm button |
| FR-08 | Dashboard must show sales charts and KPI cards | `Dashboard.xaml` — 4 KPI cards + 3 charts |
| FR-09 | Keyboard shortcuts must enable fast operation | F1–F7 global hotkeys in `MainWindow.xaml.cs` |
| FR-10 | Reports must be printable | Crystal Reports subproject + `Process.Start()` |

### Non-Functional Requirements Implemented

| ID | Requirement | Implementation |
|----|-------------|----------------|
| NF-01 | Performance: UI must never freeze | Async `Task.Run()` in DL queries and alert service |
| NF-02 | Usability: Keyboard-navigable interface | Tab order + F-key hotkeys throughout |
| NF-03 | Reliability: Data never lost on crash | SQL transactions in stored procedures |
| NF-04 | Security: Role-based access | `Admin` and `Cashier` classes with route guards |
| NF-05 | Maintainability: Layered architecture | Strict 3-tier BL/DL/UI separation |

---

## Week 3 — Software Architecture (3-Tier Architecture)

### Concepts Covered
Layered architecture, separation of concerns, tier responsibilities, coupling, cohesion.

### 3-Tier Architecture Implemented

```
┌─────────────────────────────────────────────────────┐
│  PRESENTATION TIER (UI/)                             │
│  Dashboard, ProcessOrder, ProductForm, Settings...   │
│  Pure XAML + code-behind. No SQL. No business logic. │
├─────────────────────────────────────────────────────┤
│  BUSINESS LOGIC TIER (BL/)                           │
│  Product, Order, Employee, Supplier, Stock, Alert... │
│  Validation, calculations, domain rules.             │
├─────────────────────────────────────────────────────┤
│  DATA ACCESS TIER (DL/)                              │
│  ProductDL, OrderDL, SupplierDL, NotificationDL...  │
│  All SQL, stored procedures, DataHandler.            │
└─────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│  DATABASE (SQL Server — G2DB)                        │
│  Tables: Product, Order, OrderDetail, PriceLog,      │
│  SupplierStock, Employee, Notification...             │
└─────────────────────────────────────────────────────┘
```

### Key Examples

**UI Layer (no SQL):**
```csharp
// Dashboard.xaml.cs — just calls BL/DL, never writes SQL
var (revenue, profit, orders) = ProductDL.GetTodayKpis();
kpiRevenue.Text = $"Rs. {revenue:N0}";
```

**BL Layer (business rules only):**
```csharp
// Order.cs — calculates totals purely from domain objects
public double GrandTotal => Products.Sum(x => x.TotalPrice);
public double TotalPrice => Quantity * (UnitPrice - Discount);
```

**DL Layer (all SQL here):**
```csharp
// ProductDL.cs — all data access
public static (decimal revenue, decimal profit, int orders) GetTodayKpis()
{
    var reader = Utils.ReadData(@"SELECT SUM(OD.Price*OD.Quantity) ... FROM [Order] O ...");
    ...
}
```

---

## Week 4 — User Interface Design & Usability

### Concepts Covered
HCI principles, responsive design, feedback, error prevention, learnability, efficiency.

### UI Implementations

**Feedback Mechanisms:**
- Grand total updates live as products are added — `RefreshData()` called after every `AddProduct()`.
- Camera preview in checkout footer shows live feed so cashier knows scanner is active.
- IP Webcam frame shown in checkout so tester can confirm phone is connected.
- Alert badge (red dot) on notification bell shows unread alert count.

**Error Prevention:**
- Confirm button checks: products in cart, cash received ≥ total before saving.
- Required fields marked with `IsRequired=True` in `TextEntry` controls.
- Barcode cooldown (40 ticks) prevents duplicate scan registrations.

**Efficiency:**
- Global hotkeys: F1=Dashboard, F2=Order, F3=Products, F4=Suppliers, F5=Employees, F6=Notifications, F7=Settings.
- Tab order: Product Code → Qty → Customer → Cash → Confirm (zero mouse needed).
- Ctrl+Enter confirms order. Delete removes selected row from grid.
- Del key removes selected item from checkout list.

**Bright, Modern Color Scheme:**
- Primary: `#1565C0` (royal blue) — headers, action buttons.
- Success: `#059669` (emerald) — confirm, profit KPI.
- Warning: `#F59E0B` (amber) — expiry alerts.
- Danger: `#DC2626` (red) — critical alerts, low stock.
- Background: `#F0F4F8` (light blue-grey) — easy on eyes for long shifts.

---

## Week 5 — Process Models & Software Development Life Cycle

### Concepts Covered
Waterfall, Agile, iterative development, incremental delivery, prototyping.

### SDLC Applied to This Project

**Phase 1 — Requirements (Week 2 artifacts):**  
Gathered from shop owner interviews. Output: functional/non-functional requirements table.

**Phase 2 — Design (Week 3 artifacts):**  
3-tier architecture diagram, DB schema (G2DB.sql), UI wireframes.

**Phase 3 — Implementation:**  
Incremental delivery:
- Sprint 1: Core DB schema, basic CRUD for products/suppliers.
- Sprint 2: Order processing, barcode scanning, receipt printing.
- Sprint 3: Dashboard charts, role-based access, reports.
- Sprint 4: AI alerts, expiry management, IP webcam mode, keyboard shortcuts.

**Phase 4 — Testing:**  
Unit logic tested in BL layer. Integration tested via SQL Server directly. UI tested manually. Barcode tested with IP Webcam app on Android.

**Phase 5 — Maintenance:**  
Migration scripts (`Migration_ExpiryAndAlerts.sql`) allow schema updates without data loss.

---

## Week 6 — Software Validation & Verification

### Concepts Covered
Testing types, unit testing, integration testing, acceptance testing, validation vs verification.

### Testing Strategy

**Verification (building the product right):**
- 3-tier separation enforced — DL layer is the only place with SQL strings.
- `DataHandler.cs` centralizes all DB operations — single point to test/mock.
- `AlertService` logic unit-testable: `GetLowStockProducts()` and `GetExpiringProducts()` return plain DTOs.

**Validation (building the right product):**
- Barcode scanning validated: USB scanner tested at counter; mobile IP Webcam tested for dev/QA.
- Expiry alerts validated: Setting `ExpiryAlertDays=1` triggers alerts for products expiring tomorrow.
- KPI cards validated: Match manually summed receipts.

**Test Scenarios:**

| Scenario | Expected | Method |
|----------|----------|--------|
| Scan barcode of known product | Product added to cart | USB scanner / IP Webcam |
| Enter unknown barcode | Nothing added, no crash | Manual test |
| Stock drops to reorder level | Alert appears in panel | Set threshold = current stock |
| Product expires in ≤30 days | Expiry alert fires | Set ExpiryDate = today+29 |
| Admin clicks "Apply Discount" | PriceLog updated -15% | Check DB after click |
| Cash entered < total | Error shown, no save | Manual test |
| F2 pressed | Process Order opens | Keyboard test |

---

## Week 7 — System Design Patterns

### Concepts Covered
Singleton, Factory, Observer, MVC/MVVM, Repository pattern.

### Patterns Used

**Singleton — `Configuration.cs`:**
```csharp
private static Configuration _instance;
public static Configuration getInstance()
{
    if (_instance == null) _instance = new Configuration();
    return _instance;
}
```
Ensures exactly one SQL connection instance throughout the application lifetime.

**Observer — `AlertService.cs`:**
```csharp
public static event EventHandler<List<AlertItem>>? AlertsUpdated;
// Dashboard subscribes:
AlertService.AlertsUpdated += OnAlertsUpdated;
// AlertService fires:
AlertsUpdated?.Invoke(null, _alerts);
```
Dashboard auto-refreshes whenever alerts change — decoupled from the alert logic.

**Repository Pattern — DL Layer:**
Each entity has its own DL class (`ProductDL`, `SupplierDL`, `OrderDL`, etc.) acting as a repository that encapsulates all data access for that entity.

**Data Transfer Objects (DTOs):**
`LowStockDto` and `ExpiringDto` in `AlertService.cs` — plain data containers for passing query results between DL and service layer without coupling to BL entities.

**Factory via `DataHandler.ConstructObjects()`:**
Dynamically creates any BL object from a DB reader using reflection and `Activator.CreateInstance()`.

---

## Week 8 — Software Configuration & Change Management

### Concepts Covered
Version control, configuration management, change requests, baseline management.

### Configuration Management in This Project

**Settings Persistence (`GlobalSettings.cs`):**  
All user preferences saved to `Settings.xml` at runtime:
- Theme (light/dark)
- Camera name (for physical scanner)
- IP Webcam URL (for mobile testing)
- Admin email (for alerts)
- Expiry alert days threshold

**Database Migration Strategy:**  
Schema changes never modify G2DB.sql. Instead, incremental migration scripts:
- `Migration_RemoveCustomerFromOrder.sql` — v1 change
- `Migration_ExpiryAndAlerts.sql` — v2 change (ExpiryDate column, updated stored procs)

**Barcode File Management:**  
`barcodes/` folder auto-generated. `GenerateBarcodes()` is idempotent — safe to run multiple times, only creates missing files.

**Stored Procedures as API contract:**  
The application never writes `INSERT INTO Product...` directly. It always calls `stpInsertProduct`. This means the DB schema can change (add columns) without changing application code — only the SP needs updating.

---

## Week 9 — Software Quality & Maintenance

### Concepts Covered
Software quality attributes, maintainability, reliability, portability, efficiency, code quality.

### Quality Attributes Implemented

**Maintainability:**
- `DataHandler.cs` — single centralized class for all DB operations. Change DB library once, fixes everywhere.
- `GlobalSettings.cs` — all tunable parameters in one file, saved to XML automatically.
- Business rules in BL, never duplicated in UI.

**Reliability:**
- Barcode cooldown prevents double-scanning.
- `AlertService` wrapped in try/catch — a DB failure doesn't crash the UI.
- Null checks throughout DL layer prevent null reference exceptions on empty tables.

**Performance (Efficiency):**
- `AlertService` runs every 5 minutes on background thread — never blocks UI.
- IP Webcam polls every 400ms with a 2-second HTTP timeout — never hangs UI thread.
- `DataHandler.FillDataTable()` uses `SqlDataAdapter` for bulk grid loads.

**Portability:**
- IP Webcam URL configurable at runtime — works on any local network.
- Printer name configurable — works with any installed Windows printer.
- SQL connection string in `Configuration.cs` — change server name once.

**Testability:**
- DL methods return plain objects/DataTables — easy to verify in unit tests.
- `AlertService.CheckAfterSaleAsync()` is public — can be triggered manually in tests.

---

## Appendix A — Keyboard Shortcuts Reference

| Key | Action |
|-----|--------|
| F1 | Go to Dashboard |
| F2 | Go to Process Order |
| F3 | Manage Products (Admin) |
| F4 | Manage Suppliers (Admin) |
| F5 | Manage Employees (Admin) |
| F6 | Manage Notifications (Admin) |
| F7 | Settings |
| Escape | Return to Dashboard |
| Ctrl+Enter | Confirm Order (in Process Order) |
| Delete | Remove selected row (in Order grid) |
| Enter | Move to next field (in Order entry) |

## Appendix B — Barcode Format

Barcodes in this system are **8-character** codes:
- Characters 1–5: Product Code (e.g., `PEN01`)
- Characters 6–8: Supplier Code (e.g., `SUP`)
- Full barcode: `PEN01SUP`

This allows a single scan to identify both the product and which supplier's stock to deduct from.

---
*End of SE Documentation*
