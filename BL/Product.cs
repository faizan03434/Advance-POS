using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public List<Supplier> Suppliers { get; set; }
        public List<Stock> Stocks { get; set; }
        public List<object> InitialArgs { get; set; }
        public int SupplierQuantity
        {
            get
            {
                if (Stocks != null && Stocks.Count > 0)
                    return Stocks[0].Quantity;
                return 0;
            }
            set
            {
                if (Stocks != null && Stocks.Count > 0)
                    Stocks[0].Quantity = value;
            }
        }

        public Product(int id) : this()
        {
            Id = id;
        }
        public Product()
        {

        }
        public Product(string name,string code, Company? company, int? reorderThreshold, Category? category, List<Supplier> suppliers, List<Stock> stocks) : this()
        {
            Name = name;
            Code = code;
            Company = company;
            ReorderThreshold = reorderThreshold;
            Category = category;
            Suppliers = suppliers;
            Stocks = stocks;
        }
        public Product(List<object> args)
        {
            Id = (int)args[0];
            Name = (string)args[1];
            Code = (string)args[2];
            Company = (Company?)args[3];
            ReorderThreshold = (int?)args[4];
            Category = (Category?)args[5];
            Suppliers = ((List<Supplier>?)args[6]);
            if (args.Count > 7)
                Stocks = ((List<Stock>)args[7]).Select(x => new Stock(x)).ToList();
            else
                Stocks = new List<Stock>();
            InitialArgs = args;
            InitialArgs.RemoveAt(0);
        }
        public void Save(bool isAdd = false)
        {
            ProductDL.Save(this, isAdd);
            foreach(var supplier in SupplierDL.GetProductSuppliers(this))
            { 
            Utils.GenerateBarcode(Code+supplier.Code);
            
            }
        }
    }
}
