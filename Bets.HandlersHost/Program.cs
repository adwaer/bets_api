using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bets.Games.Dal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Bets.HandlersHost
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            
            var host = CreateHostBuilder(args)
                .Build();

            if (args.Any(arg => arg.Equals("--migrate-db")))
            {
                using var scope = host.Services.CreateScope();
                var services = scope.ServiceProvider;
                SeedData.Initialize(services);
                Log.Information("EF MIGRATION SOCCEED");
            }
            else
            {
                try
                {
                    Log.Information("Starting web host");
                    host.Run();
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Host terminated unexpectedly");
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}