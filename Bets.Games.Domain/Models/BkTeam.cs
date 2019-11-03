using Bets.Games.Domain.Enums;
using In.DataAccess.Entity;

namespace Bets.Games.Domain.Models
{
    public class BkTeam: HasKey
    {
        public string Name { get; set; }
        public Bookmaker Bookmaker { get; set; }
    }
}