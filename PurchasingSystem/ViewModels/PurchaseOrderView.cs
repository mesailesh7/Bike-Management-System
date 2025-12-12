using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.ViewModels
{
    public class PurchaseOrderView
    {
        public int PurchaseOrderID { get; set; }
        public int PurchaseOrderNumber { get;set; }
        public DateTime? OrderDate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal SubTotal { get; set;  }
        public bool Closed { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string EmployeeID { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int VendorID { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public List<PurchaseOrderDetailView>? Details { get; set; }
        public bool RemoveFromViewFlag { get; set; }
    }
}
