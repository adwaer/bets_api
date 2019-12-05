using Bets.ParserHost.Config;
using Bets.ParserHost.Helpers;
using Bets.ParserHost.HostedServices;
using In.Common.Config;
using In.Cqrs.Nats.Config;
using In.Logging;
using In.Logging.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Bets.ParserHost
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddCommon()
                .AddConfigOptions(Configuration)
                .AddSingleton<ILogService, LogService>()
                .AddSingleton<ThreadProvider>()
                .AddNats(new NatsSenderOptions
                {
                    Url = Configuration["NatsSenderOptions:Url"]
                })
                .AddHostedService<OneXBetParserHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.AddSerilog();
            app.UseSerilogRequestLogging()
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Hello World!"); });
                });
        }
    }
}