using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Towermap;

public static class DictionaryUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TValue FindValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out bool exists)
    {
        ref var val = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out exists);
        return ref val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        ref var valueOrNullRef = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out bool exists);
        if (exists)
        {
            valueOrNullRef = value;
        }

        return valueOrNullRef;
    }
}