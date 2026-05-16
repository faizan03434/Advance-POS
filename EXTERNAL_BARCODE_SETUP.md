# External Barcode Feature - Setup Instructions

## Overview
This feature allows you to scan and use existing barcodes (like the one on product packaging: 8961014780578) instead of generating random system barcodes. The barcode image is captured from the camera and saved for the product.

## Issues Fixed
1. ✅ Barcode scanning not working in "Add Existing Barcode"
2. ✅ Supplier data not appearing fully on the page
3. ✅ "isDiscontinued column is missing" error when adding products
4. ✅ Camera preview added (same as Order Process)
5. ✅ Barcode image capture and storage

## Setup Steps

### Step 1: Run Database Migration
You **MUST** run this SQL migration on your database before using the feature:

1. Open SQL Server Management Studio (SSMS)
2. Connect to your database server
3. Open the file: `Migration_ExternalBarcode_Fix.sql`
4. Execute the script

**What this migration does:**
- Adds `ExternalBarcode` column to Product table (if not exists)
- Updates `stpInsertProduct` stored procedure to accept ExternalBarcode parameter and explicitly set IsDiscontinued=0
- Updates `stpUpdateProduct` stored procedure to accept ExternalBarcode parameter

### Step 2: Verify Camera Configuration
The barcode scanning uses the same camera setup as the Order Process:

1. Go to **Settings** in your application
2. Ensure your camera is configured in `GlobalSettings.CameraName`
3. If not configured, select your camera from the available devices

### Step 3: Test the Feature

#### Adding a Product with Existing Barcode:
1. Go to **Manage Product**
2. Click **"Add Existing Barcode"** button
3. Click **"📷 Scan"** button
4. A camera window will open (same as Order Process)
5. Point the camera at the product barcode (e.g., 8961014780578)
6. The barcode will be automatically detected and scanned
7. The captured barcode image will be saved
8. Fill in the product details manually:
   - Product Name
   - Internal Code (5 characters)
   - Company (optional)
   - Category (optional)
   - Reorder Threshold (optional)
   - Expiry Date (optional)
9. Assign suppliers from the list
10. Click **"Save Product"**

#### What Happens:
- The external barcode value (e.g., 8961014780578) is stored in the database
- The captured barcode image is saved to `barcodes/8961014780578.png`
- The product is linked to this barcode
- When scanning during order processing, this barcode will be recognized

## How It Works

### Barcode Scanning Flow:
1. **Camera Opens**: Same camera component as Order Process (BarcodeScanOverlay)
2. **Real-time Scanning**: Camera continuously scans for barcodes
3. **Detection**: When a barcode is detected, it's captured
4. **Image Capture**: The frame containing the barcode is saved as a Bitmap
5. **Display**: Barcode value is shown in the text field
6. **Preview**: A barcode preview image is generated and displayed

### Barcode Storage:
- **Captured Image**: The actual camera frame with the barcode is saved to `barcodes/{barcode_value}.png`
- **Fallback**: If no camera image is captured (manual entry), a generated barcode image is created
- **Location**: All barcode images are stored in the `barcodes/` folder

### Database Changes:
- **ExternalBarcode Column**: Stores the scanned barcode value (up to 100 characters)
- **IsDiscontinued**: Explicitly set to 0 (false) when inserting new products
- **Nullable**: ExternalBarcode is optional - products can still use system-generated barcodes

## Manual Entry Option
If the camera doesn't work or you prefer manual entry:
1. Type the barcode value directly in the "Barcode Value" field
2. Click **"🔍 Lookup"** to search for product information online
3. Continue with filling product details

## Troubleshooting

### "isDiscontinued column is missing" Error
**Solution**: You didn't run the migration script. Run `Migration_ExternalBarcode_Fix.sql`

### Camera Not Opening
**Solution**: 
1. Check camera configuration in Settings
2. Ensure camera permissions are granted
3. Verify camera is not being used by another application

### Barcode Not Scanning
**Solution**:
1. Ensure good lighting
2. Hold the barcode steady and clear
3. Try different distances from the camera
4. Use manual entry as fallback

### Supplier Data Not Appearing
**Solution**: 
1. Ensure suppliers are added to the system first (Manage Suppliers)
2. Select suppliers from the list in the form
3. Click "Edit Price" to set supplier-specific pricing

### Barcode Image Not Saved
**Solution**:
1. Check that `barcodes/` folder exists (created automatically)
2. Verify write permissions on the application folder
3. Check for special characters in barcode value

## File Changes Made

### Modified Files:
1. **Migration_ExternalBarcode_Fix.sql** (NEW)
   - Database migration script

2. **UI/ExternalBarcodeForm.xaml.cs**
   - Added barcode image capture
   - Stores captured image from camera
   - Saves image before product save

3. **UI/BarcodeScanOverlay.xaml.cs**
   - Added `CapturedBarcodeImage` property
   - Stores the bitmap when barcode is detected
   - Returns image to calling form

4. **BL/Product.cs**
   - Updated Save method to check if barcode image exists
   - Prevents regenerating captured images

5. **DL/ProductDL.cs**
   - Already had ExternalBarcode support (no changes needed)

## Important Notes

### Barcode Format Support:
The system supports various barcode formats including:
- EAN-13 (like 8961014780578)
- EAN-8
- UPC-A
- CODE-128
- And many others supported by ZXing library

### Existing Products:
- Products without external barcodes continue to use system-generated barcodes (Code + SupplierCode)
- You can mix both types of products in the system

### Order Processing:
- When scanning during order processing, the system will:
  1. First try to match as external barcode
  2. If not found, try to match as system barcode (Code + SupplierCode)
  3. This ensures backward compatibility

## Testing Checklist

- [ ] Database migration executed successfully
- [ ] Camera opens when clicking "Scan" button
- [ ] Barcode is detected and scanned from camera
- [ ] Barcode value appears in the text field
- [ ] Product details can be filled manually
- [ ] Suppliers can be selected and assigned
- [ ] Product saves without "isDiscontinued" error
- [ ] Barcode image is saved in `barcodes/` folder
- [ ] Product can be found by scanning the same barcode in Order Process

## Support

If you encounter any issues:
1. Check that the migration script was run successfully
2. Verify camera configuration
3. Check application logs for errors
4. Ensure database connection is working
5. Verify file system permissions for `barcodes/` folder

---

**Feature Status**: ✅ Complete and Ready to Use

**Last Updated**: May 15, 2026
