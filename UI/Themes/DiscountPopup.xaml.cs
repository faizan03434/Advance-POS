using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;

namespace StationeryStoreManagementSystem.UI
{
    public partial class DiscountPopup : Window
    {
        private int _productId;
        private double _buyingPrice;
        private double _retailPrice;

        public bool IsSuccess { get; private set; } = false;
        public double NewDiscount { get; private set; }

        public DiscountPopup(int productId, string productName)
        {
            InitializeComponent();
            _productId = productId;
            ProductNameTxt.Text = $"Product: {productName}";
            LoadPricing();
        }

        private void LoadPricing()
        {
            // Database se active prices fetch kar rahay hain
            string query = $@"SELECT TOP 1 Price, RetailPrice, DiscountAmount 
                              FROM PriceLog 
                              WHERE ProductId = {_productId} 
                              ORDER BY AddedOn DESC";

            using (SqlDataReader reader = Utils.ReadData(query))
            {
                if (reader.Read())
                {
                    _buyingPrice = Convert.ToDouble(reader["Price"]);
                    _retailPrice = Convert.ToDouble(reader["RetailPrice"]);
                    double currentDiscount = Convert.ToDouble(reader["DiscountAmount"]);

                    BuyingPriceTxt.Text = $"Rs. {_buyingPrice:F2}";
                    CurrentRetailTxt.Text = $"Rs. {_retailPrice:F2}";
                    CurrentDiscountTxt.Text = $"Rs. {currentDiscount:F2}";

                    FinalPriceTxt.Text = $"Rs. {(_retailPrice - currentDiscount):F2}";
                }
            }
        }

        private void NewDiscountInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(NewDiscountInput.Text, out double newDiscount))
            {
                double finalPrice = _retailPrice - newDiscount;

                if (finalPrice < _buyingPrice)
                {
                    FinalPriceTxt.Text = $"Rs. {finalPrice:F2} (Loss!)";
                    FinalPriceTxt.Foreground = System.Windows.Media.Brushes.Red;
                }
                else
                {
                    FinalPriceTxt.Text = $"Rs. {finalPrice:F2}";
                    FinalPriceTxt.Foreground = System.Windows.Media.Brushes.Green;
                }
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(NewDiscountInput.Text, out double newDiscount))
            {
                // Database update: Naya discount amount active PriceLog mein set ho jayega
                string updateQuery = $@"
                    UPDATE PriceLog 
                    SET DiscountAmount = {newDiscount}
                    WHERE ProductId = {_productId} AND AddedOn = (SELECT MAX(AddedOn) FROM PriceLog WHERE ProductId = {_productId})";

                Utils.ExecuteQuery(updateQuery);
                NewDiscount = newDiscount;
                IsSuccess = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please enter a valid number for the discount.");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}