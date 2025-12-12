using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PurchasingSystem.DAL;
using PurchasingSystem.BLL;

namespace PurchasingSystem
{
    public static class eBikeServiceExtension
    {
        public static void AddBackendDependencies(this IServiceCollection services,
                                                  Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<EBikeContext>(options);

            // Register Category Service
            services.AddTransient<CategoryService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<EBikeContext>();
                return new CategoryService(context);
            });

            // Register Part Service
            services.AddTransient<PartService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<EBikeContext>();
                return new PartService(context);
            });

            // Register PurchaseOrderDetail Service
            services.AddTransient<PurchaseOrderDetailService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<EBikeContext>();
                return new PurchaseOrderDetailService(context);
            });

            // Register PurchaseOrder Service
            services.AddTransient<PurchaseOrderService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<EBikeContext>();
                return new PurchaseOrderService(context);
            });

            // Register Vendor Service
            services.AddTransient<VendorService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<EBikeContext>();
                return new VendorService(context);
            });
        }
    }
}
