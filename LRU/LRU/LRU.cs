using System;
using System.Collections.Generic;
using System.Threading;

namespace LRU
{
    public class LRU<TKey, TValue> : ILRU<TKey, TValue>
    {
        private ReaderWriterLockSlim Lock { get; }

        private Lazy<Dictionary<TKey, TValue>> KeyValues { get; }

        private Lazy<LinkedList<TKey>> List { get; }

        private int Size { get; }

        public LRU(int size)
        {
            Lock = new ReaderWriterLockSlim();
            KeyValues = new Lazy<Dictionary<TKey, TValue>>(() => new Dictionary<TKey, TValue>());
            List = new Lazy<LinkedList<TKey>>(() => new LinkedList<TKey>());
            Size = size;
        }

        public bool Set(TKey key, TValue value)
        {
            Lock.EnterWriteLock();
            try
            {
                if (KeyValues.Value.ContainsKey(key))
                {
                    KeyValues.Value[key] = value;
                    List.Value.Remove(key);
                    List.Value.AddFirst(key);
                }
                else
                {
                    KeyValues.Value[key] = value;
                    if (KeyValues.Value.Count >= Size)
                    {
                        List.Value.RemoveLast();
                    }
                    List.Value.AddFirst(key);
                }
            }
            finally
            {
                Lock.ExitWriteLock();
            }
            return true;
        }

        public TValue Get(TKey key)
        {
            Lock.EnterUpgradeableReadLock();
            try
            {
                var result = KeyValues.Value.TryGetValue(key, out var value);
                if (result)
                {
                    Lock.EnterWriteLock();
                    try
                    {
                        List.Value.Remove(key);
                        List.Value.AddFirst(key);
                    }
                    finally
                    {
                        Lock.ExitWriteLock();
                    }
                }
                return value;
            }
            finally
            {
                Lock.ExitUpgradeableReadLock();
            }
        }

        public bool ContainsKey(TKey key)
        {
            Lock.EnterReadLock();
            try
            {
                return KeyValues.Value.ContainsKey(key);
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        public ICollection<TKey> Keys()
        {
            Lock.EnterReadLock();
            try
            {
                return KeyValues.Value.Keys;
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        public Dictionary<TKey, TValue> Values()
        {
            Lock.EnterReadLock();
            try
            {
                return KeyValues.Value;
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        public int Count()
        {
            Lock.EnterReadLock();
            try
            {
                return KeyValues.Value.Count;
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        public void Dispose()
        {
            Lock?.Dispose();
        }
    }
}
