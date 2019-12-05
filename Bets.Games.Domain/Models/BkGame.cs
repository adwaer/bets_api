using System;
using Bets.Games.Domain.Enums;

namespace Bets.Games.Domain.Models
{
    public class BkGame
    {
        public string Total { get; set; }
        public string TotalKef { get; set; } 
            
        public string Hc { get; set; }
        public string HcKef { get; set; }

        public Bookmaker Bookmaker { get; set; }
        public DateTime Date { get; set; }
        
        public string Team1 { get; set; }
        public string Team2 { get; set; }
    }
}
