using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;


//CLI Environment Definition [User]
//set ASPNETCORE_ENVIRONMENT=Staging
//dotnet run --no-launch-profile

//CLI Environment Definition [Machine]
//[Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Staging", "Machine") => PowerShell 

namespace Configure
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            /*User Secrets*/
            //Configure Secrets CLI => dotnet user-secrets init!
            //Setting A Secret Info CLI => dotnet user-secrets set "Secrets:PrivateKey" "128 bits private key will be stored in secret"

            services.AddControllers();

            //Dependency Injection for custom options related to binding
            services.Configure<CustomOptions>(Configuration.GetSection("keyvault")); //weatherforecastcontroller.cs di in constructor!
            services.Configure<CustomOptions>(Configuration.GetSection("secrets"));//weatherforecastcontroller.cs IOptions<CustomOptions>() in constructor!
            //for sensitive informations you may use key vaults, ex. AzureKeyVault
            
            // configuration for writable appsettings.Development.json
            services.ConfigureWritable<AppSettings>(Configuration.
                GetSection("AppSettings"),"appsettings.Development.json");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //More detailed environment management
            if (env.IsProduction() || env.IsStaging() || env.IsEnvironment("Staging_Version_2"))
            {
                app.UseExceptionHandler("/Error");
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

             app.Use(async (context,next)=>{
                //Binding
                //var custOptions = new CustomOptions();
                var custOptions = Configuration.GetSection("keyvault").Get<CustomOptions>();

                //Configuration.GetSection("keyvault").Bind(custOptions);
                Configuration.GetSection("Secrets").Bind(custOptions);

                //appsettings.json / appsettings.dev.json
                var configValue = Configuration.GetValue<int>("Secrets:SessionKey");
                await context.Response.WriteAsync($"PreMasterKey:{custOptions.PreMasterKey}\nSessionKey:{custOptions.SessionKey}\nSessionKey2X:{configValue*2}\nPublicKey:{custOptions.PublicKey}\nEnvironmentPublicKey:{Configuration["PublicKey"]}\nEnvironmentPrivateKey:{Configuration["PrivateKey"]}");

                //set a key's default value , if not exists key will be valued by default
                //Configuration.GetValue();
                System.Console.WriteLine($"MasterKey:{Configuration.GetValue<Guid>("MasterKey",Guid.NewGuid())}");

                await next.Invoke();
                await context.Response.WriteAsync($"Paths__Url_Account:{Configuration["Paths:Url_Account"]}");
            });

            app.Run(async context=>{
                //all configurations, from bottom to the top.
                var script = "\n";
                foreach (var item in Configuration.AsEnumerable())
                {
                    script += "\n"+item+"\n";
                }
                await context.Response.WriteAsync(script);
                
            });
        }
    }
}
