using CASEnet.SecureIdServer.Data.Interface;
using CASEnet.SecureIdServer.Service;
using CASEnet.SecureIdServer.Service.Implement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CASEnet.SecureIdServer.WebApi
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
            services.AddControllers();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<ILoggerServices, LoggerServices>();
            services.AddHttpContextAccessor();
            services.ReadConnectionSettingConfig(Configuration, "DeviceDb");
            services.CustomizedAddSwagger(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.CustomizedMapWhenSwagger(Configuration);

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
