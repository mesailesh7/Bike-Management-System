using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.ViewModels
{
    public class PurchaseOrderDetailView
    {
        public int PurchaseOrderDetailID { get; set; }
        public int PurchaseOrderID { get; set; }
        public int PartID { get; set; }
        public string Description { get; set; } = string.Empty;
        public int ROL { get; set; }
        public int QOH { get; set; }
        public int QOO { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public string VendorPartNumber { get; set; } = string.Empty;
        public bool RemoveFromViewFlag { get; set; }
    }
}
