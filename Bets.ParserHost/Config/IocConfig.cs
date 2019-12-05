using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bets.ParserHost.Config
{
    public static class IocConfig
    {
        public static IServiceCollection AddConfigOptions(this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.Configure<ParsingSettings>(configuration.GetSection("ParsingSettings"));
        }
    }
}