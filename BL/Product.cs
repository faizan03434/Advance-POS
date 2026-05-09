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
                       DateTime? expiryDate, List<Supplier> suppliers, List<Stock> stocks) : this()
        {
            Name = name; Code = code; Company = company; ReorderThreshold = reorderThreshold;
            Category = category; ExpiryDate = expiryDate; Suppliers = suppliers; Stocks = stocks;
        }

        // Constructor from DB args list: Id, Name, Code, Company, ReorderThreshold, Category, ExpiryDate, Suppliers, Stocks
        public Product(List<object> args)
        {
            Id = (int)args[0];
            Name = (string)args[1];
            Code = ((string)args[2]).Trim();
            Company = (Company?)args[3];
            ReorderThreshold = (int?)args[4];
            Category = (Category?)args[5];
            ExpiryDate = args.Count > 6 && args[6] != null ? (DateTime?)args[6] : null;

            int suppIdx = args.Count > 7 && args[7] is List<Supplier> ? 7 : 6;
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
            foreach (var supplier in SupplierDL.GetProductSuppliers(this))
                Utils.GenerateBarcode(Code + supplier.Code);
        }
    }
}
