using System.Collections.Generic;
using Bets.Games.Domain.Enums;

namespace Bets.Games.Domain.Models
{
    public class BkGame
    {
        public string EventName { get; set; }
        public string Group { get; set; }
        public Bookmaker Bookmaker { get; set; }

        public int SecondsPassed { get; set; }
        public IEnumerable<string> PartsScore { get; set; }
        
        public string Total { get; set; }
        public string TotalKef { get; set; } 
            
        public string Hc { get; set; }
        public string HcKef { get; set; }
        
    }
}
