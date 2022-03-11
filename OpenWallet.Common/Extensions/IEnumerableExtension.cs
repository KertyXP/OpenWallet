using System;
using System.Collections.Generic;

namespace OpenWallet.Common
{
    public static class IEnumerableExtension
    {

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> oAction)
        {
            foreach(var oEnum in enumerable)
            {
                oAction.Invoke(oEnum);
            }
            return enumerable;
        }

    }
}
