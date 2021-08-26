using System.Collections.Generic;
using System.Linq;

namespace OpenWallet.Common
{

    public class CurrencySymbol
    {
        public CurrencySymbol()
        {

        }
        public CurrencySymbol(string from, string to)
        {
            From = from;
            To = to;
        }

        public string CryptoFromId { get; set; }
        public string CryptoToId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Couple => string.Compare(From, To) > 0 ? $"{From}_{To}" : $"{To}_{From}";

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

    public class CurrencySymbolExchange : CurrencySymbol
    {
        public CurrencySymbolExchange()
        {

        }
        public CurrencySymbolExchange(string from, string to, string exchange) : base(from, to)
        {
            Exchange = exchange;
        }

        public string Exchange { get; set; }


    }

    public class CurrencySymbolPrice : CurrencySymbolExchange
    {
        public CurrencySymbolPrice()
        {

        }
        public CurrencySymbolPrice(string from, string to, double price, string exchange) : base(from, to, exchange)
        {
            Price = price;
        }

        public double Price { get; set; }


    }

    public class CurrencySymbolPriceReverted : CurrencySymbolPrice
    {
        public CurrencySymbolPriceReverted(string from, string to, double price, string exchange) : base(to, from, 1 / price, exchange)
        {
        }
    }

    public static class CurrencySymbolExtension
    {

        public static double GetBtcValue(this List<CurrencySymbolPrice> oCurrencies, GlobalBalance globalBalance) => oCurrencies.GetCustomValue(globalBalance, "BTC");
        public static double GetCustomValueFromBtc(this List<CurrencySymbolPrice> oCurrencies, GlobalBalance globalBalance, string sCrypto)
        {
            string sCryptoFrom = globalBalance.Crypto.ToUpper();

            if (sCryptoFrom == sCrypto)
                return globalBalance.Value;

            if (globalBalance.BitCoinValue == 0)
                return 0;


            var oBtcToFav = oCurrencies.FirstOrDefault(o => (o.From == "BTC" || o.From == "WBTC") && o.To == sCrypto);

            if (oBtcToFav != null)
            {
                return globalBalance.BitCoinValue * oBtcToFav.Price;
            }

            return 0;

        }
        public static double GetCustomValue(this List<CurrencySymbolPrice> oCurrencies, GlobalBalance globalBalance, string sCrypto)
        {
            string sCryptoFrom = globalBalance.Crypto.ToUpper();
            if (sCryptoFrom == sCrypto)
                return globalBalance.Value;

            if (sCryptoFrom == "BTXCRD") // Bittrex Credit
                return 0;
            if (sCryptoFrom == "MAX") // shitcoin that has the same name of another coin
                return 0;

            // look in current exchange
            var oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == sCryptoFrom && o.To == sCrypto && o.Exchange == globalBalance.Exchange);

            if (oCryptoFound != null)
            {
                return globalBalance.Value * oCryptoFound.Price;
            }

            oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == sCryptoFrom && o.To == "USDT" && o.Exchange == globalBalance.Exchange);
            var oCryptoFoundUsdtToFav = oCurrencies.FirstOrDefault(o => o.From == "USDT" && o.To == sCrypto && o.Exchange == globalBalance.Exchange);

            if (oCryptoFound != null && oCryptoFoundUsdtToFav != null)
            {
                return globalBalance.Value * oCryptoFound.Price * oCryptoFoundUsdtToFav.Price;
            }

            oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == sCryptoFrom && o.To == "BTC" && o.Exchange == globalBalance.Exchange);
            oCryptoFoundUsdtToFav = oCurrencies.FirstOrDefault(o => o.From == "BTC" && o.To == sCrypto && o.Exchange == globalBalance.Exchange);

            if (oCryptoFound != null && oCryptoFoundUsdtToFav != null)
            {
                return globalBalance.Value * oCryptoFound.Price * oCryptoFoundUsdtToFav.Price;
            }

            return 0;

            // fallback, look in all exchanges
            oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == sCryptoFrom && o.To == sCrypto);

            if (oCryptoFound != null)
            {
                return globalBalance.Value * oCryptoFound.Price;
            }

            oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == sCryptoFrom && o.To == "USDT");
            oCryptoFoundUsdtToFav = oCurrencies.FirstOrDefault(o => o.From == "USDT" && o.To == sCrypto);

            if (oCryptoFound != null && oCryptoFoundUsdtToFav != null)
            {
                return globalBalance.Value * oCryptoFound.Price * oCryptoFoundUsdtToFav.Price;
            }

            oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == sCryptoFrom && o.To == "BTC");
            oCryptoFoundUsdtToFav = oCurrencies.FirstOrDefault(o => o.From == "BTC" && o.To == sCrypto);

            if (oCryptoFound != null && oCryptoFoundUsdtToFav != null)
            {
                return globalBalance.Value * oCryptoFound.Price * oCryptoFoundUsdtToFav.Price;
            }

            return 0;
        }
        public static double GetCustomPrice(this List<CurrencySymbolPrice> oCurrencies, string sFrom, string sCrypto)
        {
            string sCryptoFrom = sFrom.ToUpper();
            if (sCryptoFrom == sCrypto)
                return 1;

            // look in current exchange
            var oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == sCryptoFrom && o.To == sCrypto);

            if (oCryptoFound != null)
            {
                return oCryptoFound.Price;
            }

            oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == sCryptoFrom && o.To == "USDT");
            var oCryptoFoundUsdtToFav = oCurrencies.FirstOrDefault(o => o.From == "USDT" && o.To == sCrypto);

            if (oCryptoFound != null && oCryptoFoundUsdtToFav != null)
            {
                return oCryptoFound.Price * oCryptoFoundUsdtToFav.Price;
            }

            oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == sCryptoFrom && o.To == "BTC");
            oCryptoFoundUsdtToFav = oCurrencies.FirstOrDefault(o => o.From == "BTC" && o.To == sCrypto);

            if (oCryptoFound != null && oCryptoFoundUsdtToFav != null)
            {
                return oCryptoFound.Price * oCryptoFoundUsdtToFav.Price;
            }

            return 1;

        }
    }
}
