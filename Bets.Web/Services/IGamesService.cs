using System.Collections.Generic;
using System.Threading.Tasks;
using Bets.Games.Services.models;
using In.Logging;
using Microsoft.AspNetCore.SignalR;

namespace Bets.Web.Services
{
    /// <summary>
    /// Game Service
    /// </summary>
    public interface IGamesService
    {
        /// <summary>
        /// Send games
        /// </summary>
        /// <param name="games"></param>
        /// <returns></returns>
        Task SendGames(IEnumerable<Game> games);
    }

    /// <inheritdoc />
    // ReSharper disable once ClassNeverInstantiated.Global
    public class GamesHub : Hub<IGamesService>
    {
        private readonly ILogService _logService;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logService"></param>
        public GamesHub(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Send games to client
        /// </summary>
        /// <param name="games"></param>
        /// <returns></returns>
        public async Task SendGamesToAllClients(IEnumerable<Game> games)
        {
            await Clients.All.SendGames(games);
        }
    }
}
