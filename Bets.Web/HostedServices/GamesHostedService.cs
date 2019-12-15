using System;
using System.Threading;
using System.Threading.Tasks;
using Bets.Configuration;
using Bets.Configuration.Services;
using Bets.Games.Domain.MQMsgs;
using Bets.Web.Config;
using Bets.Web.Services;
using In.Cqrs.Nats.Abstract;
using In.Logging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NATS.Client;

namespace Bets.Web.HostedServices
{
    /// <summary>
    /// Games hosted service
    /// </summary>
    public class GamesHostedService : HostedServiceBase, IHostedService
    {
        private readonly INatsConnectionFactory _connectionFactory;
        private readonly INatsBkMessageReplyFactory _replyFactory;
        private readonly IHubContext<GamesHub, IGamesService> _hubContext;
        private readonly ParsingQueueSettings _options;
        private IEncodedConnection _connection;
        private IAsyncSubscription _subscription;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logService"></param>
        /// <param name="connectionFactory"></param>
        /// <param name="options"></param>
        /// <param name="replyFactory"></param>
        public GamesHostedService(ILogService logService, INatsConnectionFactory connectionFactory,
            IOptions<ParsingQueueSettings> options, INatsBkMessageReplyFactory replyFactory, IHubContext<GamesHub, IGamesService> hubContext)
            : base(logService)
        {
            _connectionFactory = connectionFactory;
            _replyFactory = replyFactory;
            _hubContext = hubContext;
            _options = options.Value;
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await WrapStarter(() => _connection = _connectionFactory.Get<BkMqMessage>(), cancellationToken);

            _subscription = _connection.SubscribeAsync(_options.QueueSubject, CreateHandler());
        }

        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            LogInfo("Stopped!");

            _connection.Dispose();
            _subscription.Dispose();
            return Task.CompletedTask;
        }

        private EventHandler<EncodedMessageEventArgs> CreateHandler()
        {
            return async (sender, args) =>
            {
                var obj = (BkMqMessage) args.ReceivedObject;
                var games = _replyFactory.Get(obj);

                await _hubContext.Clients.All.SendGames(games);
            };
        }
    }
}