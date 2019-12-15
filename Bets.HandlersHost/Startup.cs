using Bets.HandlersHost.Config;
using Bets.HandlersHost.HostedServices;
using In.Common.Config;
using In.Cqrs.Command.Nats.Config;
using In.Cqrs.Query.Nats.Config;
using In.Logging.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Bets.HandlersHost
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
                .AddMapper()
                .AddLogger()
                .AddNats(Configuration)
                .AddEfCore(Configuration)
                .AddQueries()
                .AddNatsQueryFactory()
                .AddCommands()
                .AddNatsCommandFactory()
                .AddDDD()
                .AddHostedService<CommandsMessageRecieverHostedService>()
                .AddHostedService<QueryMessageReceiverHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting()
                .UseSerilogRequestLogging()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Hello World!"); });
                });
        }
    }
}