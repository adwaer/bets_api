using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bets.Configuration;
using Bets.Configuration.Services;
using Bets.Games.Domain.MQMsgs;
using Bets.Games.Services;
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
    // ReSharper disable once ClassNeverInstantiated.Global
    public class GamesHostedService : HostedServiceBase, IHostedService
    {
        private readonly INatsConnectionFactory _connectionFactory;
        private readonly INatsBkMessageReplyFactory _replyFactory;
        private readonly IHubContext<GamesHub, IGamesService> _hubContext;
        private readonly ParsingQueueSettings _options;
        private IEncodedConnection _connection;
        private IAsyncSubscription _subscription;
        private readonly CorridorsService _corridorsService = new CorridorsService();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logService"></param>
        /// <param name="connectionFactory"></param>
        /// <param name="options"></param>
        /// <param name="replyFactory"></param>
        /// <param name="hubContext"></param>
        public GamesHostedService(ILogService logService, INatsConnectionFactory connectionFactory,
            IOptions<ParsingQueueSettings> options, INatsBkMessageReplyFactory replyFactory,
            IHubContext<GamesHub, IGamesService> hubContext)
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
                var bkGames = _replyFactory.Get(obj);

                var games = _corridorsService.AddBkGames(bkGames);

                if (games.Any())
                {
                    await _hubContext.Clients.All.SendGames(games);
                }
            };
        }
    }
}