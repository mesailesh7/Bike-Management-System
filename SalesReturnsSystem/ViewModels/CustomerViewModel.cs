using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.ViewModels
{
    public class CustomerViewModel
    {
        public int CustomerID { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Phone => ContactPhone;
        public string FullName => $"{FirstName} {LastName}";
    }
}



