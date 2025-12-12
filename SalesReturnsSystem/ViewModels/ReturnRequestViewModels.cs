using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;

namespace SalesReturnsSystem.ViewModels
{
    public class ReturnRequestLineViewModel
    {
        public int PartId { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }

        public string Reason { get; set; } = string.Empty;

        public int QtyToReturn { get; set; }
    }

    public class ReturnRequestViewModel
    {
        public int SaleId { get; set; }
        public List<ReturnRequestLineViewModel> Items { get; set; } = new();
    }
}
