# Advanced POS System — Stationery Store Management
## Version 2.0 Enhanced

A full-featured desktop Point-of-Sale application for stationery shops.  
Built with C# / WPF / .NET 6 / SQL Server.

---

## Quick Start

### Prerequisites
- Windows 10/11
- .NET 6 Desktop Runtime
- SQL Server 2019+ (or SQL Server Express — free)
- Visual Studio 2022 (to build from source)

### Database Setup
1. Open SQL Server Management Studio (SSMS)
2. Create a new database named **G2DB**
3. Run `G2DB.sql` to create all tables and views
4. Run `Migration_ExpiryAndAlerts.sql` to add expiry support
5. Run `TestData_SeedItems.sql` (optional — sample data)

### Build & Run
```
1. Open StationeryStoreManagementSystem.sln in Visual Studio 2022
2. Restore NuGet packages (right-click solution → Restore)
3. Set connection string in Configuration.cs if your SQL Server name differs
4. Press F5 to build and run
```

---

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| **F1** | Dashboard |
| **F2** | Process Order (checkout) |
| **F3** | Manage Products |
| **F4** | Manage Suppliers |
| **F5** | Manage Employees |
| **F6** | Notifications |
| **F7** | Settings |
| **Escape** | Back to Dashboard |
| **Ctrl+Enter** | Confirm order (in checkout) |
| **Delete** | Remove selected row from cart |
| **Enter** | Move to next field |

---

## Barcode Scanner — Two Modes

### Mode 1: Physical USB/Bluetooth Scanner (Shop Deployment)
When you deploy this system in the shop with a real barcode scanner machine:
1. Plug in your USB barcode scanner (it acts as a keyboard — HID device)
2. Click on the **Product Code** field in Process Order
3. Scan the barcode — it auto-fills the field
4. Press Enter → Qty field → Enter → product added
5. For camera-based scanning: go to **Settings → Select Camera** and choose your webcam/USB cam

### Mode 2: Mobile Phone as Barcode Scanner (Testing)

**This mode lets you use your Android phone's camera as a barcode scanner over Wi-Fi — perfect for testing without buying hardware.**

#### Step-by-Step Setup

**On your Android phone:**
1. Install **"IP Webcam"** from Google Play Store  
   (Search: "IP Webcam by Pavel Khlebovich" — free, no ads)
2. Open the app
3. Scroll down and tap **"Start server"**
4. The app will show an IP address and port, e.g.:  
   `http://192.168.1.105:8080`
5. Keep the app running and screen on

**On your PC (POS system):**
1. Make sure PC and phone are on the **same Wi-Fi network**
2. Open the POS system → go to **Settings (F7)**
3. Under "Barcode Scanner Mode", select **"Mobile Phone as Barcode Scanner"**
4. Enter the URL shown on your phone, e.g.:  
   `http://192.168.1.105:8080`
5. Click **F2** to go to Process Order
6. You'll see your phone's camera feed in the bottom-left corner
7. Hold a barcode in front of the phone camera — it scans automatically!

**Troubleshooting Mobile Scanner:**
- If feed doesn't appear: check both devices are on same Wi-Fi
- If scan doesn't trigger: ensure good lighting, hold barcode steady for 1 second
- URL must include `http://` prefix
- Make sure IP Webcam app is showing "Server started" status
- Some phones need "Focus mode" set to "Continuous" in IP Webcam settings

#### How to Test Barcodes
Barcodes follow this format: `PPPPPSVR` (5-char product code + 3-char supplier code)

Example: If product code is `PEN01` and supplier code is `ALF`, the barcode is `PEN01ALF`

You can:
- Print a barcode from the `barcodes/` folder (generated automatically)
- Use any online barcode generator (Code 128 format) with the 8-character code
- Use a barcode generator app on your phone to display a test barcode on another phone

---

## AI Automation Features

### Low Stock Alerts
- System checks stock levels every 5 minutes and after every sale
- If any product stock ≤ its Reorder Threshold → alert appears on Dashboard
- Admin can set an email in Settings to also receive email notifications

### Expiry Alerts
- Set ExpiryDate when adding/editing a product
- System alerts admin when a product will expire within 30 days (configurable)
- Admin can click **"Apply 15% Discount"** directly from the alert — discount is applied instantly

### In-App Alert Panel
- Dashboard shows all active alerts with color coding (Warning = yellow, Critical = red)
- Acknowledge button marks alerts as read
- Refresh button manually triggers a check

---

## Reports
Reports require the **CrystalReportApp** subproject to be built:
1. Build `Subprojects/CrystalReportApp/` in Visual Studio
2. Copy `CrystalReportApp.exe` to the same folder as the main app
3. From Dashboard (Admin only), select a report type and click "Preview Report"

Available reports:
- Today Sales Report
- Weekly Sales Report  
- Monthly Sales Report
- Employees Sales Report
- Expiring Products Report (new)

---

## Project Structure
```
StationeryStoreManagementSystem/
├── BL/                  # Business Logic (domain objects)
│   ├── Product.cs
│   ├── Order.cs
│   ├── Employee.cs
│   └── ...
├── DL/                  # Data Layer (all SQL here)
│   ├── ProductDL.cs
│   ├── OrderDL.cs
│   ├── DataHandler.cs
│   └── ...
├── Services/            # Background services
│   └── AlertService.cs  # Low stock + expiry monitoring
├── UI/                  # WPF views
│   ├── Dashboard.xaml
│   ├── ProcessOrder.xaml
│   ├── Settings.xaml
│   └── ...
├── G2DB.sql             # Full database schema
├── Migration_ExpiryAndAlerts.sql  # Run after G2DB.sql
├── GlobalSettings.cs    # App settings (saved to Settings.xml)
├── Configuration.cs     # DB connection singleton
└── README.md
```

---

## SE Topics Coverage
See `docs/SE_Documentation.md` for full mapping of all 9 SE weekly topics to this project.

---

## License
Academic project — for educational use.
