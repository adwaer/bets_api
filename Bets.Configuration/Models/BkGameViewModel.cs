using System;
using Bets.Games.Domain.Enums;
using Bets.Games.Domain.Models;

namespace Bets.Configuration.Models
{
    public class BkGameViewModel
    {
        public Bookmaker Bookmaker { get; private set; }

        public BkGameViewModel(Bookmaker bookmaker)
        {
            Bookmaker = bookmaker;
        }

        public string Total { get; set; }
        public string TotalKef { get; set; } 
            
        public string Hc { get; set; }
        public string HcKef { get; set; }
        
        public DateTime Date { get; set; }

        public void Update(BkGame bkGame)
        {
            Total = bkGame.Total;
            TotalKef = bkGame.TotalKef;
            Hc = bkGame.Hc;
            HcKef = bkGame.HcKef;
            Date = bkGame.Date;
        }
    }
}