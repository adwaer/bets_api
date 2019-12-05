using In.DataAccess.Entity;

namespace Bets.Games.Domain.Models
{
    public class Team : HasKey
    {
        public string Name { get; set; }
        public string[] OtherNames { get; set; }
    }
}