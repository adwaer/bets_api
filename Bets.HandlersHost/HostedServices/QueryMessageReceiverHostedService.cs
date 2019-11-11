using System;
using System.Threading;
using System.Threading.Tasks;
using In.Common;
using In.Cqrs.Nats.Abstract;
using In.Cqrs.Query.Nats;
using In.Cqrs.Query.Nats.Adapters;
using In.Logging;
using Microsoft.Extensions.Hosting;
using NATS.Client;

namespace Bets.HandlersHost.HostedServices
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class QueryMessageReceiverHostedService : HostedServiceBase, IHostedService
    {
        private readonly IDiScope _diScope;
        private readonly INatsConnectionFactory _connectionFactory;
        private readonly INatsReceiverQueryQueueFactory _queueFactory;
        private readonly INatsQueryReplyFactory _replyFactory;

        private IEncodedConnection _connection;
        private IAsyncSubscription _subscription;

        public QueryMessageReceiverHostedService(
            IDiScope diScope, INatsConnectionFactory connectionFactory, INatsReceiverQueryQueueFactory queueFactory,
            ILogService logService, INatsQueryReplyFactory replyFactory) : base(logService)
        {
            _diScope = diScope;
            _connectionFactory = connectionFactory;
            _queueFactory = queueFactory;
            _replyFactory = replyFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await WrapStarter(() => _connection = _connectionFactory.Get<QueryNatsAdapter>(), cancellationToken);

            var queue = _queueFactory.Get();
            _subscription = _connection.SubscribeAsync(queue, CreateHandler());
        }

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
                var response = (QueryNatsAdapter) args.ReceivedObject;
                var data = _replyFactory.Get(response);
                LogDebug($"accepted msg: {data}");

                try
                {
                    var query = _diScope.Resolve(data.GetQueryType());
                    if (query == null)
                    {
                        throw new Exception(
                            $"No handler found for: {data}");
                    }

                    var queryResult = await _replyFactory.ExecuteQuery(query, data);
                    response.QueryResult = queryResult;
                    response.QueryResultType = queryResult.GetType().ToString();
                }
                catch (Exception ex)
                {
                    LogError(ex, ex.Message);

                    response.QueryResultType = typeof(string).ToString();
                    response.QueryResult = ex.Message;
                }
                
                try
                {
                    _connection.Publish(data.Reply, response);
                    _connection.Flush();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error when publish result");
                }
            };
        }
    }
}