using System;
using System.Collections.Generic;
using System.Linq;
using BYSResults;
using Microsoft.EntityFrameworkCore;
using ServicingSystem.DAL;
using ServicingSystem.ViewModels;

namespace ServicingSystem.BLL
{
    public class ServiceManagementService
    {
        #region Data Field
        private readonly EBikeServicingContext _context;
        #endregion

        internal ServiceManagementService(EBikeServicingContext context)
        {

            _context = context ?? throw new ArgumentException(nameof(context));
        }

        public Result<List<ServiceView>> GetStandardServices(int customerId, int vehicleId)
        {
            var result = new Result<List<ServiceView>>();

            try
            {
                if ((customerId > 0) && (vehicleId > 0))
                {
                    var services = _context.Jobs
                        .Where(x => x.CustomerVehicleID == vehicleId && !x.RemoveFromViewFlag)
                        .SelectMany(x => x.JobDetails)
                        .Where(x => !x.RemoveFromViewFlag)
                        .Select(x => new ServiceView
                        {
                            ServiceID = x.JobDetailID,
                            Description = x.Description,
                            StandardHours = x.JobHours,
                            Rate = 65.5m,
                            ExtPrice = x.JobHours * 65.5m,
                            StartDate = x.Job.JobDateStarted,
                            Comment = x.Comments
                        })
                        .ToList();

                    if (services == null || services.Count == 0)
                    {
                        result.AddError(new Error("No services found", "No services were found for this vehicle"));
                        return result;
                    }

                    return result.WithValue(services);
                }
                else
                {
                    var standardServices = _context.StandardJobs
                        .Where(x => !x.RemoveFromViewFlag)
                        .OrderBy(x => x.Description)
                        .Select(x => new ServiceView
                        {
                            StandardJobID = x.StandardJobID,
                            Description = x.Description,
                            StandardHours = x.StandardHours,
                            Rate = 65.5m,
                            ExtPrice = x.StandardHours * 65.5m
                        })
                        .ToList();

                    if (standardServices == null || standardServices.Count == 0)
                    {
                        result.AddError(new Error("No standard services", "No standard services were found"));
                        return result;
                    }

                    return result.WithValue(standardServices);
                }
            }
            catch (Exception ex)
            {
                result.AddError(new Error("Database Error", ex.Message));
                return result;
            }
        }

        public Result<List<PartView>> GetPartsByCategory(int categoryID, int vehicleId)
        {
            var result = new Result<List<PartView>>();

            try
            {
                var query = _context.Parts
                    .Where(x => !x.RemoveFromViewFlag && x.QuantityOnHand > 0);

                if (categoryID > 0)
                {
                    query = query.Where(x => x.CategoryID == categoryID);
                }

                var parts = query
                    .Include(x => x.Category)
                    .OrderBy(x => x.Description)
                    .Select(x => new PartView
                    {
                        PartID = x.PartID,
                        Description = x.Description,
                        SellingPrice = x.SellingPrice,
                        Quantity = x.QuantityOnHand,
                        CategoryDescription = x.Category.Description
                    })
                    .ToList();

                if (parts == null || parts.Count == 0)
                {
                    result.AddError(new Error("No parts found", "No parts were found for the selected category"));
                    return result;
                }

                return result.WithValue(parts);
            }
            catch (Exception ex)
            {
                result.AddError(new Error("Database Error", ex.Message));
                return result;
            }
        }

        public Result<List<CategoryView>> GetCategories()
        {
            var result = new Result<List<CategoryView>>();

            try
            {
                var salesCategories = _context.Categories
                    .Where(x => !x.RemoveFromViewFlag)
                    .OrderBy(x => x.Description)
                    .Select(x => new CategoryView
                    {
                        CategoryID = x.CategoryID,
                        Description = x.Description,
                        RemoveFromViewFlag = x.RemoveFromViewFlag
                    })
                    .ToList();

                if (salesCategories == null || salesCategories.Count == 0)
                {
                    result.AddError(new Error("No salesCategories found", "No salesCategories were found"));
                    return result;
                }

                return result.WithValue(salesCategories);
            }
            catch (Exception ex)
            {
                result.AddError(new Error("Database Error", ex.Message));
                return result;
            }
        }


        public Result<decimal> ValidateCoupon(string couponCode)
        {
            var result = new Result<decimal>();

            try
            {
                if (string.IsNullOrWhiteSpace(couponCode))
                {
                    result.AddError(new Error("Invalid Input", "Coupon code cannot be empty"));
                    return result;
                }

                var coupon = _context.Coupons
                    .Where(x => x.CouponIDValue == couponCode &&
                               x.StartDate <= DateTime.Now &&
                               x.EndDate >= DateTime.Now &&
                               !x.RemoveFromViewFlag)
                    .FirstOrDefault();

                if (coupon == null)
                {
                    result.AddError(new Error("Invalid Coupon", "Coupon not found or expired"));
                    return result;
                }

                return result.WithValue(coupon.CouponDiscount);
            }
            catch (Exception ex)
            {
                result.AddError(new Error("Database Error", ex.Message));
                return result;
            }
        }

        public Result<CustomerView> GetCustomerById(int customerId)
        {
            var result = new Result<CustomerView>();

            try
            {
                var customer = _context.Customers
                    .Where(x => x.CustomerID == customerId && !x.RemoveFromViewFlag)
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
                    .FirstOrDefault();

                if (customer == null)
                {
                    result.AddError(new Error("Customer not found", $"No customer found with ID: {customerId}"));
                    return result;
                }

                return result.WithValue(customer);
            }
            catch (Exception ex)
            {
                result.AddError(new Error("Database Error", ex.Message));
                return result;
            }
        }

        public Result<List<PartView>> GetPartsForVehicle(int vehicleId)
        {
            var result = new Result<List<PartView>>();

            try
            {
                if (vehicleId <= 0)
                {
                    result.AddError(new Error("Invalid Input", "Vehicle ID must be greater than 0"));
                    return result;
                }

                var salesParts = _context.Jobs
                    .Where(j => j.CustomerVehicleID == vehicleId && !j.RemoveFromViewFlag)
                    .SelectMany(j => j.JobParts)
                    .Where(jp => !jp.RemoveFromViewFlag)
                    .Select(jp => new PartView
                    {
                        PartID = jp.Part.PartID,
                        Description = jp.Part.Description,
                        SellingPrice = jp.SellingPrice,
                        Quantity = jp.Quantity,
                        CategoryDescription = jp.Part.Category.Description
                    })
                    .ToList();

                if (salesParts == null || salesParts.Count == 0)
                {
                    result.AddError(new Error("No salesParts found", "No salesParts were found for this vehicle"));
                    return result;
                }

                return result.WithValue(salesParts);
            }
            catch (Exception ex)
            {
                result.AddError(new Error("Database Error", ex.Message));
                return result;
            }
        }

        public Result<int> UpdateRunningService(int jobDetailId, decimal hours, decimal rate)
        {
            var result = new Result<int>();

            try
            {
                if (jobDetailId <= 0)
                {
                    result.AddError(new Error("Invalid Input", "Job Detail ID must be greater than 0"));
                    return result;
                }

                var jobDetail = _context.JobDetails
                    .Where(x => x.JobDetailID == jobDetailId && !x.RemoveFromViewFlag)
                    .FirstOrDefault();

                if (jobDetail == null)
                {
                    result.AddError(new Error("Service not found", $"No running service found with ID: {jobDetailId}"));
                    return result;
                }

                jobDetail.JobHours = hours;

                _context.JobDetails.Update(jobDetail);
                _context.SaveChanges();

                return result.WithValue(jobDetail.JobDetailID);
            }
            catch (Exception ex)
            {
                result.AddError(new Error("Database Error", ex.Message));
                return result;
            }
        }

        public Result<int> RemoveRunningService(int jobDetailId)
        {
            var result = new Result<int>();

            try
            {
                if (jobDetailId <= 0)
                {
                    result.AddError(new Error("Invalid Input", "Job Detail ID must be greater than 0"));
                    return result;
                }

                var jobDetail = _context.JobDetails
                    .Where(x => x.JobDetailID == jobDetailId && !x.RemoveFromViewFlag)
                    .FirstOrDefault();

                if (jobDetail == null)
                {
                    result.AddError(new Error("Service not found", $"No running service found with ID: {jobDetailId}"));
                    return result;
                }

                jobDetail.RemoveFromViewFlag = true;
                _context.JobDetails.Update(jobDetail);
                _context.SaveChanges();

                return result.WithValue(jobDetail.JobDetailID);
            }
            catch (Exception ex)
            {
                result.AddError(new Error("Database Error", ex.Message));
                return result;
            }
        }

        public Result<int> SaveServiceOrder(int customerId, int vehicleId, List<ServiceCartItemView> services, List<PartView> parts, string couponCode = null)
        {
            var result = new Result<int>();

            if (customerId <= 0)
            {
                result.AddError(new Error("Invalid Input", "Customer ID must be greater than 0"));
            }

            if (vehicleId <= 0)
            {
                result.AddError(new Error("Invalid Input", "Vehicle ID must be greater than 0"));
            }

            if (result.IsFailure)
            {
                return result;
            }


            if (services != null && services.Any())
            {
                foreach (var service in services.Where(s => !s.RemoveFromViewFlag))
                {
                    if (string.IsNullOrWhiteSpace(service.Description))
                    {
                        result.AddError(new Error("Invalid Service", "Service description cannot be empty"));
                    }
                    if (service.Hours <= 0)
                    {
                        result.AddError(new Error("Invalid Service", "Service hours must be greater than 0"));
                    }
                    if (service.Rate <= 0)
                    {
                        result.AddError(new Error("Invalid Service", "Service rate must be greater than 0"));
                    }
                }
            }

            if (parts != null && parts.Any())
            {
                foreach (var part in parts.Where(p => !p.RemoveFromViewFlag && p.IsNewPart))
                {
                    if (part.PartID <= 0)
                    {
                        result.AddError(new Error("Invalid Part", "Part ID must be greater than 0"));
                    }
                    if (part.Quantity <= 0)
                    {
                        result.AddError(new Error("Invalid Part", "Part quantity must be greater than 0"));
                    }
                    if (part.SellingPrice < 0)
                    {
                        result.AddError(new Error("Invalid Part", "Part selling price cannot be negative"));
                    }

                    var partEntity = _context.Parts.FirstOrDefault(p => p.PartID == part.PartID && !p.RemoveFromViewFlag);
                    if (partEntity == null)
                    {
                        result.AddError(new Error("Part Not Found", $"Part with ID {part.PartID} not found"));
                    }
                    else if (partEntity.QuantityOnHand < part.Quantity)
                    {
                        result.AddError(new Error("Insufficient Stock", $"Part {part.Description} has insufficient stock. Available: {partEntity.QuantityOnHand}, Required: {part.Quantity}"));
                    }
                }
            }

            if (result.IsFailure)
            {
                return result;
            }

            try
            {
                var activeJob = _context.Jobs
                    .Where(j => j.CustomerVehicleID == vehicleId && !j.RemoveFromViewFlag)
                    .OrderByDescending(j => j.JobDateStarted)
                    .FirstOrDefault();


                var vehicle = _context.CustomerVehicles
                    .FirstOrDefault(cv => cv.CustomerVehicleID == vehicleId);

                if (vehicle == null)
                {
                    result.AddError(new Error("Vehicle Not Found", "Customer vehicle not found"));
                    return result;
                }

                if (activeJob == null)
                {
                    activeJob = new Entities.Job
                    {
                        CustomerVehicleID = vehicleId,
                        JobDateIn = DateTime.Now,
                        JobDateStarted = DateTime.Now,
                        EmployeeID = "SYSTEM",
                        ShopRate = 65.50m,
                        VehicleIdentification = vehicle.VehicleIdentification,
                        RemoveFromViewFlag = false
                    };


                    if (!string.IsNullOrWhiteSpace(couponCode))
                    {
                        var coupon = _context.Coupons
                            .FirstOrDefault(x => x.CouponIDValue == couponCode &&
                                               x.StartDate <= DateTime.Now &&
                                               x.EndDate >= DateTime.Now &&
                                               !x.RemoveFromViewFlag);

                        if (coupon != null)
                        {
                            activeJob.CouponID = coupon.CouponID;
                        }
                    }

                    _context.Jobs.Add(activeJob);
                    _context.SaveChanges();
                }
                else
                {

                    if (!string.IsNullOrWhiteSpace(couponCode))
                    {
                        var coupon = _context.Coupons
                            .FirstOrDefault(x => x.CouponIDValue == couponCode &&
                                               x.StartDate <= DateTime.Now &&
                                               x.EndDate >= DateTime.Now &&
                                               !x.RemoveFromViewFlag);

                        if (coupon != null)
                        {
                            activeJob.CouponID = coupon.CouponID;
                            _context.SaveChanges();
                        }
                    }
                }


                if (services != null && services.Any())
                {
                    foreach (var service in services.Where(s => !s.RemoveFromViewFlag))
                    {

                        var existingService = _context.JobDetails
                            .FirstOrDefault(jd => jd.JobID == activeJob.JobID &&
                                                 jd.Description == service.Description &&
                                                 !jd.RemoveFromViewFlag);

                        if (existingService == null)
                        {
                            var jobDetail = new Entities.JobDetail
                            {
                                JobID = activeJob.JobID,
                                Description = service.Description,
                                JobHours = service.Hours,
                                Comments = service.Comment ?? string.Empty,
                                RemoveFromViewFlag = false
                            };
                            _context.JobDetails.Add(jobDetail);
                        }
                    }
                }


                if (parts != null && parts.Any())
                {
                    foreach (var part in parts.Where(p => !p.RemoveFromViewFlag && p.IsNewPart))
                    {

                        var existingJobPart = _context.JobParts
                            .FirstOrDefault(jp => jp.JobID == activeJob.JobID &&
                                                 jp.PartID == part.PartID &&
                                                 !jp.RemoveFromViewFlag);

                        if (existingJobPart == null)
                        {

                            var partEntity = _context.Parts.FirstOrDefault(p => p.PartID == part.PartID);
                            if (partEntity != null)
                            {
                                partEntity.QuantityOnHand -= part.Quantity;
                            }


                            var jobPart = new Entities.JobPart
                            {
                                JobID = activeJob.JobID,
                                PartID = part.PartID,
                                Quantity = part.Quantity,
                                RemoveFromViewFlag = false
                            };
                            _context.JobParts.Add(jobPart);
                        }
                    }
                }

                _context.SaveChanges();
                return result.WithValue(activeJob.JobID);
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                result.AddError(new Error("Error Saving Changes", ex.InnerException?.Message ?? ex.Message));
                return result;
            }
        }





        public Result<bool> UpdateJobWithDetails(int jobId, List<ServiceCartItemView> services, List<PartView> parts)
        {
            var result = new Result<bool>();

            try
            {

                if (jobId <= 0)
                {
                    result.AddError(new Error("Invalid Input", "Job ID must be greater than 0"));
                    return result;
                }

                var job = _context.Jobs.FirstOrDefault(j => j.JobID == jobId && !j.RemoveFromViewFlag);
                if (job == null)
                {
                    result.AddError(new Error("Job Not Found", $"Job with ID {jobId} not found"));
                    return result;
                }

                job.JobDateStarted = DateTime.Now;
                _context.Jobs.Update(job);


                if (services != null)
                {

                    foreach (var service in services.Where(s => s.RemoveFromViewFlag && s.ServiceID > 0))
                    {
                        var jobDetail = _context.JobDetails.FirstOrDefault(jd => jd.JobDetailID == service.ServiceID);
                        if (jobDetail != null)
                        {
                            jobDetail.RemoveFromViewFlag = true;
                            _context.JobDetails.Update(jobDetail);
                        }
                    }


                    foreach (var service in services.Where(s => !s.RemoveFromViewFlag && s.ServiceID > 0))
                    {
                        var jobDetail = _context.JobDetails.FirstOrDefault(jd => jd.JobDetailID == service.ServiceID);
                        if (jobDetail != null)
                        {
                            jobDetail.Description = service.Description;
                            jobDetail.JobHours = service.Hours;
                            jobDetail.Comments = service.Comment ?? string.Empty;
                            _context.JobDetails.Update(jobDetail);
                        }
                    }


                    foreach (var service in services.Where(s => !s.RemoveFromViewFlag && s.ServiceID == 0))
                    {
                        var jobDetail = new Entities.JobDetail
                        {
                            JobID = jobId,
                            Description = service.Description,
                            JobHours = service.Hours,
                            Comments = service.Comment ?? string.Empty,
                            RemoveFromViewFlag = false
                        };
                        _context.JobDetails.Add(jobDetail);
                    }
                }


                if (parts != null)
                {

                    foreach (var part in parts.Where(p => p.RemoveFromViewFlag))
                    {
                        var jobParts = _context.JobParts
                            .Where(jp => jp.Part.PartID == part.PartID && jp.JobID == jobId && !jp.RemoveFromViewFlag)
                            .ToList();

                        foreach (var jobPart in jobParts)
                        {
                            jobPart.RemoveFromViewFlag = true;
                            _context.JobParts.Update(jobPart);


                            var partEntity = _context.Parts.FirstOrDefault(p => p.PartID == part.PartID);
                            if (partEntity != null)
                            {
                                partEntity.QuantityOnHand += jobPart.Quantity;
                                _context.Parts.Update(partEntity);
                            }
                        }
                    }


                    foreach (var part in parts.Where(p => !p.RemoveFromViewFlag && p.IsExistingVehiclePart))
                    {
                        var jobParts = _context.JobParts
                            .Where(jp => jp.Part.PartID == part.PartID && jp.JobID == jobId && !jp.RemoveFromViewFlag)
                            .ToList();

                        foreach (var jobPart in jobParts)
                        {
                            var quantityDifference = part.Quantity - jobPart.Quantity;
                            jobPart.Quantity = part.Quantity;
                            jobPart.SellingPrice = part.SellingPrice;
                            _context.JobParts.Update(jobPart);


                            if (quantityDifference != 0)
                            {
                                var partEntity = _context.Parts.FirstOrDefault(p => p.PartID == part.PartID);
                                if (partEntity != null)
                                {
                                    partEntity.QuantityOnHand -= quantityDifference;
                                    _context.Parts.Update(partEntity);
                                }
                            }
                        }
                    }


                    foreach (var part in parts.Where(p => !p.RemoveFromViewFlag && p.IsNewPart))
                    {
                        var jobPart = new Entities.JobPart
                        {
                            JobID = jobId,
                            PartID = part.PartID,
                            Quantity = part.Quantity,
                            SellingPrice = part.SellingPrice,
                            RemoveFromViewFlag = false
                        };
                        _context.JobParts.Add(jobPart);


                        var partEntity = _context.Parts.FirstOrDefault(p => p.PartID == part.PartID);
                        if (partEntity != null)
                        {
                            partEntity.QuantityOnHand -= part.Quantity;
                            _context.Parts.Update(partEntity);
                        }
                    }
                }

                _context.SaveChanges();

                return result.WithValue(true);
            }
            catch (Exception ex)
            {
                result.AddError(new Error("Database Error", $"An error occurred while updating the job: {ex.Message}"));
                return result;
            }
        }
        public Result<string> GetJobCoupon(int vehicleId)
        {
            var result = new Result<string>();
            try
            {
                var job = _context.Jobs
                    .Include(j => j.Coupon)
                    .Where(j => j.CustomerVehicleID == vehicleId && !j.RemoveFromViewFlag)
                    .OrderByDescending(j => j.JobDateIn)
                    .FirstOrDefault();

                if (job != null && job.Coupon != null)
                {
                    result.WithValue(job.Coupon.CouponIDValue);
                }
            }
            catch (Exception ex)
            {
                result.AddError(new Error("Database error", ex.Message));
            }
            return result;
        }

        public Result<bool> RemoveJobCoupon(int vehicleId)
        {
            var result = new Result<bool>();
            try
            {
                var job = _context.Jobs
                    .Where(j => j.CustomerVehicleID == vehicleId && !j.RemoveFromViewFlag)
                    .OrderByDescending(j => j.JobDateIn)
                    .FirstOrDefault();

                if (job != null && job.CouponID.HasValue)
                {
                    job.CouponID = null;
                    _context.SaveChanges();
                    result.WithValue(true);
                }
                else
                {
                    result.WithValue(false);
                }
            }
            catch (Exception ex)
            {
                result.AddError(new Error("Database error", ex.Message));
            }
            return result;
        }
    }
}
