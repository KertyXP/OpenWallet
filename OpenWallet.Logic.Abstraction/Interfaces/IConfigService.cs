﻿using OpenWallet.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWallet.Logic.Abstraction.Interfaces
{
    public interface IConfigService
    {
        void Init(string sFolderPath);

        GlobalConfig oGlobalConfig { get; }
        void SetHideArchive(bool hideArchive);
        bool GetHideArchive();


        Dictionary<string, GlobalTrade> GetArchiveTrades();
        void SaveArchiveTradeToCache(Dictionary<string, GlobalTrade> archiveTrades);
        void LoadArchiveTradeFromCache();
        List<GlobalTrade> LoadTradefromCache(IExchange exchange);
        void SaveTradetoCache(IEnumerable<GlobalTrade> trades);
        void SaveBalanceToCache(IExchange exchange, List<GlobalBalance> balance);
        List<GlobalBalance> LoadBalanceFromCache(IExchange exchange);
        Task<List<IExchange>> LoadExchangesAsync<T>();
        void SaveGenericToCache<T>(IExchange exchange, T oExchangeInfo, string type) where T : class, new();
        T LoadGenericFromCache<T>(IExchange exchange, string type) where T : class, new();
    }
}
