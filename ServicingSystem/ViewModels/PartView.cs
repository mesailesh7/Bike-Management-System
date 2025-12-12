using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicingSystem.ViewModels
{
    public class PartView
    {
        public int PartID { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal ExtPrice { get; set; }
        public string CategoryDescription { get; set; } = string.Empty;
        public bool IsExistingVehiclePart { get; set; }
        public bool IsNewPart { get; set; }
        public bool RemoveFromViewFlag { get; set; }
        public string PartType => IsExistingVehiclePart ? "Running" : "New";
        public decimal Total => SellingPrice * Quantity;
    }
}



