using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    .map(item => ({group: item.name, games: item.events
        .map(e => {
			var game = {};
			if(e.state == 1 && e.hasTotal && e.sortTotal){
				try { var totalStat = e.mainLines[4][e.sortTotal[3]][1]; game.total = totalStat.line.koef; game.totalKef = totalStat.value; } catch{}
			}
			if(e.state == 1 && e.hasFora && e.sortFora){
				try {var foraStat = e.mainLines[3][e.sortFora[3]][1]; game.fora = foraStat.line.koefFora[0].substr(1); game.foraKef = foraStat.value;} catch{}
			}
			
			game.eventName = e.members.join(' - ');
			game.scores = (e.scores || []).map(score => {
				score = score.trim();
				if(!score) score = '0:0';
				return score;
			});
			
			game.idSport = e.idSport;
			game.sourceTime = e.sourceTime;
			return game;
        })}))
";


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
                try
                {
                    var webDriver = WebDriver.GetPage();
                    var jsResult =
                        webDriver.ExecuteJavaScript<IReadOnlyCollection<object>>(Js, GetSportTypeArg(), _groups);

                    var games = jsResult
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
                }
                catch (Exception ex)
                {
                    LogError(ex, "Parsing exception");
                    return new List<BkGame>();
                }
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
                var eventName = (string) game["eventName"];

                game.TryGetValue("fora", out var hc);
                game.TryGetValue("foraKef", out var hcKef);
                game.TryGetValue("total", out var total);
                game.TryGetValue("totalKef", out var totalKef);
                var scores = ((ReadOnlyCollection<object>) game["scores"]).Cast<string>();

                return new BkGame
                {
                    EventName = eventName,
                    Group = group,
                    Bookmaker = Bookmaker.Winline,
                    Hc = hc?.ToString().Trim('+', '-', '−'),
                    HcKef = hcKef?.ToString(),
                    Total = total?.ToString(),
                    TotalKef = totalKef?.ToString(),
                    SecondsPassed = ParseTime(game),
                    PartsScore = scores
                };
            }

            int ParseTime(Dictionary<string, object> game)
            {
                var seconds = -1;
                var idSport = (long) game["idSport"];
                if (idSport.Equals(2)) // basket
                {
                    var time = game["sourceTime"]?.ToString(); //2Пол. 3:22 //2Ч 6-17 //1Т 27 //paused / Пер.2 
                    if (!string.IsNullOrEmpty(time))
                    {
                        try
                        {
                            if (time.StartsWith("Пер."))
                            {
                                if (int.TryParse(time.Split('.')[1], out var halfmultiplier))
                                {
                                    if (group.Equals("NBA"))
                                    {
                                        return 12 * halfmultiplier;
                                    }

                                    return 10 * halfmultiplier;
                                }
                            }

                            var timeSplit = time.Split(' ', ':', '-');
                            if (timeSplit.Length <= 1)
                            {
                                return -1;
                            }

                            var partTime = timeSplit[0];
                            var partType = partTime.Substring(1);

                            int partMultiplierMinutes;
                            if (group.Equals("NBA"))
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