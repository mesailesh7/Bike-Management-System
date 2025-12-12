using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.ViewModels
{
    public class CategoryViewModel
    {
        public int CategoryID { get; set; }
        public string Description { get; set; } = string.Empty;
        public int ItemCount { get; set; }
    }
}
