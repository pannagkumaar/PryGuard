using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace PryGuard.Services.Helpers;

public static class RandomNumber
{
    private static readonly RNGCryptoServiceProvider _generator = new RNGCryptoServiceProvider();

    public static int Between(int minimumValue, int maximumValue)
    {
        byte[] data = new byte[1];
        _generator.GetBytes(data);
        double num = Math.Floor(Math.Max(0.0, Convert.ToDouble(data[0]) / byte.MaxValue - 1E-11) * (maximumValue - minimumValue + 1));
        return (int)(minimumValue + num);
    }

    public static T GetRandValue<T>(this IList<T> list)
    {
        if (list == null)
            throw new ArgumentNullException("list == null");
        if (list.Count == 1)
            return list[0];
        return list[Between(0, list.Count - 1)];
    }

    public static TValue GetRandValue<TKey, TValue>(this IDictionary<TKey, TValue> list)
    {
        if (list == null)
            throw new ArgumentNullException("list == null");
        int num = Between(0, list.Count - 1);
        List<TKey> list1 = list.Keys.ToList();
        for (int index = 0; index < list1.Count; ++index)
        {
            if (index == num)
            {
                TKey key = list1[index];
                TValue obj;
                if (list.TryGetValue(key, out obj))
                    return obj;
                throw new ArgumentException("Error retrieving from dictionary");
            }
        }
        return default(TValue);
    }
}
