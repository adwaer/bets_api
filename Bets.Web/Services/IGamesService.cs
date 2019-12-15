using System.Collections.Generic;
using System.Threading.Tasks;
using Bets.Games.Domain.Models;
using In.Logging;
using Microsoft.AspNetCore.SignalR;

namespace Bets.Web.Services
{
    public interface IGamesService
    {
        Task SendGames(IEnumerable<BkGame> games);
    }

    public class GamesHub : Hub<IGamesService>
    {
        private readonly ILogService _logService;

        public GamesHub(ILogService logService)
        {
            _logService = logService;
        }

        public async Task SendGamesToAllClients(IEnumerable<BkGame> games)
        {
            await Clients.All.SendGames(games);
        }
    }
}