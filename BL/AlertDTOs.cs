using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationeryStoreManagementSystem.BL
{
    public class LowStockDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int ReorderThreshold { get; set; }
        public int CurrentStock { get; set; }
    }

    public class ExpiringDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
    }
}