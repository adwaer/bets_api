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
using In.Cqrs.Nats.Abstract;
using In.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;

namespace Bets.ParserHost.HostedServices
{
    public class WinlineParserHostedService : BetsParserHostedService
    {
        private readonly string[] _groups = {"NCAA", "NBA"};

        private const string Js = @"
window.scrollTo(0,document.body.scrollHeight); setTimeout(() => window.scrollTo(0,0));
return angular.element(document.querySelector('.events .table')).scope().filteredChampionships.filter(x => x.idSport == arguments[0] && arguments[1].indexOf(x.name) != -1)
    .map(item => ({group: item.name, games: item.events}));";

        public WinlineParserHostedService(ILogService logger, INatsConnectionFactory connectionFactory,
            IOptions<ParsingSettings> options, ThreadProvider threadProvider)
            : base(logger, connectionFactory, threadProvider, By.Id("sticky-header-bottom-sticky-wrapper"),
                options.Value, options.Value.Winline)
        {
        }

        protected override Task<List<BkGame>> FetchGames()
        {
            return Task.Run(() =>
            {
                var webDriver = WebDriver.GetPage();
                var v = webDriver.ExecuteJavaScript<IReadOnlyCollection<object>>(Js, GetSportTypeArg(), _groups);

                var games = v
                    .OfType<Dictionary<string, object>>()
                    .ToArray();

                var foundGames = new List<BkGame>();
                var waiters = GetWaiters(games.Length);

                if (games.Length <= 0) return foundGames;

                for (var i = 0; i < games.Length; i++)
                {
                    var node = games[i];
                    var waiter = waiters[i];

                    ThreadProvider.Run(() =>
                    {
                        var parsedGames = ParseGames(node);
                        if (parsedGames.Any())
                        {
                            lock (foundGames)
                            {
                                foundGames.AddRange(parsedGames);
                            }
                        }
                    }, () => waiter.Set());
                }

                WaitHandle.WaitAll(waiters, TimeSpan.FromMinutes(1));
                return foundGames;
            });
        }

        private BkGame[] ParseGames(Dictionary<string, object> champ)
        {
            var group = champ["group"].ToString();
            var games = (IEnumerable<object>) champ["games"];

            return games
                .OfType<Dictionary<string, object>>()
                .Select(Parse)
                .Where(src => src != null)
                .ToArray();

            BkGame Parse(Dictionary<string, object> game)
            {
                var eventName = string.Join(" — ", ((IEnumerable<object>) game["members"]).Cast<string>());

                object hc = null, total = null;
                if ((bool) game["hasTotal"])
                {
                    game.TryGetValue("userSelectedForaValue", out hc);
                }

                if ((bool) game["hasFora"])
                {
                    game.TryGetValue("userSelectedTotalValue", out total);
                }

                if (!game.TryGetValue("scores", out var scores))
                {
                    return null;
                }

                var partsScore = ((IEnumerable<object>) scores)
                    .Select(score =>
                    {
                        var str = ((string) score).Trim();
                        if (string.IsNullOrEmpty(str))
                        {
                            str = "0:0";
                        }

                        return str;
                    });

                return new BkGame
                {
                    EventName = eventName,
                    Group = group,
                    Bookmaker = Bookmaker.Winline,
                    Hc = hc?.ToString().Trim('+', '-', '−'),
                    Total = total?.ToString(),
                    SecondsPassed = ParseTime(game),
                    PartsScore = partsScore
                };
            }

            int ParseTime(Dictionary<string, object> game)
            {
                var seconds = -1;
                var idSport = (long) game["idSport"];
                if (idSport.Equals(2)) // basket
                {
                    var time = game["sourceTime"]?.ToString(); //2Пол. 3:22 //2Ч 6-17 //1Т 27 //paused
                    if (!string.IsNullOrEmpty(time))
                    {
                        try
                        {
                            var timeSplit = time.Split(' ', ':', '-');
                            if (timeSplit.Length <= 1)
                            {
                                return -1;
                            }

                            var partTime = timeSplit[0];
                            var partType = partTime.Substring(1);

                            int partMultiplierMinutes;
                            if (group.Equals("США: NBA"))
                            {
                                partMultiplierMinutes = 12;
                            }
                            else if (!timePartMultiplier.TryGetValue(partType, out partMultiplierMinutes))
                            {
                                return -1;
                            }

                            if (!int.TryParse(partTime[0].ToString(), out var partCounter))
                            {
                                return -1;
                            }

                            var minutes = int.Parse(timeSplit[1].Trim('\''));
                            seconds = 0;
                            if (timeSplit.Length > 2)
                            {
                                seconds = int.Parse(timeSplit[2].Trim('\''));
                            }

                            var partMultiplierSeconds = partMultiplierMinutes * 60;
                            seconds = partMultiplierSeconds - (minutes * 60 + seconds); // time long - xm:xs
                            seconds += (partCounter - 1) * partMultiplierSeconds;
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    }
                }

                return seconds;
            }
        }

        private int GetSportTypeArg()
        {
            switch (ParsingSettings.SportType)
            {
                case SportType.Basketball:
                    return 2;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static Dictionary<string, int> timePartMultiplier = new Dictionary<string, int>
        {
            {"Пол.", 20},
            {"П", 20},
            {"Ч", 10},
            {"Т", 45}
        };
    }
}