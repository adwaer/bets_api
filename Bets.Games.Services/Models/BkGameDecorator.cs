using System;
using Bets.Games.Domain.Models;

namespace Bets.Games.Services.models
{
    public class BkGameDecorator
    {
        public Guid Id = Guid.NewGuid(); 
        public BkGame BkGame { get; }
        public DateTime UpdateDate { get; private set; }

        public BkGameDecorator(BkGame bkGame)
        {
            BkGame = bkGame;
            UpdateDate = DateTime.UtcNow;
        }

        public void Update(BkGame newData)
        {
            BkGame.PartsScore = newData.PartsScore;
            BkGame.SecondsPassed = newData.SecondsPassed;
            BkGame.Total = newData.Total;
            BkGame.TotalKef = newData.TotalKef;
            BkGame.Hc = newData.Hc;
            BkGame.HcKef = newData.HcKef;
            UpdateDate = DateTime.UtcNow;
        }

    }
}
