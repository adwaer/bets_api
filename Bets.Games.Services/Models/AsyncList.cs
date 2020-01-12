using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Bets.Games.Services.models
{
    public class AsyncList<T>
    {
        private readonly ReaderWriterLockSlim _rwl = new ReaderWriterLockSlim();
        private readonly List<T> _list = new List<T>();

        public void Add(T item)
        {
            _rwl.EnterWriteLock();
            try
            {
                _list.Add(item);
            }
            finally
            {
                _rwl.ExitWriteLock();
            }
        }

        public void Remove(Predicate<T> predicate)
        {
            _rwl.EnterWriteLock();
            try
            {
                _list.RemoveAll(predicate);
            }
            finally
            {
                _rwl.ExitWriteLock();
            }
        }

        public T Get(Func<T, bool> predicate)
        {
            _rwl.EnterReadLock();
            try
            {
                return _list.FirstOrDefault(predicate);
            }
            finally
            {
                _rwl.ExitReadLock();
            }
        }
        
        public T[] Get()
        {
            _rwl.EnterReadLock();
            try
            {
                return _list.ToArray();
            }
            finally
            {
                _rwl.ExitReadLock();
            }
        }
    }
}
