using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

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
