using In.Cqrs.Nats.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Bets.Web.Config
{
    public static class IocConfig
    {
        private const string CorsPolicy = "CorsPolicy";

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicy,
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            return services;
        }

        public static IServiceCollection AddNats(this IServiceCollection services, IConfiguration config)
        {
            var natsUrl = config["NatsSenderOptions:Url"];
            return services
                .AddNats(new NatsSenderOptions
                {
                    Url = natsUrl
                });
        }

        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            return services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Bets API",
                    Version = "v1"
                });
            });
        }
    }
}