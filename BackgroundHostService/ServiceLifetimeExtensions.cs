using Hardware.Info;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Udemy1.Business.Services;
using Udemy1.Business.Utilities.HostService;
using Udemy1.Data.Interfaces;
using Udemy1.WebAPI.ActionFilters;

namespace Helpers
{
    public static class ServiceLifetimeExtensions
    {
      
        public static IServiceCollection AddBusinessUtilityLifetime(this IServiceCollection services)
        {
            // HostingEnvironmetnLog : IHostedService
            services.AddTransient<IHostedService, HostingEnvironmentLog>();
            // Version = "1.1.1.1" cross-patform hardware info
            services.AddTransient<IHardwareInfo, HardwareInfo>();
            return services;
        }
    }
}
