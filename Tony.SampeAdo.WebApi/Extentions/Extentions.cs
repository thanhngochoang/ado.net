using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.IO;
using System.Reflection;
using static Tony.SampeAdo.Service.DeviceDbSql;

namespace Tony.SampeAdo.WebApi.Extentions
{
    public static class Extentions
    {
        public static IApplicationBuilder CustomizedMapWhenSwagger(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.MapWhen(
                    context =>
                        context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase),
                    branch =>
                    {
                        branch.UseSwagger();
                        branch.UseSwaggerUI(options =>
                        {
                            options.SwaggerEndpoint(configuration["Swagger:Endpoint"], configuration["Swagger:Title"]);
                        });
                    });
            return app;
        }

        public static void CustomizedAddSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(configuration["Swagger:Version"], new OpenApiInfo
                {
                    Title = configuration["Swagger:Title"],
                    Version = configuration["Swagger:Version"],
                    Description = configuration["Swagger:Description"]
                });


                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                options.OperationFilter<AddResponseHeadersFilter>();
            });
        }

        public static IServiceCollection ReadConnectionSettingConfig(this IServiceCollection services, IConfiguration configuration, string connectionName)
        {
            services.Configure<Settings>(options =>
            {
                configuration.GetSection(nameof(Settings)).Bind(options);
                options.ConnectionString = configuration.GetConnectionString(connectionName);
                options.ServerSMSConnect = configuration.GetValue<string>("SmsConfig");
            });

            return services;
        }

        public static string RandomDigit()
        {
            var generator = new Random();
            return generator.Next(0, 999999).ToString("D6");
        }

        public static bool IsPhoneNumber(string number)
        {
            return true;
            //return Regex.Match(number, @"^(\+[0-9]{9})$").Success;
        }
    }
}
