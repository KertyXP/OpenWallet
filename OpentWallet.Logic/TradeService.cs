using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpentWallet.Logic
{
    public class TradeService
    {

        public static List<GlobalTrade> LoadTradesFromCacheOnly(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies)
        {
            List<CurrencySymbolPrice> aFiatisation = BalanceService.LoadFiatisation(aAllCurrencies);

            var aListTrades = aExchanges.Select(oExchange =>
            {
                return ConfigService.LoadTradesFromCache(oExchange);
            })
                .SelectMany(s => s)
                .OrderByDescending(s => s.dtTrade)
                .GroupBy(l => l.InternalExchangeId + l.Exchange)
                .Select(l => l.FirstOrDefault())
                .ToList();


            aListTrades.ForEach(trade =>
            {
                FiatiseOneTrade(trade, aFiatisation);
            });

            aListTrades.ForEach(trade =>
            {
                CalculGainBack(trade, aAllCurrencies);
            });

            return aListTrades;
        }

        public static async Task<List<GlobalTrade>> LoadTrades(List<IExchange> aExchanges, List<GlobalBalance> aAllBalances, List<CurrencySymbolPrice> aAllCurrencies)
        {

            List<CurrencySymbolPrice> aFiatisation = BalanceService.LoadFiatisation(aAllCurrencies);

            var aTasks3 = aExchanges.Select(oExchange =>
            {
                var aTrades = ConfigService.LoadTradesFromCache(oExchange);

                return Task.Run(() => oExchange.GetTradeHistory(aTrades, aAllBalances));
            }).ToList();
            List<GlobalTrade> aListTrades = new List<GlobalTrade>();

            foreach (var oTask in aTasks3)
            {
                try
                {
                    var aBalance = await oTask;
                    if (aBalance.Any() == false)
                        continue;

                    ConfigService.SaveTradesToCache(aBalance);

                    aListTrades.AddRange(aBalance);
                }
                catch
                {

                }
            }

            aListTrades = aListTrades
                .OrderByDescending(s => s.dtTrade)
                .GroupBy(l => l.InternalExchangeId + l.Exchange)
                .Select(l => l.FirstOrDefault())
                .ToList(); ;


            aListTrades.ForEach(trade =>
            {
                FiatiseOneTrade(trade, aFiatisation);
            });

            aListTrades.ForEach(trade =>
            {
                CalculGainBack(trade, aAllCurrencies);
            });

            return aListTrades;


        }
        /// <summary>
        /// not working
        /// </summary>
        /// <param name="oTrade"></param>
        /// <param name="aAllCurrencies"></param>
        private static void CalculGainBack(GlobalTrade oTrade, List<CurrencySymbolPrice> aAllCurrencies)
        {
            var oCurrencyCouple = aAllCurrencies.FirstOrDefault(c => c.From == oTrade.To && c.To == oTrade.From && c.Exchange == oTrade.Exchange);
        }
        private static void FiatiseOneTrade(GlobalTrade oTrade, List<CurrencySymbolPrice> aFiatisation)
        {
            var fiatFrom = aFiatisation.FirstOrDefault(f => f.From == oTrade.From);
            var fiatTo = aFiatisation.FirstOrDefault(f => f.From == oTrade.To);
            if (fiatFrom != null && fiatTo != null)
                return;

            if (fiatFrom != null)
            {
                oTrade.From = fiatFrom.To;
                oTrade.Price = oTrade.Price / fiatFrom.Price;
                oTrade.SetQuantities(oTrade.QuantityFrom / fiatFrom.Price, oTrade.QuantityTo);
                return;
            }

            if (fiatTo != null)
            {
                oTrade.To = fiatTo.To;
                oTrade.Price = oTrade.Price * fiatTo.Price;
                oTrade.SetQuantities(oTrade.QuantityFrom, oTrade.QuantityTo / fiatTo.Price);
                return;
            }
        }

        public static List<GlobalTrade> ConvertTradesToDailyTrades(List<GlobalTrade> aListTrades)
        {

            return aListTrades.GroupBy(l => l.From + "|" + l.To + "_" + l.dtTrade.ToString("_yyyy-MM-dd")).Select(l =>
            {
                var one = l.FirstOrDefault();
                double price = one.Couple.StartsWith(one.From) ? one.QuantityTo / one.QuantityFrom : one.QuantityFrom / one.QuantityTo;
                var oGlobalTrade = new GlobalTrade(one.From, one.To, price, one.Couple, one.Exchange);
                oGlobalTrade.CryptoFromId = one.CryptoFromId;
                oGlobalTrade.CryptoToId = one.CryptoToId;
                oGlobalTrade.SetQuantities(l.Sum(m => m.QuantityFrom), l.Sum(m => m.QuantityTo));

                oGlobalTrade.InternalExchangeId = one.InternalExchangeId;
                oGlobalTrade.dtTrade = one.dtTrade;

                return oGlobalTrade;
            }).OrderByDescending(l => l.dtTrade).OrderBy(t => t.Couple).ToList();

        }


        public static List<GlobalTrade> ConvertTradesToGlobalTrades(List<GlobalTrade> aListTrades)
        {
            return aListTrades.GroupBy(l => l.From + "|" + l.To).Select(l =>
            {
                var one = l.FirstOrDefault();
                var oGlobalTrade = new GlobalTrade(one.From, one.To, l.Sum(m => m.QuantityFrom) / l.Sum(m => m.QuantityTo), one.Couple, one.Exchange);
                oGlobalTrade.CryptoFromId = one.CryptoFromId;
                oGlobalTrade.CryptoToId = one.CryptoToId;
                oGlobalTrade.SetQuantities(l.Sum(m => m.QuantityFrom), l.Sum(m => m.QuantityTo));
                oGlobalTrade.InternalExchangeId = one.InternalExchangeId;
                oGlobalTrade.dtTrade = one.dtTrade;

                return oGlobalTrade;
            }).OrderBy(l => l.From).OrderBy(t => t.Couple).ToList();
        }

        public static Dictionary<string, List<GlobalTrade>> LoadArchiveTrade()
        {

            Dictionary<string, List<GlobalTrade>> dicArchives = new Dictionary<string, List<GlobalTrade>>();

            dicArchives = ConfigService.LoadArchiveTradeFromCache();


            //for (int i = archiveTrades.Count - 1; i > 0; i--)
            //{
            //    for (int j = i - 1; i >= 0; i--)
            //    {
            //        if (i == j)
            //            continue;

            //        if (archiveTrades[i].Any(gtSource => archiveTrades[j].Any(gtDest => gtDest.InternalExchangeId == gtSource.InternalExchangeId)))
            //        {
            //            archiveTrades.RemoveAt(i);
            //            break;
            //        }
            //    }
            //}

            return dicArchives;

        }
    }
}
