using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bets.Configuration;
using Bets.Games.Domain.Models;
using Bets.Games.Domain.MQMsgs;
using Bets.ParserHost.Helpers;
using Bets.Selenium;
using In.Cqrs.Nats.Abstract;
using In.Logging;
using Microsoft.Extensions.Hosting;
using NATS.Client;
using OpenQA.Selenium;

namespace Bets.ParserHost.HostedServices
{
    public abstract class BetsParserHostedService : BackgroundService
    {
        private readonly INatsConnectionFactory _connectionFactory;
        private readonly By _waitBy;
        private readonly ParsingBookmakerSettings _settings;

        private IEncodedConnection _connection;

        private ILogService LogService { get; }
        protected ThreadProvider ThreadProvider { get; }
        protected WebDriverWrap WebDriver;
        protected readonly ParsingSettings ParsingSettings;

        protected BetsParserHostedService(ILogService logger, INatsConnectionFactory connectionFactory,
            ThreadProvider threadProvider, By waitBy, ParsingSettings parsingSettings,
            ParsingBookmakerSettings bkSettings)
        {
            _connectionFactory = connectionFactory;
            _waitBy = waitBy;
            ParsingSettings = parsingSettings;
            _settings = bkSettings;

            LogService = logger;
            ThreadProvider = threadProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.Enabled)
            {
                return;
            }

            LogInfo("Starting..");

            _connection = _connectionFactory.Get<BkMqMessage>();

            WebDriver = DriverFactory.GetNewDriver(_settings.Driver);
            WebDriver.Open(_settings.Url, _waitBy);

            var sw = new Stopwatch();
            sw.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                var games = await FetchGames();
                if (!games.Any())
                {
                    continue;
                }

                SendResult(new BkMqMessage(games));

                sw.Restart();
            }

            LogInfo("Started");
        }

        protected abstract Task<List<BkGame>> FetchGames();

        private readonly List<ManualResetEvent> _eventSlims = new List<ManualResetEvent>();

        protected ManualResetEvent[] GetWaiters(int count)
        {
            IEnumerable<ManualResetEvent> waiters;
            if (_eventSlims.Count < count)
            {
                _eventSlims.AddRange(new ManualResetEvent[count - _eventSlims.Count]
                    .Select(w => new ManualResetEvent(false)));
                waiters = _eventSlims;
            }
            else
            {
                waiters = _eventSlims.Take(count);
            }

            return waiters
                .Select(w =>
                {
                    w.Reset();
                    return w;
                })
                .ToArray();
        }

        private void SendResult(BkMqMessage msg)
        {
            try
            {
                _connection.Publish(ParsingSettings.QueueSubject, msg);
                _connection.Flush();
            }
            catch (Exception ex)
            {
                LogError(ex, "Error when publish result");
            }
        }

        private void LogInfo(string msg)
        {
            LogService.LogInfo(GetType().ToString(), msg);
        }

        private void LogError(Exception ex, string msg)
        {
            LogService.LogError(GetType().ToString(), ex, msg);
        }
    }
}