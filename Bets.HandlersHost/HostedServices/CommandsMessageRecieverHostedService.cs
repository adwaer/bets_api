using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using In.Cqrs.Command.Nats;
using In.Cqrs.Command.Nats.Adapters;
using In.Cqrs.Nats.Abstract;
using In.Logging;
using Microsoft.Extensions.Hosting;
using NATS.Client;

namespace Bets.HandlersHost.HostedServices
{
    public class CommandsMessageRecieverHostedService : HostedServiceBase, IHostedService
    {
        private readonly INatsConnectionFactory _connectionFactory;
        private readonly INatsReceiverCommandQueueFactory _queueFactory;
        private readonly INatsCommandReplyFactory _replyFactory;

        private IEncodedConnection _connection;
        private IEncodedConnection _responeConnection;
        private IAsyncSubscription _subscription;

        public CommandsMessageRecieverHostedService(
            INatsConnectionFactory connectionFactory, INatsReceiverCommandQueueFactory queueFactory, ILogService logService,
            INatsCommandReplyFactory replyFactory) : base(logService)
        {
            _connectionFactory = connectionFactory;
            _queueFactory = queueFactory;
            _replyFactory = replyFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await WrapStarter(() =>
            {
                _connection = _connectionFactory.Get<CommandNatsAdapter>();
                _responeConnection = _connectionFactory.Get<ResultAdapter>();
            }, cancellationToken);

            var commandQueue = _queueFactory.Get();
            _subscription = _connection.SubscribeAsync(commandQueue.Key, commandQueue.Value, CreateHandler());
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            LogInfo("Stopped!");
            
            _connection.Dispose();
            _subscription.Dispose();
            _responeConnection.Dispose();
            return Task.CompletedTask;
        }
        
        private EventHandler<EncodedMessageEventArgs> CreateHandler()
        {
            return async (sender, args) =>
            {
                var response = (CommandNatsAdapter) args.ReceivedObject;
                Result result;
                try
                {
                    var data = _replyFactory.Get(response);
                    LogDebug($"accepted msg: {data}");

                    result = await _replyFactory.ExecuteCmd(data.GetCommand());
                }
                catch (Exception ex)
                {
                    var err = $"Error from handler {ex.Message}";
                    LogError(ex, err);
                    result = Result.Failure(err);
                }
                
                SendResult(response.Reply, result);
            };
        }
        
        private void SendResult(string reply, Result result)
        {
            try
            {
                _responeConnection.Publish(reply, new ResultAdapter
                {
                    IsSuccess = result.IsSuccess,
                    Data = result.IsSuccess ? string.Empty : result.Error
                });
                _responeConnection.Flush();
            }
            catch (Exception ex)
            {
                LogError(ex, "Error when publish result");
            }
        }
    }
}