using OpenWallet.Common;
using System.Collections.Generic;

namespace OpenWallet.Logic.Abstraction
{
    public interface IExchange
    {
        string ExchangeCode { get; }
        string ExchangeName { get; }
        ExchangeConfig oConfig { get; set; }
        List<GlobalBalance> GetBalance();
        List<CurrencySymbolPrice> GetCurrencies();
        void Init(GlobalConfig oGlobalConfig, ExchangeConfig oConfig);
        List<GlobalTrade> GetTradeHistory(List<GlobalTrade> aCache, List<GlobalBalance> aAllBalances);
        GlobalTrade PlaceMarketOrder(CurrencySymbol symbol, double quantity, SellBuy SellOrBuy, bool bTest);
    }

    public interface IRefreshOneCoupleTrade
    {
        List<GlobalTrade> GetTradeHistoryOneCouple(List<GlobalTrade> aCache, List<GlobalBalance> aAllBalances, string couple);
    }

    public enum SellBuy
    {
        Sell,Buy
    }

}
