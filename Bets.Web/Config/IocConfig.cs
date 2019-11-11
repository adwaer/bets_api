using System;
using System.IO;
using System.Reflection;
using Bets.Api.Dal;
using Bets.Games.Domain.Models;
using In.Cqrs.Nats.Config;
using In.DataAccess.Mongo.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Bets.Web.Config
{
    /// <summary>
    /// Cfg
    /// </summary>
    public static class IocConfig
    {
        private const string CorsPolicy = "CorsPolicy";

        private static readonly Assembly[] Assemblies =
        {
            typeof(ApiCtx).Assembly,
            typeof(SimpleMessageResult).Assembly
        };

        /// <summary>
        /// Cors policy
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Nats mq
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection AddNats(this IServiceCollection services, IConfiguration config)
        {
            var natsUrl = config["NatsSenderOptions:Url"];
            return services
                .AddNats(new NatsSenderOptions
                {
                    Url = natsUrl
                });
        }

        /// <summary>
        /// Swagger
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            return services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Bets API",
                    Version = "v1"
                });
                
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Bets.Web.xml"));
                
//                var security = new Dictionary<string, IEnumerable<string>>
//                {
//                    {"Bearer", new string[] { }},
//                };
//
//                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
//                {
//                    In = "header",
//                    Description = "Please enter JWT with Bearer into field",
//                    Name = "Authorization",
//                    Type = "apiKey"
//                });
//
//                c.AddSecurityRequirement(security);
            });
        }

        /// <summary>
        /// Mongo db
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration)
        {
            const string eventsDbConnectionName = "EventsConnection";
            var eventsDbConnectionString = configuration.GetConnectionString(eventsDbConnectionName);
            
            return services
                .AddScoped(provider => new ApiCtx(eventsDbConnectionString))
                .AddMongo(Assemblies);
        }
    }
}