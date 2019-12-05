using System.Collections.Generic;
using System.Threading.Tasks;
using In.Cqrs.Command;
using In.DataAccess.Repository.Abstract;
using In.Specifications;

namespace Bets.HandlersHost.Config
{
    public class TupleMessageHistoryStorage : IRepository<IMessageResult>
    {
        public Task<IEnumerable<IMessageResult>> Find(Specification<IMessageResult> specification)
        {
            throw new System.NotImplementedException();
        }

        public Task<IMessageResult> FindOne(Specification<IMessageResult> specification)
        {
            return null;
        }

        public void Add(IMessageResult data)
        {
            
        }

        public void Remove(IMessageResult data)
        {
        }

        public Task Save()
        {
            return Task.CompletedTask;
        }
    }
}