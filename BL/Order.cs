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
                    if (Product?.Category == null || Product.Category.GST == null)
                        return 0;
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
            productsLookup = new Dictionary<string, Product>();
            List<Product> listProducts = ProductDL.GetProducts();
            foreach(Product product in listProducts)
            {
                product.Suppliers = SupplierDL.GetProductSuppliers(product.Id);
                product.Stocks = ProductDL.GetProductStocks(product);
                productsLookup[product.Code] = product;
            }
        }
        public void AddProduct(string productID, int quantity)
        {
            productID = productID.Trim();

            // ── PATH 1: Check ExternalBarcode FIRST (highest priority) ──
            // Scan kiya hua barcode pehle ExternalBarcode se match karo (in-memory)
            Product? extProduct = productsLookup.Values
                .FirstOrDefault(p => !string.IsNullOrEmpty(p.ExternalBarcode)
                                  && string.Equals(p.ExternalBarcode.Trim(), productID, StringComparison.OrdinalIgnoreCase));

            // Agar in-memory mein nahi mila toh DB se try karo
            if (extProduct == null)
            {
                extProduct = DL.ProductDL.GetProductByExternalBarcode(productID);
                // DB se aaye product ko lookup mein bhi add karo future use ke liye
                if (extProduct != null && !productsLookup.ContainsKey(extProduct.Code))
                    productsLookup[extProduct.Code] = extProduct;
            }

            if (extProduct != null)
            {
                // Agar stocks null/empty hain toh dobara load karo DB se
                if (extProduct.Stocks == null || extProduct.Stocks.Count == 0)
                {
                    if (extProduct.Suppliers == null || extProduct.Suppliers.Count == 0)
                        extProduct.Suppliers = SupplierDL.GetProductSuppliers(extProduct.Id);
                    extProduct.Stocks = ProductDL.GetProductStocks(extProduct);
                }

                // Pehla valid stock wala supplier use karo
                Stock? stock = extProduct.Stocks?.FirstOrDefault(s => s.Supplier != null);
                if (stock != null)
                {
                    string orderCode = extProduct.ExternalBarcode; // cart key = original barcode
                    OrderProduct? orderProduct = Products.Find(x => x.Code == orderCode);
                    if (orderProduct == null)
                        Products.Add(new OrderProduct(orderCode, extProduct, stock.Supplier, quantity, stock.RetailPrice, stock.DiscountAmount));
                    else
                        orderProduct.Quantity += quantity;
                    return; // handled
                }
            }

            // ── PATH 2: System-generated barcode (ProductCode 5 + SupplierCode 3 = exactly 8 chars) ──
            // Sirf tab yahan aao jab ExternalBarcode match na hua ho
            if (productID.Length == 8)
            {
                string productCode = productID.Substring(0, 5);
                string supplierCode = productID.Substring(5, 3);
                if (productsLookup.ContainsKey(productCode))
                {
                    Product product = productsLookup[productCode];
                    Stock? stock = product.Stocks?.FirstOrDefault(x => x.Supplier != null && x.Supplier.Code == supplierCode);
                    if (stock != null)
                    {
                        OrderProduct? orderProduct = Products.Find(x => x.Code == productID);
                        if (orderProduct == null)
                            Products.Add(new OrderProduct(productID, product, stock.Supplier, quantity, stock.RetailPrice, stock.DiscountAmount));
                        else
                            orderProduct.Quantity += quantity;
                    }
                }
                return; // handled
            }

            // ── PATH 3: Direct product Code match (5-char or custom code manual entry) ──
            if (productsLookup.ContainsKey(productID))
            {
                Product product = productsLookup[productID];
                if (product.Stocks == null || product.Stocks.Count == 0)
                {
                    if (product.Suppliers == null || product.Suppliers.Count == 0)
                        product.Suppliers = SupplierDL.GetProductSuppliers(product.Id);
                    product.Stocks = ProductDL.GetProductStocks(product);
                }
                Stock? stock = product.Stocks?.FirstOrDefault(s => s.Supplier != null);
                if (stock != null)
                {
                    string orderCode = productID;
                    OrderProduct? orderProduct = Products.Find(x => x.Code == orderCode);
                    if (orderProduct == null)
                        Products.Add(new OrderProduct(orderCode, product, stock.Supplier, quantity, stock.RetailPrice, stock.DiscountAmount));
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
