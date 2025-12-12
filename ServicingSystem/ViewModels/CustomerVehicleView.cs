using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicingSystem.ViewModels
{
    public class CustomerVehicleView
    {
        public int CustomerVehicleID { get; set; }
       
        public string Make { get;set; }
        public string Model { get;set; }
        public string VehicleIdentification { get; set; } = string.Empty; 
        public bool RemoveFromViewFlag { get; set; }
    }
}
