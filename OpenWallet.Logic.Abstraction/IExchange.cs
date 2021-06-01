using OpenWallet.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWallet.Logic.Abstraction
{
    public interface IExchange
    {
        string ExchangeCode { get; }
        string ExchangeName { get; }
        List<GlobalBalance> GetBalance();
        List<CurrencySymbolPrice> GetCurrencies();
        void Init(GlobalConfig oGlobalConfig, ExchangeConfig oConfig);
        List<GlobalTrade> GetTradeHistory(List<GlobalTrade> aCache);
    }
}
