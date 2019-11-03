using System.Collections.Generic;
using In.DataAccess.Entity;

namespace Bets.Games.Domain.Models
{
    public class Team : HasKey
    {
        public string DisplayName { get; set; }
        public ICollection<BkTeam> BkTeams { get; set; }
    }
}