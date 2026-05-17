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

        public double FinalDiscountPercent { get; private set; } // Nayi property percent ke liye
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
            if (double.TryParse(NewDiscountInput.Text, out double percent))
            {
                // User ne Percentage daali exact Rs (amount) nikal liya
                double discountAmount = _retailPrice * (percent / 100.0);
                double finalPrice = _retailPrice - discountAmount;

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
            if (double.TryParse(NewDiscountInput.Text, out double percent))
            {
                double discountAmount = _retailPrice * (percent / 100.0);

               
                
                string updateQuery = $@"
            UPDATE PriceLog SET DiscountAmount = {discountAmount} 
            WHERE ProductId = {_productId} AND AddedOn = (SELECT MAX(AddedOn) FROM PriceLog WHERE ProductId = {_productId});
            
            UPDATE Product SET DefaultDiscountPercent = {percent} 
            WHERE Id = {_productId};";

                Utils.ExecuteQuery(updateQuery);

                FinalDiscountPercent = percent; // Percent save kar liya
                IsSuccess = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please enter a valid percentage number.");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}