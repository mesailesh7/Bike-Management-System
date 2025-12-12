using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicingSystem.ViewModels
{
    public class ServiceCartItemView
    {
        public int ServiceID { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Hours { get; set; }
        public decimal Rate { get; set; } = 65.50m;
        public decimal ExtPrice => Hours * Rate;
        public string Comment { get; set; } = string.Empty;
        public bool IsCustom { get; set; }
        public bool RemoveFromViewFlag { get; set; }
        public DateTime? StartDate { get; set; }
        public bool IsRunning => StartDate.HasValue;
    }
}
