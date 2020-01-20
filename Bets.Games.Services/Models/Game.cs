using System.Collections.Generic;
using System.Linq;
using Bets.Games.Domain.Models;

namespace Bets.Games.Services.models
{
    public class Game
    {
        public string Group { get; }
        public List<BkGameDecorator> BkGames { get; set; } = new List<BkGameDecorator>();
        public int SecondsMin { get; set; }
        public int SecondsMax { get; set; }

        public Game(string group)
        {
            Group = group;
        }

        public bool IsSameEvent(BkGame bkGame)
        {
            return GamesComparer
                .FindBkGames(bkGame, BkGames, this)
                .Any();
        }

        public void AddBkGame(BkGame bkGame)
        {
            var gameDec = new BkGameDecorator(bkGame);
            BkGames.Add(gameDec);
        }

        public void AddBkGame(BkGameDecorator bkGame)
        {
            BkGames.Add(bkGame);
        }

        public void UpdateBkGame(BkGame bkGame)
        {
            var foundGame = BkGames.FirstOrDefault(bg => bg.BkGame.Bookmaker == bkGame.Bookmaker);
            if (foundGame == null)
            {
                AddBkGame(bkGame);
            }
            else
            {
                Update(foundGame, bkGame);
            }
        }

        private void Update(BkGameDecorator bkGame, BkGame newData)
        {
            bkGame.Update(newData);
            SecondsMin = bkGame.BkGame.SecondsPassed - 50;
            SecondsMax = bkGame.BkGame.SecondsPassed + 50;
        }
    }
}