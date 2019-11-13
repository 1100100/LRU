using System;
using System.Collections.Generic;

namespace LRU
{
    public interface ILRU<TKey, TValue> : IDisposable
    {
        bool Set(TKey key, TValue value);

        TValue Get(TKey key);

        bool ContainsKey(TKey key);

        ICollection<TKey> Keys();

        Dictionary<TKey, TValue> Values();

        int Count();
    }
}
