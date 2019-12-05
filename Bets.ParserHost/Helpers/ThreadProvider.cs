using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Bets.ParserHost.Helpers
{
    public class ThreadProvider
    {
        private readonly ILogger<ThreadProvider> _logger;

        public ThreadProvider(ILogger<ThreadProvider> logger)
        {
            _logger = logger;
        }

        public void Run(Action action, Action continueWith = null)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
                
                continueWith?.Invoke();
            });
        }
    }
}