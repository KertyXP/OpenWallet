using OpenWallet.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWallet.Logic.Abstraction
{
    public interface IExchange
    {
        string ExchangeCode { get; }
        string ExchangeName { get; }
        ExchangeConfig oConfig { get; set; }
        Task<List<GlobalBalance>> GetBalanceAsync();
        Task<List<CurrencySymbolPrice>> GetCurrenciesAsync();
        Task InitAsync(GlobalConfig oGlobalConfig, ExchangeConfig oConfig);
        Task<List<GlobalTrade>> GetTradeHitoryAsync(List<GlobalTrade> aCache, List<GlobalBalance> aAllBalances);
        Task<GlobalTrade> PlaceMarketOrderAsync(CurrencySymbol symbol, double quantity, SellBuy SellOrBuy, bool bTest);
    }

    public interface IRefreshOneCoupleTrade
    {
        Task<List<GlobalTrade>> GetTradeHitoryOneCoupleAsync(List<GlobalTrade> aCache, CurrencySymbol symbol);
    }
    public interface IGetTradesData
    {
        Task<TradesData> GetTradeHitoryOneCoupleAsync(string interval, CurrencySymbolExchange symbol);
    }

    public enum SellBuy
    {
        Sell,Buy
    }

}
