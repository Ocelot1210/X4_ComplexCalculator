using Collections.Pooled;
using System;

/// <summary>
/// <typeparamref name="TValue"/> が <see cref="IDisposable"/> な場合、自動で Dispose してくれる <see cref="PooledDictionary{TKey, TValue}"/>
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
internal class PooledDisposableDictionary<TKey, TValue> : PooledDictionary<TKey, TValue> , IDisposable
    where TKey : notnull 
    where TValue : IDisposable
{

    public new void Dispose()
    {
        foreach (var value in Values)
        {
            value.Dispose();
        }
        base.Dispose();
    }
}