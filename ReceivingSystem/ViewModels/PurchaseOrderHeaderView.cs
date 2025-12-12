using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingSystem.ViewModels
{
    public class PurchaseOrderHeaderView
    {
        public int PurchaseOrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string Vendor { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }



}
