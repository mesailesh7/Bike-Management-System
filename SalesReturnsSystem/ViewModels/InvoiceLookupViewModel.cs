using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.ViewModels
{
    public class InvoiceLookupViewModel
    {
        public int InvoiceID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public decimal Total { get; set; }
    }
}

