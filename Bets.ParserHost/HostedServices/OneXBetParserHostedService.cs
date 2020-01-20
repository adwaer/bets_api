using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bets.Configuration;
using Bets.Configuration.Enums;
using Bets.Games.Domain.Enums;
using Bets.Games.Domain.Models;
using Bets.ParserHost.Helpers;
using HtmlAgilityPack;
using In.Cqrs.Nats.Abstract;
using In.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;

namespace Bets.ParserHost.HostedServices
{
    public class OneXBetParserHostedService : BetsParserHostedService
    {
        private readonly HtmlDocument _document;
        private readonly string[] _groups = {"NCAA", "NBA"};

        public OneXBetParserHostedService(ILogService logger, INatsConnectionFactory connectionFactory,
            IOptions<ParsingSettings> options, ThreadProvider threadProvider)
            : base(logger, connectionFactory, threadProvider, By.Id("games_content"),
                options.Value, options.Value.OneXBet)
        {
            _document = new HtmlDocument();
        }

        protected override Task<List<BkGame>> FetchGames()
        {
            return Task.Run(() =>
            {
                var webDriver = WebDriver.GetPage();
                var html = webDriver.ExecuteJavaScript<string>(
                    "return document.getElementById('games_content').innerHTML");

                _document.LoadHtml(html);
                var htmlNodes = _document.DocumentNode
                    .SelectNodes("//div[@data-name='dashboard-champ-content']")
                    .Where(node => _groups.Contains(node.SelectSingleNode("div/div[@class='c-events__name']").InnerText))
                    .ToArray();

                var foundGames = new List<BkGame>();
                var waiters = GetWaiters(htmlNodes.Length);

                if (htmlNodes.Length <= 0) return foundGames;

                for (var i = 0; i < htmlNodes.Length; i++)
                {
                    var node = htmlNodes[i];
                    var waiter = waiters[i];

                    ThreadProvider.Run(() =>
                    {
                        var games = ParseGames(node);

                        if (games.Any())
                        {
                            lock (foundGames)
                            {
                                foundGames.AddRange(games);
                            }
                        }
                    }, () => waiter.Set());
                }

                WaitHandle.WaitAll(waiters, 60000);
                return foundGames;
            });
        }

        private BkGame[] ParseGames(HtmlNode champNode)
        {
            var groupNode = champNode.SelectSingleNode("div[contains(@class, 'c-events__item_head')]");
            var group = groupNode.SelectSingleNode("div[@class='c-events__name']/a").InnerText;

            var nodes = champNode.SelectNodes("div[@class='c-events__item c-events__item_col']");
            return nodes.Select(Parse)
                .Where(src => src != null)
                .ToArray();

            BkGame Parse(HtmlNode node)
            {
                var scoreNode = node.SelectSingleNode("div/div[@class='c-events-scoreboard']");
                if (scoreNode == null)
                {
                    return null;
                }

                var eventName = scoreNode.SelectSingleNode("div//span[@class='c-events__teams']")
                    .GetAttributeValue("title", null)
                    .Split(" с ОТ")[0];
                
                if (eventName.Contains("Статистика"))
                {
                    return null;
                }

                if (string.IsNullOrEmpty(eventName))
                {
                    return null;
                }

                var betsNode = node.SelectSingleNode("div/div[@class='c-bets']").ChildNodes;

                var hc = betsNode[7].InnerText.Trim().Trim('+', '-');
                var hcKef1 = betsNode[6].InnerText.Trim();
                var hcKef2 = betsNode[8].InnerText.Trim();

                var total = betsNode[4].InnerText.Trim();
                var totalKef1 = betsNode[3].InnerText.Trim();
                var totalKef2 = betsNode[5].InnerText.Trim();

                var time = scoreNode.SelectSingleNode("div//div[@class='c-events__time']/span")?.InnerText;
                if (time == null)
                {
                    return null;
                }

                var timeSplit = time.Split(':');

                var seconds = -1;
                try
                {
                    if (timeSplit.Length == 2)
                    {
                        seconds = int.Parse(timeSplit[0]) * 60 + int.Parse(timeSplit[1]);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }

                var parts = scoreNode.SelectNodes("div//span[@class='c-events-scoreboard__cell']")
                    ?.Select(cell => cell.InnerText)
                    .ToArray();

                if (parts == null)
                {
                    return null;
                }

                var partsScoreBuilder = new List<string>();
                var partCount = parts.Length / 2;
                for (int i = 0; i < partCount; i++)
                {
                    var leftScore = parts[i];
                    var rightScore = parts[i + partCount];
                    if (partCount - 1 == i && leftScore.Equals("0") && rightScore.Equals("0"))
                    {
                        continue;
                    }
                    partsScoreBuilder.Add($"{parts[i]}:{parts[i + partCount]}");
                }

                return new BkGame
                {
                    SecondsPassed = seconds,
                    PartsScore = partsScoreBuilder,
                    EventName = eventName.Replace('—', '-'),
                    Group = group,
                    Bookmaker = Bookmaker.OneXBet,
                    Hc = hc,
                    HcKef = string.IsNullOrEmpty(hcKef1) ? hcKef2 : hcKef1,
                    Total = total,
                    TotalKef = string.IsNullOrEmpty(totalKef1) ? totalKef2 : totalKef1
                };
            }
        }
        
        private string GetSportTypeArg()
        {
            switch (ParsingSettings.SportType)
            {
                case SportType.Basketball:
                    return "Basketball/";
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}