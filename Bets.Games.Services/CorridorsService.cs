using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bets.Games.Domain.Models;
using Bets.Games.Services.models;

namespace Bets.Games.Services
{
    public class CorridorsService : IDisposable
    {
        private bool _disposed;

        private readonly AsyncList<Game> _gameList = new AsyncList<Game>();
        private readonly AsyncList<BkGameDecorator> _freeBkGameList = new AsyncList<BkGameDecorator>();

        public CorridorsService()
        {
            ThreadPool.QueueUserWorkItem(subj =>
            {
                while (!_disposed)
                {
                    try
                    {
                        _gameList.Remove(game => game.BkGames.Count <= 1 || game.BkGames
                            .Max(bkGame => bkGame.UpdateDate.AddMinutes(2) < DateTime.UtcNow)
                        );

                        _freeBkGameList.Remove(bkGame => bkGame.UpdateDate.AddMinutes(2) < DateTime.UtcNow);
                        Thread.Sleep(TimeSpan.FromMinutes(3));
                    }
                    catch
                    {
                        // ignored
                    }
                }
            });
        }

        public Game[] AddBkGames(IEnumerable<BkGame> bkGames)
        {
            foreach (var bkGame in bkGames)
            {
                var game = _gameList.Get(g => g.IsSameEvent(bkGame));

                if (game == null)
                {
                    UpdateNotConnected(bkGame);
                }
                else
                {
                    lock (game)
                    {
                        game.UpdateBkGame(bkGame);
                    }
                }
            }

            return _gameList.Get();
        }

        private void UpdateNotConnected(BkGame bkGame)
        {
            var existBkGame = _freeBkGameList.Get(eg =>
                eg.BkGame.EventName == bkGame.EventName && bkGame.Bookmaker == eg.BkGame.Bookmaker);

            var existsOtherBkGames = GamesComparer.FindBkGames(bkGame, _freeBkGameList.Get())
                .Where(fg => fg.BkGame.Bookmaker != bkGame.Bookmaker)
                .ToArray();

            if (existsOtherBkGames.Any())
            {
                var game = new Game(bkGame.Group);
                game.AddBkGame(bkGame);
                foreach (var existsOtherBkGame in existsOtherBkGames)
                {
                    game.AddBkGame(existsOtherBkGame);
                }

                _gameList.Add(game);

                var ids = GamesComparer
                    .FindBkGames(bkGame, _freeBkGameList.Get())
                    .Select(g => g.Id);
                _freeBkGameList.Remove(decorator => ids.Contains(decorator.Id));
            }
            else if (existBkGame == null)
            {
                _freeBkGameList.Add(new BkGameDecorator(bkGame));
            }
            else
            {
                existBkGame.Update(bkGame);
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
