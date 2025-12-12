using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReceivingSystem.BLL;
using ReceivingSystem.DAL;
using ReceivingSystem.Entities;

namespace ReceivingSystem
{
    public static class EBikeServiceExtension
    {
        public static void AddBackendDependencies(this IServiceCollection services,
            Action<DbContextOptionsBuilder> options)
        {
        
            services.AddDbContext<eBikeContext>(options);

          
            services.AddTransient<ReceivingService>((serviceProvider) =>
            {
                var context = serviceProvider.GetRequiredService<eBikeContext>();
                return new ReceivingService(context);
            });
        }
    }
}
