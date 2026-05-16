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
                    // Add a check to see if Product or Category is null
                    if (Product?.Category == null) return 0;

                    return (double)((Product.Category.GST * UnitPrice) / 100);
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
        public string CustomerName { get; set; }
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
            productsLookup = new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase);
            List<Product> listProducts = ProductDL.GetProducts();
            foreach(Product product  in listProducts)
            {
                product.Suppliers = SupplierDL.GetProductSuppliers(product.Id);
                product.Stocks = ProductDL.GetProductStocks(product);
                productsLookup.Add(product.Code, product);
            }
        }
        public void AddProduct(string productID, int quantity)
        {
            productID = productID.Trim();

            // ── PATH 1: System-generated barcode (8 chars) ──
            if (productID.Length == 8)
            {
                string productCode = productID.Substring(0, 5);
                string supplierCode = productID.Substring(5, 3);
                if (productsLookup.ContainsKey(productCode))
                {
                    Product product = productsLookup[productCode];
                    Stock? stock = product.Stocks.Where(x => x.Supplier.Code == supplierCode).FirstOrDefault();
                    if (product.Suppliers.Exists(x => x.Code == supplierCode) && stock != null)
                    {
                        OrderProduct? orderProduct = Products.Find(x => x.Code == productID);
                        if (orderProduct == null)
                            Products.Add(new OrderProduct(productID, product, stock.Supplier, quantity, stock.RetailPrice, stock.DiscountAmount));
                        else
                            orderProduct.Quantity += quantity;
                    }
                }
                return;
            }

            // ── PATH 2: External barcode (NOT 8 chars) ──
            Product? extProduct = productsLookup.Values
                .FirstOrDefault(p => !string.IsNullOrEmpty(p.ExternalBarcode)
                                  && p.ExternalBarcode.Trim() == productID);

            if (extProduct == null)
            {
                extProduct = DL.ProductDL.GetProductByExternalBarcode(productID);
            }

            // ── NEW PATH 3: Internal Code Fallback (Handles bp001, etc.) ──
            if (extProduct == null && productsLookup.ContainsKey(productID))
            {
                extProduct = productsLookup[productID];
            }

            // ── FINAL ADDITION LOGIC ──
            if (extProduct != null && extProduct.Stocks != null && extProduct.Stocks.Count > 0)
            {
                // Use the first available supplier's stock
                Stock stock = extProduct.Stocks[0];
                if (stock.Supplier != null)
                {
                    // Use the entered productID as the key in the cart
                    OrderProduct? orderProduct = Products.Find(x => x.Code == productID);
                    if (orderProduct == null)
                        Products.Add(new OrderProduct(productID, extProduct, stock.Supplier, quantity, stock.RetailPrice, stock.DiscountAmount));
                    else
                        orderProduct.Quantity += quantity;
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
