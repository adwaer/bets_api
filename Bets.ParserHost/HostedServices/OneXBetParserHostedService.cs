using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bets.Games.Domain.Enums;
using Bets.Games.Domain.Models;
using Bets.ParserHost.Config;
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
        
        public OneXBetParserHostedService(ILogService logService, INatsConnectionFactory connectionFactory,
            IOptions<ParsingSettings> options, ThreadProvider threadProvider)
            : base(logService, connectionFactory, threadProvider, By.Id("games_content"), options)
        {
            _document = new HtmlDocument();
        }

        protected override IEnumerable<BkGame> FetchGames()
        {
            var webDriver = WebDriver.GetPage();
            var html = webDriver.ExecuteJavaScript<string>("return document.getElementById('games_content').innerHTML");

            _document.LoadHtml(html);
            var htmlNodes = _document.DocumentNode
                .SelectNodes("//div[@class='c-events__item c-events__item_col']");

            var foundGames = new List<BkGame>(htmlNodes.Count);
            var waiters = new WaitHandle[htmlNodes.Count];

            if (htmlNodes.Count <= 0) return foundGames;

            for (var i = 0; i < htmlNodes.Count; i++)
            {
                var node = htmlNodes[i];

                var waiter = new ManualResetEvent(false);
                waiters[i] = waiter;

                ThreadProvider.Run(() => foundGames.Add(ParseGame(node)), () => waiter.Set());
            }

            WaitHandle.WaitAll(waiters, TimeSpan.FromMinutes(1));

            return foundGames;
        }

        private BkGame ParseGame(HtmlNode node)
        {
            var teams = node.SelectNodes("div/div[@class='c-events-scoreboard']/div/a/span/div/div")
                ?.Select(x => x.InnerText)
                .ToArray();

            if (teams?.Length != 2)
            {
                return null;
            }

            var bets = node.SelectSingleNode("div/div[@class='c-bets']").ChildNodes;

            var hc = bets[7].InnerText.Trim();
            var hcKef1 = bets[6].InnerText.Trim();
            var hcKef2 = bets[8].InnerText.Trim();
            
            var total = bets[4].InnerText.Trim();
            var totalKef1 = bets[3].InnerText.Trim();
            var totalKef2 = bets[5].InnerText.Trim();
            
            return new BkGame
            {
                Team1 = teams[0],
                Team2 = teams[1],
                Bookmaker = Bookmaker.OneXBet,
                Date = DateTime.UtcNow,
                Hc = hc,
                HcKef = string.IsNullOrEmpty(hcKef1) ? hcKef2 : hcKef1,
                Total = total,
                TotalKef = string.IsNullOrEmpty(totalKef1) ? totalKef2 : totalKef1
            };
        }
    }
}
