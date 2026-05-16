using StationeryStoreManagementSystem.BL;
using StationeryStoreManagementSystem.DL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace StationeryStoreManagementSystem.Services
{
    public class AlertItem
    {
        public enum AlertType { LowStock, Expiring }
        public AlertType Type { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductCode { get; set; } = "";
        public int ProductId { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderThreshold { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public bool DiscountApplied { get; set; }
        public bool IsAcknowledged { get; set; }

        public string Title => Type == AlertType.LowStock
            ? $"Low Stock: {ProductName}"
            : $"Expiring Soon: {ProductName}";

        public string Message => Type == AlertType.LowStock
            ? $"Only {CurrentStock} units left (reorder at {ReorderThreshold}). Please restock."
            : $"Expires on {ExpiryDate?.ToString("dd MMM yyyy")} — {DaysUntilExpiry} days left. Consider applying a discount.";

        public string Severity => Type == AlertType.LowStock
            ? (CurrentStock == 0 ? "Critical" : "Warning")
            : (DaysUntilExpiry <= 7 ? "Critical" : "Warning");
    }

    public static class AlertService
    {
        private static DispatcherTimer _timer = new DispatcherTimer();
        private static List<AlertItem> _alerts = new List<AlertItem>();
        public static event EventHandler<List<AlertItem>>? AlertsUpdated;

        public static IReadOnlyList<AlertItem> CurrentAlerts => _alerts.AsReadOnly();
        public static int UnreadCount => _alerts.Count(a => !a.IsAcknowledged);

        public static void Start()
        {
            //Application start hote hi 30 din purane notifications ko DB se permanently delete kar do
            try
            {
                Utils.ExecuteQuery("DELETE FROM [dbo].[Notification] WHERE DATEDIFF(day, AddedOn, GETDATE()) > 30");
            }
            catch { } 
            
            // Agar koi DB issue ho tou app crash na ho

            _timer.Interval = TimeSpan.FromMinutes(5);
            _timer.Tick += async (s, e) => await CheckAlertsAsync();
            _timer.Start();

            // Run immediately on startup
            Task.Run(async () => await CheckAlertsAsync());
        }

        public static void Stop() => _timer.Stop();

        
        public static async Task CheckAfterSaleAsync() => await CheckAlertsAsync();

        private static async Task CheckAlertsAsync()
        {
            try
            {
                var newAlerts = new List<AlertItem>();

                // Check low stock
                var lowStockItems = await Task.Run(() => ProductDL.GetLowStockProducts());
                foreach (var item in lowStockItems)
                {
                    // Keep acknowledged state from previous alerts
                    var existing = _alerts.FirstOrDefault(a => a.Type == AlertItem.AlertType.LowStock && a.ProductId == item.ProductId);
                    newAlerts.Add(new AlertItem
                    {
                        Type = AlertItem.AlertType.LowStock,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductCode = item.ProductCode,
                        CurrentStock = item.CurrentStock,
                        ReorderThreshold = item.ReorderThreshold,
                        IsAcknowledged = existing?.IsAcknowledged ?? false
                    });
                }

                // Check expiring products
                var expiringItems = await Task.Run(() => ProductDL.GetExpiringProducts(GlobalSettings.ExpiryAlertDays));
                foreach (var item in expiringItems)
                {
                    var existing = _alerts.FirstOrDefault(a => a.Type == AlertItem.AlertType.Expiring && a.ProductId == item.ProductId);
                    newAlerts.Add(new AlertItem
                    {
                        Type = AlertItem.AlertType.Expiring,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductCode = item.ProductCode,
                        ExpiryDate = item.ExpiryDate,
                        DaysUntilExpiry = item.DaysUntilExpiry,
                        DiscountApplied = existing?.DiscountApplied ?? false,
                        IsAcknowledged = existing?.IsAcknowledged ?? false
                    });
                }

                // Find NEW alerts (not in previous list) and send emails
                var newCritical = newAlerts.Where(a =>
                    !_alerts.Any(old => old.Type == a.Type && old.ProductId == a.ProductId)).ToList();

                _alerts = newAlerts;

                if (newCritical.Count > 0)
                    await SendEmailAlertAsync(newCritical);

                // Notify UI
                Application.Current?.Dispatcher.Invoke(() =>
                    AlertsUpdated?.Invoke(null, _alerts));
            }
            catch { }
        }

        public static void AcknowledgeAlert(int productId, AlertItem.AlertType type)
        {
            var alert = _alerts.FirstOrDefault(a => a.ProductId == productId && a.Type == type);
            if (alert != null)
            {
                alert.IsAcknowledged = true;

                
                SaveAlertToDatabase(alert, "Acknowledged");
            };
            AlertsUpdated?.Invoke(null, _alerts);
        }

        public static async Task ApplyExpiryDiscountAsync(int productId, double discountPercent)
        {
            await Task.Run(() => ProductDL.ApplyExpiryDiscount(productId, discountPercent));
            var alert = _alerts.FirstOrDefault(a => a.ProductId == productId && a.Type == AlertItem.AlertType.Expiring);
            if (alert != null)
            {
                alert.DiscountApplied = true;
                alert.IsAcknowledged = true;

                // NYI LINE: Discount apply hone ka record bhi Bell Icon ke liye save kar do
                SaveAlertToDatabase(alert, $"Applied {discountPercent}% Discount");
            }
            AlertsUpdated?.Invoke(null, _alerts);
        }

        private static async Task SendEmailAlertAsync(List<AlertItem> alerts)
        {
            if (string.IsNullOrEmpty(GlobalSettings.AdminEmail)) return;
            try
            {
                await Task.Run(() =>
                {
                    var body = "<h2>POS System Alerts</h2><ul>";
                    foreach (var a in alerts)
                        body += $"<li><b>[{a.Severity}] {a.Title}</b> — {a.Message}</li>";
                    body += "</ul><p>Please login to the POS system to take action.</p>";

                    // Uses system default SMTP - configure in app for real deployment
                    // For testing, alerts show in-app only if SMTP not configured
                    var msg = new MailMessage("pos@shop.local", GlobalSettings.AdminEmail)
                    {
                        Subject = $"[POS Alert] {alerts.Count} new alert(s) require attention",
                        Body = body,
                        IsBodyHtml = true
                    };
                    // Attempt send — silently fails if SMTP not configured (in-app alerts still work)
                    try
                    {
                        using var smtp = new SmtpClient("localhost", 25) { EnableSsl = false };
                        smtp.Send(msg);
                    }
                    catch { }
                });
            }
            catch { }
        }


        // Yeh method alert ko SQL Database ki Notification table mein save karega
        private static void SaveAlertToDatabase(AlertItem alert, string extraInfo = "")
        {
            try
            {
               
                int currentUserId = Utils.CurrentEmployee != null ? Utils.CurrentEmployee.Id : 101;

                string info = string.IsNullOrEmpty(extraInfo) ? "" : $" ({extraInfo})";
                string content = $"[{alert.Title}] {alert.Message}{info}";

                
                content = content.Replace("'", "''");

               
                string query = $@"
            INSERT INTO [dbo].[Notification] ([UserId], [Content], [AddedOn], [AddedBy], [ViewedAt])
            VALUES ({currentUserId}, '{content}', GETDATE(), {currentUserId}, GETDATE())";

                Utils.ExecuteQuery(query);
            }
            catch { }
        }
    }

    // DTOs for DL queries
    public class LowStockDto { public int ProductId; public string ProductName = ""; public string ProductCode = ""; public int CurrentStock; public int ReorderThreshold; }
    public class ExpiringDto { public int ProductId; public string ProductName = ""; public string ProductCode = ""; public DateTime? ExpiryDate; public int DaysUntilExpiry; }
}
