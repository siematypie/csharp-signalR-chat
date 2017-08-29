using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Chatappwow.Models
{
    public class ThreadSafeSet<T>
    {
        private readonly HashSet<T> _set = new HashSet<T>();
        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();

        public void Add(T item)
        {
            _cacheLock.EnterWriteLock();
            try
            {
                _set.Add(item);
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        public void Remove(T item)
        {
            _cacheLock.EnterWriteLock();
            try
            {
                _set.Remove(item);
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        public IEnumerable<T> AllValues
        {
            get
            {
                _cacheLock.EnterReadLock();
                try
                {
                    foreach (T item in _set)
                    {
                        yield return item;
                    }
                }
                finally
                {
                    _cacheLock.ExitReadLock();
                }
            }
        }

    }
}