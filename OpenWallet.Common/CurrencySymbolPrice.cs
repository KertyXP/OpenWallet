using System.Collections.Generic;
using System.Linq;

namespace OpenWallet.Common
{

    public class CurrencySymbol
    {
        public CurrencySymbol(string from, string to)
        {
            From = from;
            To = to;
        }

        public string From { get; set; }
        public string To { get; set; }

        private static List<string> aSymbols = new List<string>(){
            "BUSDS", "USDC", "TUSD", "USDT", "BUSD", "BTC", "ETH", "BNB", "PAX"};
        public static CurrencySymbol AutoDiscoverCurrencySymbol(string sSymbolToParse)
        {
            if (sSymbolToParse.Length == 6)
            {
                return new CurrencySymbol(sSymbolToParse.Substring(0, 3), sSymbolToParse.Substring(3, 3));
            }
            foreach (var sSymbol in aSymbols)
            {
                    var l = sSymbol.Length;
                if (sSymbolToParse.EndsWith(sSymbol))
                {
                    return new CurrencySymbol(sSymbolToParse.Substring(0, sSymbolToParse.Length - l), sSymbolToParse.Substring(sSymbolToParse.Length - l, l));
                }
                if(sSymbolToParse.StartsWith(sSymbol))
                {
                    return new CurrencySymbol(sSymbolToParse.Substring(0, l), sSymbolToParse.Substring(l));
                }
            }
            return null;
        }
    }

    public class CurrencySymbolPrice : CurrencySymbol
    {
        public CurrencySymbolPrice(string from, string to, double price) : base(from, to)
        {
            Price = price;
        }

        public double Price { get; set; }


    }

    public class CurrencySymbolPriceReverted : CurrencySymbolPrice
    {
        public CurrencySymbolPriceReverted(string from, string to, double price) : base(to, from, 1 / price)
        {
        }
    }

    public static class CurrencySymbolExtension
    {

        public static double GetBtcValue(this List<CurrencySymbolPrice> oCurrencies, string Crypto, double Value)
        {
            if (Crypto == "BTC")
                return Value;


            var oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == Crypto && o.To == "BTC");

            if (oCryptoFound != null)
            {
                return Value * oCryptoFound.Price;
            }

            var oCryptoFoundUsdtBtc = oCurrencies.FirstOrDefault(o => o.From == "USDT" && o.To == "BTC");
            oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == Crypto && o.To == "USDT");

            if (oCryptoFound != null)
            {
                return Value * oCryptoFound.Price * oCryptoFoundUsdtBtc.Price;
            }

            return 0;
        }
    }
}
