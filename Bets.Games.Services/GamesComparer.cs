using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bets.Games.Domain.Models;
using Bets.Games.Services.models;
using FuzzyStrings;

namespace Bets.Games.Services
{
    public static class GamesComparer
    {
        public static IEnumerable<BkGameDecorator> FindBkGames(BkGame bkGame, IEnumerable<BkGameDecorator> src,
            Game game = null)
        {
            return src.Where(existsGame =>
            {
                var existsBkGame = existsGame.BkGame;
                return bkGame.EventName == existsBkGame.EventName ||
                       IsSameStats(existsGame.BkGame, bkGame, game);
            });
        }

        public static IEnumerable<BkGameDecorator> CheckGamesStats(BkGame bkGame, IEnumerable<BkGameDecorator> src,
            Game game)
        {
            return src.Where(existsGame => IsSameStats(existsGame.BkGame, bkGame, game));
        }

        private static bool IsSameNames(string game1Name, string game2Name)
        {
            if (game1Name == game2Name) return true;
            var game1Split = game1Name.Split('-', StringSplitOptions.RemoveEmptyEntries);
            var game2Split = game2Name.Split('-', StringSplitOptions.RemoveEmptyEntries);

            var game1Team1Words = game1Split[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var game1Team2Words = game1Split[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            var game2Team1Words = game2Split[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var game2Team2Words = game2Split[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return (IsSameWords(game1Team1Words, game2Team1Words) || IsSameWords(game1Team1Words, game2Team2Words)) &&
                   (IsSameWords(game1Team2Words, game2Team1Words) || IsSameWords(game1Team2Words, game2Team2Words));
                
            bool IsSameWords(string[] words1, string[] words2)
            {
                return words1.Any(words2.Contains);
            }
        }

        private static bool IsSameStats(BkGame existsBkGame, BkGame bkGame, Game game)
        {
            if (bkGame.SecondsPassed < 100 || bkGame.PartsScore.Count() == 1 && bkGame.PartsScore.First().Equals("0:0"))
            {
                return false;
            }

            return existsBkGame.Group.Equals(bkGame.Group, StringComparison.CurrentCultureIgnoreCase) &&
                   IsPartsEqual(existsBkGame, bkGame) &&
                   IsInsidePeriod(game, bkGame, existsBkGame);
        }

        private static bool IsInsidePeriod(Game game, BkGame bksGame, BkGame existsBkGame)
        {
            if (game == null)
            {
                return existsBkGame.SecondsPassed - 50 <= bksGame.SecondsPassed &&
                       existsBkGame.SecondsPassed + 50 >= bksGame.SecondsPassed;
            }

            return game.SecondsMin <= bksGame.SecondsPassed &&
                   game.SecondsMax >= bksGame.SecondsPassed;
        }

        private static bool IsPartsEqual(BkGame existsBkGame, BkGame newBkGame)
        {
            var existsGameParts = existsBkGame.PartsScore.ToList();
            var newGameParts = newBkGame.PartsScore.ToList();
            var count = newGameParts.Count;

            if (count != existsGameParts.Count)
            {
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                var existsPart = existsGameParts[i];
                var newPart = newGameParts[i];

                if (!newPart.Equals(existsPart, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (i == count - 1)
                    {
                        try
                        {
                            var existsSplit = existsPart.Split(':');
                            var newSplit = newPart.Split(':');
                            var existsOne = int.Parse(existsSplit[0]);
                            var existsTwo = int.Parse(existsSplit[1]);
                            var newOne = int.Parse(newSplit[0]);
                            var newTwo = int.Parse(newSplit[1]);

                            return existsOne - 5 <= newOne && existsOne + 5 >= newOne &&
                                   existsTwo - 5 <= newTwo && existsTwo + 5 >= newTwo;
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    }

                    return false;
                }
            }

            return true;
        }

        private static bool IsSameTeams(BkGame existsBkGame, BkGame newBkGame)
        {
            var dice = existsBkGame.EventName
                .DiceCoefficient(newBkGame.EventName);

            int leven = existsBkGame.EventName
                .LevenshteinDistance(newBkGame.EventName);

            return dice > 0.6 && leven < 25;
        }
    }
}