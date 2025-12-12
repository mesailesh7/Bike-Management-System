using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.ViewModels
{
    public class PartViewModel
    {
       
        public string PartName { get; set; } = string.Empty;
        public int QOH { get; set; }
        public int CategoryID { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal SellingPrice { get; set; }
        public int PartID { get; set; }

    }
}

