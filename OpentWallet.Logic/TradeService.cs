using OpenWallet.Common;
using OpenWallet.Common.Models;
using OpenWallet.Logic.Abstraction;
using OpenWallet.Logic.Abstraction.Interfaces;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace OpentWallet.Logic
{
    public class TradeService : ITradeService
    {

        private List<GlobalTrade> trades = new List<GlobalTrade>();

        public TradeService(IBalanceService balanceService, IConfigService configService)
        {
            BalanceService = balanceService;
            ConfigService = configService;
        }


        public List<GlobalTrade> GetTrades()
            => trades;


        public void LoadTradefromCacheOnly(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies)
        {
            List<CurrencySymbolPrice> aFiatisation = BalanceService.LoadFiatisation(aAllCurrencies);

            trades.Clear();
            trades.AddRange(aExchanges.Select(oExchange =>
            {
                return ConfigService.LoadTradefromCache(oExchange);
            })
                .SelectMany(s => s)
                .Where(s => ConfigService.oGlobalConfig.ignoredCurrencies.Any(ic => ic == s.From) == false && ConfigService.oGlobalConfig.ignoredCurrencies.Any(ic => ic == s.To) == false)
                .OrderByDescending(s => s.dtTrade)
                .GroupBy(l => l.InternalExchangeId + l.Exchange)
                .Select(l => l.FirstOrDefault())
                );
        }

        public double GetAverageBuy(List<GlobalTrade> trades)
        {
            var tradesBought = trades.Where(t => t.IsBuy == true).ToList();
            return tradesBought.Sum(t => t.RealQuantityTo) / tradesBought.Sum(t => t.RealQuantityFrom);
        }
        public double GetAverageSell(List<GlobalTrade> trades)
        {
            var tradeSold = trades.Where(t => t.IsBuy == false).ToList();
            return tradeSold.Sum(t => t.RealQuantityTo) / tradeSold.Sum(t => t.RealQuantityFrom);
        }

        public async Task<List<GlobalTrade>> LoadTrades(List<IExchange> aExchanges, List<GlobalBalance> aAllBalances, List<CurrencySymbolPrice> aAllCurrencies)
        {

            var tasks = aExchanges.Select(oExchange =>
            {
                var tradefromCache = ConfigService.LoadTradefromCache(oExchange);

                return oExchange.GetTradeHitoryAsync(tradefromCache, aAllBalances);

            });


            var result = await Task.WhenAll(tasks);

            return result
                .ForEach(trades =>
                {
                    ConfigService.SaveTradetoCache(trades);
                })
                .SelectMany(r => r)
                .OrderByDescending(s => s.dtTrade)
                .GroupBy(l => l.InternalExchangeId + l.Exchange)
                .Select(l => l.FirstOrDefault())
                .ToList();

        }

        public GlobalTrade FiatiseOneTrade(GlobalTrade oTrade, List<CurrencySymbolPrice> aFiatisation)
        {
            var fiatFrom = aFiatisation.FirstOrDefault(f => f.From == oTrade.From);
            var fiatTo = aFiatisation.FirstOrDefault(f => f.From == oTrade.To);
            if (fiatFrom != null && fiatTo != null)
                return oTrade;

            if (fiatFrom != null)
            {
                oTrade.From = fiatFrom.To;
                oTrade.Price = oTrade.Price / fiatFrom.Price;
                oTrade.SetQuantities(oTrade.QuantityFrom / fiatFrom.Price, oTrade.QuantityTo);
                return oTrade;
            }

            if (fiatTo != null)
            {
                oTrade.To = fiatTo.To;
                oTrade.Price = oTrade.Price * fiatTo.Price;
                oTrade.SetQuantities(oTrade.QuantityFrom, oTrade.QuantityTo / fiatTo.Price);
                return oTrade;
            }

            return oTrade;

        }

        public GlobalTradeUI GetGlobalTradeUI(GlobalTrade globalTrade, List<CurrencySymbolPrice> allCurrencies, List<CurrencySymbolPrice> aFiatisation, bool isArchived)
        {
            var trade = FiatiseOneTrade(globalTrade, aFiatisation);
            var delta = GetDelta(trade, allCurrencies);
            var isProfitable = IsProfitable(trade, delta);

            var globalTradeUI = new GlobalTradeUI()
            {
                Trade = trade,

                Delta = delta,
                IsProfitable = isProfitable,

                SellStateBackColor = GetSellStateBackColor(trade),
                SellStateBackColorSelected = GetSellStateBackColorSelected(trade),
                ArchiveStateForeColor = GetArchiveStateForeColor(isArchived),

                DeltaColor = GetDeltaColor(isProfitable),
                DeltaColorSelected = GetDeltaColorSelected(isProfitable),
            };

            return globalTradeUI;
        }

        public TradeArchived ArchiveTrades(List<GlobalTrade> g)
        {
            TradeArchived tradeArchived;

            tradeArchived.from = g.FirstOrDefault()?.From;
            tradeArchived.to = g.FirstOrDefault()?.To;
            tradeArchived.quantityFrom = g.Where(t => t.To == tradeArchived.from).Sum(t => t.QuantityTo) - g.Where(t => t.From == tradeArchived.from).Sum(t => t.QuantityFrom);
            tradeArchived.quantityTo = g.Where(t => t.To == tradeArchived.to).Sum(t => t.QuantityTo) - g.Where(t => t.From == tradeArchived.to).Sum(t => t.QuantityFrom);

            return tradeArchived;
        }

        public IEnumerable<string> GetCouplefromTrade(IEnumerable<GlobalTrade> trades)
        {
            return trades.Select(t => t.CutomCouple)
            .GroupBy(t => t)
            .Select(t => t.FirstOrDefault());
        }

        public double GetDelta(GlobalTrade trade, List<CurrencySymbolPrice> currencies)
        {
            var currentPrice = currencies.FirstOrDefault(c => c.Couple == trade.Couple)?.RealPrice ?? 1;
            var delta = currentPrice / trade.RealPrice * 100 - 100;
            return delta;
        }

        public bool IsProfitable(GlobalTrade trade, double delta)
        {
            return (delta > 0 && trade.IsBuy) || (delta < 0 && trade.IsBuy == false);
        }

        private Color colorGreenLight = Color.FromArgb(255, 200, 255, 200);
        private Color colorGreenSelected = Color.FromArgb(255, 150, 200, 150);
        private Color colorRedLight = Color.FromArgb(255, 255, 200, 200);
        private Color colorRedSelected = Color.FromArgb(255, 200, 150, 150);
        private Color colorGrey = Color.FromArgb(255, 150, 150, 150);
        private Color colorDark = Color.FromArgb(255, 50, 50, 50);

        public IBalanceService BalanceService { get; }
        public IConfigService ConfigService { get; }

        public Color GetSellStateBackColor(GlobalTrade trade)
        {
            return trade.IsBuy ? colorGreenLight : colorRedLight;
        }

        public Color GetSellStateBackColorSelected(GlobalTrade trade)
        {
            return trade.IsBuy ? colorGreenSelected : colorRedSelected;
        }

        public Color GetArchiveStateForeColor(bool tradeIsArchived)
        {
            return tradeIsArchived ? colorGrey : colorDark;
        }

        public Color GetDeltaColor(bool isProfitable)
        {
            return isProfitable ? colorGreenLight : colorRedLight;
        }

        public Color GetDeltaColorSelected(bool isProfitable)
        {
            return isProfitable ? colorGreenSelected : colorRedSelected;
        }

    }
}
