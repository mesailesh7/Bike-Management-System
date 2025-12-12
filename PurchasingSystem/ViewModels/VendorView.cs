using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.ViewModels
{
    public class VendorView
    {
        public int VendorID { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string Phone { get; set;} = string.Empty;
        public string Address { get; set;} = string.Empty;
        public string City { get; set;} = string.Empty;
        public string ProvinceID { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public bool RemoveFromViewFlag { get; set; }
    }
}
