using Bets.Games.Domain.Models;

namespace Bets.Configuration.Models
{
    public class GameKey
    {
        public string Team1 { get; }
        public string Team2 { get; }

        public GameKey(string team1, string team2)
        {
            Team1 = team1;
            Team2 = team2;
        }

        public static GameKey FromBkGame(BkGame bkGame)
        {
            return new GameKey(bkGame.Team1, bkGame.Team2);
        }

        protected bool Equals(GameKey other)
        {
            return Team1 == other.Team1 && Team2 == other.Team2;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((GameKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Team1 != null ? Team1.GetHashCode() : 0) * 397) ^ (Team2 != null ? Team2.GetHashCode() : 0);
            }
        }
    }
}