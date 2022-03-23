using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using System.Collections.Generic;
using System.Drawing;
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
                .ForEach(trade => FiatiseOneTrade(trade, aFiatisation))
                .ToList();


            return aListTrades;
        }

        public static double GetAverageBuy(List<GlobalTrade> trades)
        {
            var tradesBought = trades.Where(t => t.IsBuy == true).ToList();
            return tradesBought.Sum(t => t.RealQuantityTo) / tradesBought.Sum(t => t.RealQuantityFrom);
        }
        public static double GetAverageSell(List<GlobalTrade> trades)
        {
            var tradeSold = trades.Where(t => t.IsBuy == false).ToList();
            return tradeSold.Sum(t => t.RealQuantityTo) / tradeSold.Sum(t => t.RealQuantityFrom);
        }

        public static async Task<List<GlobalTrade>> LoadTrades(List<IExchange> aExchanges, List<GlobalBalance> aAllBalances, List<CurrencySymbolPrice> aAllCurrencies)
        {

            List<CurrencySymbolPrice> aFiatisation = BalanceService.LoadFiatisation(aAllCurrencies);

            var tasks = aExchanges.Select(oExchange =>
            {
                var tradesFromCache = ConfigService.LoadTradesFromCache(oExchange);

                return oExchange.GetTradeHistoryAsync(tradesFromCache, aAllBalances);

            });


            var result = await Task.WhenAll(tasks);

            return result
                .ForEach(trades =>
                {
                    ConfigService.SaveTradesToCache(trades);
                })
                .SelectMany(r => r)
                .OrderByDescending(s => s.dtTrade)
                .GroupBy(l => l.InternalExchangeId + l.Exchange)
                .Select(l => l.FirstOrDefault())
                .ForEach(trade => FiatiseOneTrade(trade, aFiatisation))
                .ToList();

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


        public struct TradeArchived
        {

            public string from, to;
            public double quantityFrom, quantityTo;
        }
        public static TradeArchived ArchiveTrades(List<GlobalTrade> g)
        {
            TradeArchived tradeArchived;

            tradeArchived.from = g.FirstOrDefault()?.From;
            tradeArchived.to = g.FirstOrDefault()?.To;
            tradeArchived.quantityFrom = g.Where(t => t.To == tradeArchived.from).Sum(t => t.QuantityTo) - g.Where(t => t.From == tradeArchived.from).Sum(t => t.QuantityFrom);
            tradeArchived.quantityTo = g.Where(t => t.To == tradeArchived.to).Sum(t => t.QuantityTo) - g.Where(t => t.From == tradeArchived.to).Sum(t => t.QuantityFrom);

            return tradeArchived;
        }

        public static IEnumerable<string> GetCouplesFromTrade(IEnumerable<GlobalTrade> trades)
        {
            return trades.Select(t => t.CustomCouple)
            .GroupBy(t => t)
            .Select(t => t.FirstOrDefault());
        }

        public static double GetDelta(GlobalTrade trade, List<CurrencySymbolPrice> currencies)
        {
            var currentPrice = currencies.FirstOrDefault(c => c.Couple == trade.Couple)?.RealPrice ?? 1;
            var delta = currentPrice / trade.RealPrice * 100 - 100;
            return delta;
        }

        public static bool IsProfitable(GlobalTrade trade, double delta)
        {
            return (delta > 0 && trade.IsBuy) || (delta < 0 && trade.IsBuy == false);
        }

        private static Color colorGreenLight = Color.FromArgb(255, 200, 255, 200);
        private static Color colorGreenSelected = Color.FromArgb(255, 150, 200, 150);
        private static Color colorRedLight = Color.FromArgb(255, 255, 200, 200);
        private static Color colorRedSelected = Color.FromArgb(255, 200, 150, 150);
        private static Color colorGrey = Color.FromArgb(255, 150, 150, 150);
        private static Color colorDark = Color.FromArgb(255, 50, 50, 50);

        public static Color GetSellStateBackColor(GlobalTrade trade)
        {
            return trade.IsBuy ? colorGreenLight : colorRedLight;
        }

        public static Color GetSellStateBackColorSelected(GlobalTrade trade)
        {
            return trade.IsBuy ? colorGreenSelected : colorRedSelected;
        }

        public static Color GetArchiveStateForeColor(bool tradeIsArchived)
        {
            return tradeIsArchived ? colorGrey : colorDark;
        }

        public static Color GetDeltaColor(bool isProfitable)
        {
            return isProfitable ? colorGreenLight : colorRedLight;
        }

        public static Color GetDeltaColorSelected(bool isProfitable)
        {
            return isProfitable ? colorGreenSelected : colorRedSelected;
        }

    }
}
