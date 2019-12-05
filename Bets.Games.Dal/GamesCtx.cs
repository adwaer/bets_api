using Bets.Games.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Bets.Games.Dal
{
    public class GamesCtx : DbContext
    {
        public GamesCtx(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Team> Teams { get; set; }
    }
}