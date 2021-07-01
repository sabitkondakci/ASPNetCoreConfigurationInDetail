using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Configure
{
    public class Program
    {
        public static async Task  Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.CaptureStartupErrors(true);
                    //if static file container of wwwroot doesn't exist use alternative stafiles.
                    webBuilder.UseWebRoot("stafiles");
                    //for more detailed errors
                    webBuilder.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
                    //customs ports
                    webBuilder.UseUrls("http://localhost:5003", "https://localhost:5004");
                })
                .ConfigureServices(services=>{
                    //lifetime management
                    services.AddHostedService<LifetimeHostedService>();

                    //buy some programs time to end their service!
                    services.Configure<HostOptions>(option =>{
                        option.ShutdownTimeout = System.TimeSpan.FromSeconds(20);
                    });

                }).ConfigureAppConfiguration(appConfig=>{
                    appConfig.AddJsonFile("custom_configurations.json",true);
                    appConfig.AddEnvironmentVariables("PROCESSOR_");

                    //in memory configurations
                    var inMemoConfiguration = new Dictionary<string,string>(){
                        {"KeyVault:PublicKey","From InMemory Configuration"}
                    };

                    //it is put after appConfig.AddJsonFile , hence it gets overridden by inMemoConfiguration
                    appConfig.AddInMemoryCollection(inMemoConfiguration);

                    //For EFCustomConfigurations, visit the link : https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#custom-configuration-provider
                });
                //at app level configuration,you may use ConfigureAppConfiguration((context,builder)=>{})

                /*Key Differences , ConfigureAppConfiguration & ConfigureHostConfiguration
                    1. ConfigureHostConfiguration runs before startup.cs gets triggered so that
                    if there's similar key value pairs in custom_configuration they will be overridden 
                    by appsettings.json & appsettings.Development.json
                    
                    2. ConfigureAppConfiguration runs the last so that key value pairs will be of value in custom_configuration.json
                    nothing will be overridden by default appsettings' key value pairs.

                */
    }
}
