using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingSystem.ViewModels
{

    public class UnorderedItemView
    {
        public string Description { get; set; } = string.Empty;
        public string VendorPartId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
