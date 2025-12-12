using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServicingSystem.BLL;
using ServicingSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ServicingSystem
{
    public static class eBikeServiceExtension
    {
        public static void AddBackendDependencies(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<EBikeServicingContext>(options);

            services.AddTransient<CustomerLookupService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<EBikeServicingContext>();

                return new CustomerLookupService(context);
            }
            );

            services.AddTransient<ServiceManagementService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<EBikeServicingContext>();

                return new ServiceManagementService(context);
            }
            );

        }


    }
}
