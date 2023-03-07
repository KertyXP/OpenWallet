using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace OpenWallet.Common
{
    public static class StringExtension
    {

        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }


        public static int ToInt(this string s, int nDefaultValue)
        {
            int result;
            if (string.IsNullOrEmpty(s) || !int.TryParse(s, out result))
                return nDefaultValue;
            return result;
        }


        public static double ToDouble(this string s)
        {
            var a = s.Split(',', '.', '\'', ' ').ToList();

            var sIntPart = a.FirstOrDefault();
            var sDecimalPart = String.Join("", a.Skip(1));

            string sCorrectString = string.IsNullOrEmpty(sDecimalPart) ? sIntPart : $"{sIntPart}.{sDecimalPart}";

            if (string.IsNullOrEmpty(sCorrectString))
                return 0;

            return Convert.ToDouble(sCorrectString, CultureInfo.InvariantCulture);
        }


        private static List<string> aSymbols = new List<string>(){
            "BUSDS", "USDC", "TUSD", "USDT", "BUSD", "BTC", "ETH", "BNB", "PAX"};

        public static CurrencySymbol AutoDiscoverCurrencySymbol(this string sSymbolToParse)
        {
            if (sSymbolToParse.Length == 6)
            {
                return new CurrencySymbol(sSymbolToParse.Substring(0, 3), sSymbolToParse.Substring(3, 3), sSymbolToParse);
            }
            foreach (var sSymbol in aSymbols)
            {
                var l = sSymbol.Length;
                if (sSymbolToParse.EndsWith(sSymbol))
                {
                    return new CurrencySymbol(sSymbolToParse.Substring(0, sSymbolToParse.Length - l), sSymbolToParse.Substring(sSymbolToParse.Length - l, l), sSymbolToParse);
                }
                if (sSymbolToParse.StartsWith(sSymbol))
                {
                    return new CurrencySymbol(sSymbolToParse.Substring(0, l), sSymbolToParse.Substring(l), sSymbolToParse);
                }
            }
            return null;
        }

    }
}
