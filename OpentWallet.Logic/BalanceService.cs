using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpentWallet.Logic
{
    public class BalanceService
    {

        public static async Task<List<CurrencySymbolPrice>> GetCurrencriesAsync(List<IExchange> aExchanges)
        {

            List<CurrencySymbolPrice> aAllCurrencies = new List<CurrencySymbolPrice>();
            var aTaskCurrencies = aExchanges
                .Select(async oExchange => await oExchange.GetCurrenciesAsync())
                .ToList();

            foreach (var oTask in aTaskCurrencies)
            {
                var aBalance = await oTask;
                aAllCurrencies.AddRange(aBalance);
            }

            return aAllCurrencies;
        }

        public static List<CurrencySymbolPrice> LoadFiatisation(List<CurrencySymbolPrice> aAllCurrencies)
        {

            List<CurrencySymbolPrice> aFiatisation = new List<CurrencySymbolPrice>();
            // fiatisation (needs at least 2 fiats Moneys (first is the fiatisation TO)
            if (ConfigService.oGlobalConfig.FiatMoneys.Count() > 1)
            {
                string sTo = ConfigService.oGlobalConfig.FiatMoneys.FirstOrDefault();
                aFiatisation = ConfigService.oGlobalConfig.FiatMoneys.Skip(1).Select(fiat =>
                {
                    return new CurrencySymbolPrice(fiat, sTo, aAllCurrencies.GetCustomPrice(fiat, sTo), string.Empty, string.Empty);
                }).ToList();
            }

            return aFiatisation;
        }

        public static List<GlobalBalance> SetBitcoinFavCryptoValue(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies, List<GlobalBalance> aAllBalances)
        {

            foreach (var oBalance in aAllBalances)
            {
                var oExchange = aExchanges.FirstOrDefault(e => e.ExchangeCode == oBalance.Exchange);

                if (oExchange.oConfig.CurrenciesToIgnore?.Any(c => c == oBalance.Crypto) == true)
                    continue;

                oBalance.FavCrypto = ConfigService.oGlobalConfig.FavoriteCurrency;
                if (oBalance.Exchange == "BSC")
                {
                    //oBalance.BitCoinValue = aAllCurrencies.GetBtcValue(oBalance);
                    oBalance.FavCryptoValue = aAllCurrencies.GetCustomValueFromBtc(oBalance, oBalance.FavCrypto);
                }
                else
                {
                    oBalance.BitCoinValue = aAllCurrencies.GetBtcValue(oBalance);
                    oBalance.FavCryptoValue = aAllCurrencies.GetCustomValue(oBalance, oBalance.FavCrypto);
                }
            }

            return aAllBalances;
        }

        public static async Task<List<GlobalBalance>> GetBalances(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies)
        {

            ConcurrentQueue<GlobalBalance> aAll = new ConcurrentQueue<GlobalBalance>();

            var tasks = aExchanges.Select(async exchange =>
            {
                var aBalance = await exchange.GetBalanceAsync();
                ConfigService.SaveBalanceToCache(exchange, aBalance);

                aBalance = SetBitcoinFavCryptoValue(aExchanges, aAllCurrencies, aBalance);
                foreach (var oBalance in aBalance)
                {
                    if (exchange.oConfig.CurrenciesToIgnore?.Any(c => c == oBalance.Crypto) == true)
                        continue;

                    aAll.Enqueue(oBalance);
                }
            }).ToList();

            foreach (var task in tasks)
            {
                await task;
            }



            return aAll.ToList();
        }
        public static List<GlobalBalance> LoadBalancesFromCacheOnly(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies)
        {

            ConcurrentQueue<GlobalBalance> aAll = new ConcurrentQueue<GlobalBalance>();


            var aBalance = aExchanges.Select(oExchange =>
            {
                return ConfigService.LoadBalanceFromCache(oExchange);
            }).SelectMany(b => b).ToList();


            aBalance = SetBitcoinFavCryptoValue(aExchanges, aAllCurrencies, aBalance);

            return aBalance;
        }
    }
}
