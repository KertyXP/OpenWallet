using OpenWallet.Common;
using OpenWallet.Common.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace OpenWallet.Logic.Abstraction.Interfaces
{

    public struct TradeArchived
    {

        public string from, to;
        public double quantityFrom, quantityTo;
    }

    public interface ITradeService
    {

        List<GlobalTrade> LoadTradesFromCacheOnly(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies);
        double GetAverageBuy(List<GlobalTrade> trades);
        double GetAverageSell(List<GlobalTrade> trades);

        GlobalTradeUI GetGlobalTradeUI(GlobalTrade globalTrade, List<CurrencySymbolPrice> allCurrencies, List<CurrencySymbolPrice> aFiatisation, bool isArchived);

        Task<List<GlobalTrade>> LoadTrades(List<IExchange> aExchanges, List<GlobalBalance> aAllBalances, List<CurrencySymbolPrice> aAllCurrencies);


        TradeArchived ArchiveTrades(List<GlobalTrade> g);

        IEnumerable<string> GetCouplesFromTrade(IEnumerable<GlobalTrade> trades);

        double GetDelta(GlobalTrade trade, List<CurrencySymbolPrice> currencies);

        bool IsProfitable(GlobalTrade trade, double delta);

        Color GetSellStateBackColor(GlobalTrade trade);

        Color GetSellStateBackColorSelected(GlobalTrade trade);

        Color GetArchiveStateForeColor(bool tradeIsArchived);

        Color GetDeltaColor(bool isProfitable);

        Color GetDeltaColorSelected(bool isProfitable);

    }
}
