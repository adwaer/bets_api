using System.Reflection;
using AutoMapper;
using Bets.Games.Dal;
using Bets.Games.Domain.Models;
using Bets.Games.QueryHandlers;
using In.Cqrs.Command.Config;
using In.Cqrs.Nats.Config;
using In.Cqrs.Query.Config;
using In.DataAccess.EfCore.Config;
using In.DataMapping.Automapper.Config;
using In.DDD.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bets.HandlersHost.Config
{
    public static class IocConfig
    {
        private const string CorsPolicy = "CorsPolicy";

        private static readonly Assembly[] Assemblies =
        {
            typeof(GamesCtx).Assembly,
            typeof(QueryHandler).Assembly
        };

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

        public static IServiceCollection AddEfCore(this IServiceCollection services, IConfiguration configuration)
        {
            const string connName = "DefaultConnection";
            var conn = configuration.GetConnectionString(connName);
            
            return services
                .AddDbContext<GamesCtx>(x => x.UseSqlServer(conn))
                .AddEfCore<GamesCtx>(Assemblies);
        }

        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            return services.AddAutomapping(Assemblies);
        }

        public static IServiceCollection AddDDD(this IServiceCollection services)
        {
            return services.AddDdd(Assemblies);
        }

        public static IServiceCollection AddQueries(this IServiceCollection services)
        {
            return services.AddQueries(Assemblies);
        }
        
        public static IServiceCollection AddCommands(this IServiceCollection services)
        {
            return services.AddCommands<SimpleMessageResult>(Assemblies);
        }
    }
}