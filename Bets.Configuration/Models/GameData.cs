using System.Collections.Generic;
using System.Linq;
using Bets.Games.Domain.Models;

namespace Bets.Configuration.Models
{
    public class GameData
    {
        public List<BkGameViewModel> Games { get; set; }

        public GameData()
        {
            Games = new List<BkGameViewModel>();
        }

        public void Set(BkGame bkGame)
        {
            var game = Games.FirstOrDefault(g => g.Bookmaker == bkGame.Bookmaker);
            if (game == null)
            {
                game = new BkGameViewModel(bkGame.Bookmaker);
                Games.Add(game);
            }
            else
            {
                game.Update(bkGame);
            }
        }
    }
}
