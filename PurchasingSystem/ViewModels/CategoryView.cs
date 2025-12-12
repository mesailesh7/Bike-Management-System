using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.ViewModels
{
    public class CategoryView
    {
        public int CategoryID { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool RemoveFromViewFlag { get; set; }
    }
}
