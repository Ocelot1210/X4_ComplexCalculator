using Collections.Pooled;
using System;

namespace X4_ComplexCalculator.DB.X4DB.Manager;


sealed class TagsManager<T>(int capacity, Func<string, T> converter) : IDisposable
{
    private readonly PooledDictionary<string, T> _tags = new(capacity);


    private readonly Func<string, T> _converter = converter;


    public T this[string str]
    {
        get
        {
            if (!_tags.TryGetValue(str, out var ret))
            {
                ret = _converter(str);
                _tags.Add(str, ret);
            }

            return ret;
        }
    }


    public void Dispose()
    {
        _tags.Dispose();
    }
}