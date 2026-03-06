using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationeryStoreManagementSystem.BL
{
    public class Stock
    {
        public Supplier Supplier { get; set; }
        public double Price { get; set; }
        public double RetailPrice { get; set; }
        public double DiscountAmount { get; set; }
        public int Quantity { get; set; }

        public Stock(Supplier supplier, double price, double retailPrice, double discountAmount, int quantity)
        {
            Supplier = supplier;
            Price = price;
            RetailPrice = retailPrice;
            DiscountAmount = discountAmount;
            Quantity = quantity;
        }
        public Stock(Stock stock)
        {
            Supplier = stock.Supplier;
            Price = stock.Price;
            RetailPrice = stock.RetailPrice;
            DiscountAmount = stock.DiscountAmount;
            Quantity = stock.Quantity;
        }
        public override string ToString()
        {
            return $"{Supplier.Id} {Price} {RetailPrice} {DiscountAmount} {Quantity}";
        }
    }
}
