using BYSResults;
using ServicingSystem.DAL;
using ServicingSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServicingSystem.BLL
{
    public class CustomerLookupService
    {
        #region Data Field
        private readonly EBikeServicingContext _context;
        #endregion

      
        internal CustomerLookupService(EBikeServicingContext context)
        {
            
            _context = context ?? throw new ArgumentException(nameof(context));
        }

        public Result<List<CustomerView>> GetCustomerByLastName(string lastName)
        {
            var result = new Result<List<CustomerView>>();

            try
            {
                if (string.IsNullOrWhiteSpace(lastName))
                {
                    result.AddError(new Error("Invalid Input", "Last name cannot be empty"));
                    return result;
                }

                var salesCustomers = _context.Customers
                    .Where(x => x.LastName.ToLower().Contains(lastName.ToLower()) && !x.RemoveFromViewFlag)
                    .OrderBy(x => x.LastName)
                    .ThenBy(x => x.FirstName)
                    .Select(x => new CustomerView
                    {
                        Id = x.CustomerID,
                        Name = $"{x.FirstName} {x.LastName}",
                        Phone = x.ContactPhone,
                        Address = $"{x.Address}, {x.City}, {x.Province}, {x.PostalCode}",
                        Vehicles = x.CustomerVehicles.Select(v => new CustomerVehicleView
                        {
                            CustomerVehicleID = v.CustomerVehicleID,
                            Make = v.Make,
                            Model = v.Model,
                            VehicleIdentification = v.VehicleIdentification
                        }).ToList()
                    })
                    .ToList();

                if (salesCustomers == null || salesCustomers.Count == 0)
                {
                    result.AddError(new Error("No Customer", "Could not find customer with the last name provided"));
                    return result;
                }

                return result.WithValue(salesCustomers);
            }
            catch (Exception ex)
            {
                result.AddError(new Error("Database Error", ex.Message));
                return result;
            }
        }
    }
}
