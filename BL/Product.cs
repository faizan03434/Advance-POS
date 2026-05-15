using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StationeryStoreManagementSystem.BL
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        // NEW: Physical barcode already printed on the product packaging
        public string ExternalBarcode { get; set; }

        public Company? Company { get; set; }
        public int? ReorderThreshold { get; set; }
        public Category? Category { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public List<Supplier> Suppliers { get; set; }
        public List<Stock> Stocks { get; set; }
        public List<object> InitialArgs { get; set; }

        public int SupplierQuantity
        {
            get { return Stocks != null && Stocks.Count > 0 ? Stocks[0].Quantity : 0; }
            set { if (Stocks != null && Stocks.Count > 0) Stocks[0].Quantity = value; }
        }

        public Product(int id) : this() { Id = id; }
        public Product() { }

        public Product(string name, string code, Company? company, int? reorderThreshold, Category? category,
                       DateTime? expiryDate, List<Supplier> suppliers, List<Stock> stocks,
                       string externalBarcode = null) : this()
        {
            Name = name; Code = code; Company = company; ReorderThreshold = reorderThreshold;
            Category = category; ExpiryDate = expiryDate; Suppliers = suppliers; Stocks = stocks;
            ExternalBarcode = externalBarcode;
        }

        // Constructor from DB args list: Id, Name, Code, ExternalBarcode, Company, ReorderThreshold, Category, ExpiryDate, Suppliers, Stocks
        public Product(List<object> args)
        {
            Id = (int)args[0];
            Name = (string)args[1];
            Code = ((string)args[2]).Trim();
            // args[3] = ExternalBarcode (new column, nullable)
            ExternalBarcode = args.Count > 3 && args[3] != null ? (string)args[3] : null;

            Company = args.Count > 4 && args[4] != null ? (Company?)args[4] : null;
            ReorderThreshold = args.Count > 5 && args[5] != null ? (int?)args[5] : null;
            Category = args.Count > 6 && args[6] != null ? (Category?)args[6] : null;
            ExpiryDate = args.Count > 7 && args[7] != null ? (DateTime?)args[7] : null;

            int suppIdx = args.Count > 8 && args[8] is List<Supplier> ? 8 : 7;
            int stksIdx = suppIdx + 1;

            Suppliers = args.Count > suppIdx && args[suppIdx] is List<Supplier> s ? s : new List<Supplier>();
            if (args.Count > stksIdx && args[stksIdx] is List<Stock> stocks)
                Stocks = stocks.Select(x => new Stock(x)).ToList();
            else
                Stocks = new List<Stock>();

            InitialArgs = new List<object>(args);
            InitialArgs.RemoveAt(0);
        }

        public void Save(bool isAdd = false)
        {
            ProductDL.Save(this, isAdd);

            // Only generate system barcodes for products WITHOUT an external barcode
            if (string.IsNullOrWhiteSpace(ExternalBarcode))
            {
                foreach (var supplier in SupplierDL.GetProductSuppliers(this))
                    Utils.GenerateBarcode(Code + supplier.Code);
            }
            else
            {
                // Save the external barcode image using the raw scanned value
                Utils.GenerateBarcode(ExternalBarcode);
            }
        }
    }
}
