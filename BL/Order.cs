using StationeryStoreManagementSystem.DL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationeryStoreManagementSystem.BL
{
    public class Order
    {
        public class OrderProduct
            {
            public string Code { get; set; }
            public Product Product { get; set; }
            public Supplier Supplier { get; set; }
            private int quantity;
            public int Quantity { 
                get
                {
                    return quantity;
                }
                set
                {
                    quantity = value;
                }
                }
            public double UnitPrice { get; set; }
            public double Discount { get; set; }
            public double Tax
            {
                get
                {
                    return (double)((Product.Category.GST * UnitPrice)/100);
                }
            }
            public double TotalPrice
            {
                get
                {
                    return Quantity*(UnitPrice-Discount);
                }
            }

            public OrderProduct(string code, Product product, Supplier supplier, int quantity, double unitPrice,double discount)
            {
                Code = code;
                Product = product;
                Supplier = supplier;
                Quantity = quantity;
                UnitPrice = unitPrice;
                Discount = discount;
            }
        }
        public Customer? Customer { get; set; }
        public List<OrderProduct> Products;
        private Dictionary<string, Product> productsLookup;
        public double GrandTotal
        {
            get
            {
                return Products.Sum(x => x.TotalPrice);
            }
        }
        public double SavedTotal
        {
            get
            {
                return Products.Sum(x => x.UnitPrice * x.Quantity) - GrandTotal;
            }
        }
        public double Received { get; set; }
        public Order()
        {
            Products = new List<OrderProduct>();
            productsLookup = new Dictionary<string, Product>();
            List<Product> listProducts = ProductDL.GetProducts();
            foreach(Product product  in listProducts)
            {
                product.Suppliers = SupplierDL.GetProductSuppliers(product.Id);
                product.Stocks = ProductDL.GetProductStocks(product);
                productsLookup.Add(product.Code, product);
            }
        }
        public void AddProduct(string productID,int quantity)
        {
            if (productID.Length == 8)
            {
                string productCode = productID.Substring(0, 5);
                string suppliercode = productID.Substring(5, 3);
                if (productsLookup.ContainsKey(productCode))
                {
                    Product product = productsLookup[productCode];
                    Stock? stock = product.Stocks.Where(x => x.Supplier.Code == suppliercode).FirstOrDefault();
                    if (productsLookup[productCode].Suppliers.Exists(x => x.Code == suppliercode))
                        if (stock != null)
                        {
                            OrderProduct? orderProduct = Products.Find(x => x.Code == productID);
                            if (orderProduct == null)
                                Products.Add(new OrderProduct(productID, product, stock.Supplier, quantity,stock.RetailPrice,stock.DiscountAmount));
                            else
                                orderProduct.Quantity += quantity;
                        }
                }
            }
        }
        public void RemoveProduct(string ProductID)
        {
            Products.Remove(Products.Where(x => x.Code == ProductID).First());
        }
        public void RemoveProduct(int index)
        {
            Products.RemoveAt(index);
        }
    }
}
