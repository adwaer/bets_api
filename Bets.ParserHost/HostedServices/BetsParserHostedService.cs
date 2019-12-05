using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bets.Games.Domain.Models;
using Bets.Games.Domain.MQMsgs;
using Bets.ParserHost.Config;
using Bets.ParserHost.Helpers;
using Bets.Selenium;
using In.Cqrs.Nats.Abstract;
using In.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NATS.Client;
using OpenQA.Selenium;

namespace Bets.ParserHost.HostedServices
{
    public abstract class BetsParserHostedService : IHostedService
    {
        private bool _stopped;
        private readonly INatsConnectionFactory _connectionFactory;
        protected readonly By WaitBy;
        private readonly ParsingBookmakerSettings _settings;

        private IEncodedConnection _connection;

        protected ILogService LogService { get; }
        protected ThreadProvider ThreadProvider { get; }
        protected WebDriverWrap WebDriver;

        protected BetsParserHostedService(ILogService logger, INatsConnectionFactory connectionFactory,
            ThreadProvider threadProvider, By waitBy,
            IOptions<ParsingSettings> options)
        {
            _connectionFactory = connectionFactory;
            WaitBy = waitBy;
            if (GetType() == typeof(OneXBetParserHostedService))
            {
                _settings = options.Value.OneXBet;
            }

            LogService = logger;
            ThreadProvider = threadProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_settings.Enabled)
            {
                _stopped = true;
                return Task.CompletedTask;
            }

            LogInfo("Starting..");

            _connection = _connectionFactory.Get<BkMqMessage>();

            WebDriver = DriverFactory.GetNewDriver(_settings.Driver);
            WebDriver.Open(_settings.Url, WaitBy);

            while (!_stopped)
            {
                var games = FetchGames();
                
                SendResult(new BkMqMessage(games));
            }

            LogInfo("Started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            LogInfo("Stopping..");
            _stopped = true;
            while (!_stopped)
            {
                Thread.Sleep(1000);
            }

            _connection.Dispose();

            LogInfo("Stopped");
            return Task.CompletedTask;
        }

        protected abstract IEnumerable<BkGame> FetchGames();

        private void SendResult(BkMqMessage msg)
        {
            try
            {
                _connection.Publish(_settings.QueueSubject, msg);
                _connection.Flush();
            }
            catch (Exception ex)
            {
                LogError(ex, "Error when publish result");
            }
        }

        protected void LogInfo(string msg)
        {
            LogService.LogInfo(GetType().ToString(), msg);
        }

        protected void LogError(Exception ex, string msg)
        {
            LogService.LogError(GetType().ToString(), ex, msg);
        }
    }
}