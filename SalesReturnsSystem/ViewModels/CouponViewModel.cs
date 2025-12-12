using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace SalesReturnsSystem.ViewModels
{
    public class CouponViewModel
    {
        public int CouponID { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public string CouponIDValue { get; set; } = string.Empty;   
        public int CouponDiscount { get; set; }                    
        public DateTime StartDate { get; set; }                     
        public DateTime EndDate { get; set; }
        public bool RemoveFromViewFlag { get; set; }
       
        public string CouponDescription => $"{CouponIDValue} ({CouponDiscount}% Off)";
        public bool IsActive => DateTime.Now >= StartDate && DateTime.Now <= EndDate;
    }
}




