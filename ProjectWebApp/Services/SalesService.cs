using SalesReturnsSystem.ViewModels;
//using SalesReturnsSystem.ViewModels.SalesReturnsSystem.ViewModels;

namespace ProjectWebApp.Services
{
    public class SalesService
    {
        public Task<List<CustomerViewModel>> SearchCustomersByPhoneAsync(string phone)
        {
            // Dummy data
            var data = new List<CustomerViewModel>
            {
                new() { CustomerID = 1, FirstName = "Sam", LastName = "Smith", ContactPhone = "780.444.4444", Address = "12345 – 67 St Edmonton, AB T5J1X1" },
                new() { CustomerID = 2, FirstName = "John", LastName = "Jones", ContactPhone = "780.432.2222", Address = "23456 – 78 St Edmonton, AB T5J1X2" }
            };
            return Task.FromResult(data.Where(c => c.ContactPhone.Contains(phone)).ToList());
        }

        public async Task<List<CategoryViewModel>> GetCategoriesAsync()
        {
           
            return await Task.FromResult(new List<CategoryViewModel>
            {
                new() { CategoryID = 1, Description = "Hand Tools", ItemCount = 10 },
                new() { CategoryID = 2, Description = "Power Tools", ItemCount = 5 },
            });
        }

        public async Task<List<PartViewModel>> GetPartsAsync()
        {
           
            return await Task.FromResult(new List<PartViewModel>
            {
                new() { PartID = 1, PartName = "Hammer", QOH = 10, CategoryID = 1, Price = 12.5m },
                new() { PartID = 2, PartName = "Screwdriver", QOH = 15, CategoryID = 1, Price = 7.99m },
                new() { PartID = 3, PartName = "Drill", QOH = 5, CategoryID = 2, Price = 99.99m }
            });
        }

        public Task<CouponViewModel?> ValidateCouponAsync(string couponCode)
        {
            if (couponCode == "GSTFree")
            {
             
                return Task.FromResult<CouponViewModel?>(new CouponViewModel
                {
                    CouponCode = "GSTFree",
                    CouponIDValue = "GSTFree",
                    CouponDiscount = 5
                });
            }

            return Task.FromResult<CouponViewModel?>(null);
        }

    }
}

