using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SalesReturnsSystem.DAL;
using SalesReturnsSystem.BLL;



namespace SalesReturnsSystem
{
    public static class eBikeServiceExtension
    {
        public static void AddBackendDependencies(this IServiceCollection services,
            Action<DbContextOptionsBuilder> options)
        {
        
            services.AddDbContext<eBike_2025Context>(options);

       
            services.AddScoped<SalesService>();
            services.AddScoped<SalesTransactionService>();
            services.AddScoped<ReturnService>();
            services.AddScoped<ReturnTransactionService>();
        }
    }
}


