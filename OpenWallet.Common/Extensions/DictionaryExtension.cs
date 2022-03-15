using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace OpenWallet.Common
{
    public static class DictionaryExtension
    {

        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key) where TValue : class, new()
        {
            if (dic.ContainsKey(key))
            {
                return dic[key];
            }
            return default(TValue);
        }

        public static void InsertOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value) where TValue : class, new()
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = value;
            }
            else
            {
                dic.Add(key, value);
            }
        }


    }
}
