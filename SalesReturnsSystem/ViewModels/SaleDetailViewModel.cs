using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.ViewModels
{
    public class SaleDetailViewModel
    {
        public int PartID { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int QOH { get; set; }
        public decimal Price { get; set; }
        public decimal ExtPrice => Price * Quantity;
    }
}

