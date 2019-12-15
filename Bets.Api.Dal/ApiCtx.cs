using Bets.Games.Domain.Models;
using In.DataAccess.Mongo;
using MongoDB.Driver;

namespace Bets.Api.Dal
{
    public class ApiCtx : IMongoCtx
    {
        private readonly IMongoDatabase _database;

        public ApiCtx(string connectionString)
        {
            var connection = new MongoUrlBuilder(connectionString);
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(connection.DatabaseName);
        }
        
        public IMongoDatabase Db => _database;

        public IMongoCollection<SimpleMessageResult> SimpleMessageResults =>
            _database.GetCollection<SimpleMessageResult>(nameof(SimpleMessageResult));
    }
}