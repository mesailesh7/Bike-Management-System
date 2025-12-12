using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.ViewModels
{
    public class ReturnViewModel
    {
        public int RefundID { get; set; }
        public DateTime SaleRefundDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
    }

}
