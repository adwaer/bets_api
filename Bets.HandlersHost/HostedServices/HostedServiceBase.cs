using System;
using System.Threading;
using System.Threading.Tasks;
using In.Logging;
using NATS.Client;

namespace Bets.HandlersHost.HostedServices
{
    public abstract class HostedServiceBase
    {
        private readonly ILogService _logService;

        protected HostedServiceBase(ILogService logService)
        {
            _logService = logService;
        }

        protected async Task WrapStarter(Action onStarted, CancellationToken cancellationToken)
        {
            LogInfo("Staring..");

            var disconnected = true;
            do
            {
                try
                {
                    onStarted();
                    disconnected = false;
                }
                catch (NATSNoServersException ex)
                {
                    LogError(ex, "Cant connect NATS queue, retry in 10 sec..");
                    await Task.Delay(10000, cancellationToken);
                }
            } while (disconnected);

            LogInfo("Started!");
        }

        protected void LogDebug(string msg)
        {
            _logService.LogDebug(GetType().ToString(), msg);
        }

        protected void LogInfo(string msg)
        {
            _logService.LogInfo(GetType().ToString(), msg);
        }

        protected void LogError(Exception ex, string msg)
        {
            _logService.LogError(GetType().ToString(), ex, msg);
        }
    }
}