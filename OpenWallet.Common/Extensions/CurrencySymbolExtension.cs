using System.Collections.Generic;
using System.Linq;

namespace OpenWallet.Common
{
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
        public static double GetCustomValue(this List<CurrencySymbolPrice> oCurrencies, GlobalBalance globalBalance, string sCrypto, double? overrideValue = null)
        {
            string sCryptoFrom = globalBalance.Crypto.ToUpper();
            if (sCryptoFrom == sCrypto)
                return overrideValue ?? globalBalance.Value;

            if (sCryptoFrom == "BTXCRD") // Bittrex Credit
                return 0;
            if (sCryptoFrom == "MAX") // shitcoin that has the same name of another coin
                return 0;

            // look in current exchange
            var oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == sCryptoFrom && o.To == sCrypto && o.Exchange == globalBalance.Exchange);

            if (oCryptoFound != null)
            {
                return (overrideValue ?? globalBalance.Value) * oCryptoFound.Price;
            }

            oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == sCryptoFrom && o.To == "USDT" && o.Exchange == globalBalance.Exchange);
            var oCryptoFoundUsdtToFav = oCurrencies.FirstOrDefault(o => o.From == "USDT" && o.To == sCrypto && o.Exchange == globalBalance.Exchange);

            if (oCryptoFound != null && oCryptoFoundUsdtToFav != null)
            {
                return (overrideValue ?? globalBalance.Value) * oCryptoFound.Price * oCryptoFoundUsdtToFav.Price;
            }

            oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == sCryptoFrom && o.To == "BTC" && o.Exchange == globalBalance.Exchange);
            oCryptoFoundUsdtToFav = oCurrencies.FirstOrDefault(o => o.From == "BTC" && o.To == sCrypto && o.Exchange == globalBalance.Exchange);

            if (oCryptoFound != null && oCryptoFoundUsdtToFav != null)
            {
                return (overrideValue ?? globalBalance.Value) * oCryptoFound.Price * oCryptoFoundUsdtToFav.Price;
            }

            return 0;

            // fallback, look in all exchanges
            oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == sCryptoFrom && o.To == sCrypto);

            if (oCryptoFound != null)
            {
                return (overrideValue ?? globalBalance.Value) * oCryptoFound.Price;
            }

            oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == sCryptoFrom && o.To == "USDT");
            oCryptoFoundUsdtToFav = oCurrencies.FirstOrDefault(o => o.From == "USDT" && o.To == sCrypto);

            if (oCryptoFound != null && oCryptoFoundUsdtToFav != null)
            {
                return (overrideValue ?? globalBalance.Value) * oCryptoFound.Price * oCryptoFoundUsdtToFav.Price;
            }

            oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == sCryptoFrom && o.To == "BTC");
            oCryptoFoundUsdtToFav = oCurrencies.FirstOrDefault(o => o.From == "BTC" && o.To == sCrypto);

            if (oCryptoFound != null && oCryptoFoundUsdtToFav != null)
            {
                return (overrideValue ?? globalBalance.Value) * oCryptoFound.Price * oCryptoFoundUsdtToFav.Price;
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
