using Bets.Games.Domain.Models;
using Bets.Web.Config;
using In.Common.Config;
using In.Cqrs.Command.Nats.Config;
using In.Cqrs.Query.Nats.Config;
using In.Web.Middlerwares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Bets.Web
{
    /// <summary>
    /// Startup
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// web config
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public IConfiguration Configuration { get; }
        private const string CorsPolicy = "CorsPolicy";

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container. 
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddCorsPolicy()
                .AddCommon()
                .AddNats(Configuration)
                .AddMongo(Configuration)
                .AddNatsCommands<SimpleMessageResult>()
                .AddNatsQueries()
                .AddSwagger()
                .AddControllers();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ErrorHandlingMiddleware>()
                .UseStaticFiles()
                .UseSwagger()
                .UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bets API v1"); })
                .UseHttpsRedirection()
                .UseSerilogRequestLogging()
                .UseCors("CorsPolicy")
                .UseRouting()
//                .UseAuthorization()
                .UseEndpoints(endpoints => { endpoints.MapControllers(); })
                .Run(context => context.Response.WriteAsync("The service is online"));
        }
    }
}