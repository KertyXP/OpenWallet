using Newtonsoft.Json;
using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenWallet.Logic.Abstraction.Interfaces
{
    public interface IConfigService
    {
        void Init(string sFolderPath);

        GlobalConfig oGlobalConfig { get; }

        void SaveArchiveTradeToCache(Dictionary<string, List<GlobalTrade>> archiveTrades);
        Dictionary<string, List<GlobalTrade>> LoadArchiveTradeFromCache();
        List<GlobalTrade> LoadTradesFromCache(IExchange exchange);
        void SaveTradesToCache(IEnumerable<GlobalTrade> trades);
        void SaveBalanceToCache(IExchange exchange, List<GlobalBalance> balance);
        List<GlobalBalance> LoadBalanceFromCache(IExchange exchange);
        Task<List<IExchange>> LoadExchangesAsync();

    }
}
