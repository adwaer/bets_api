using System.Collections.Generic;
using System.Threading;
using Bets.Configuration.Models;
using Bets.Games.Domain.Models;

namespace Bets.Configuration.Services.Implementations
{
    public class GamesService
    {
        private Dictionary<GameKey, GameData> _data;
        private ReaderWriterLockSlim _lockSlim;

        public GamesService()
        {
            _data = new Dictionary<GameKey, GameData>();
            _lockSlim = new ReaderWriterLockSlim();
        }

        public void SetGame(BkGame bkGame)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                var key = GameKey.FromBkGame(bkGame);
                _data[key].Set(bkGame);
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }
    }
}