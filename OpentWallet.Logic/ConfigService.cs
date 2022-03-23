using Newtonsoft.Json;
using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using OpenWallet.Logic.Abstraction.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpentWallet.Logic
{
    public class ConfigService : IConfigService
    {
        private string sRootPath = "";

        public void Init(string sFolderPath)
        {
            sRootPath = sFolderPath;
        }

        private string GetPath(string fileName)
        {
            return Path.Combine(sRootPath, fileName);
        }

        private GlobalConfig _globalConfig;

        public GlobalConfig oGlobalConfig => _globalConfig ?? (_globalConfig = LoadConfig());
        private GlobalConfig LoadConfig()
        {
            string sPath = GetPath("config.json");
            Console.WriteLine(sPath);
            GlobalConfig oConfig = null;
            if (File.Exists(sPath))
            {
                oConfig = JsonConvert.DeserializeObject<GlobalConfig>(File.ReadAllText(sPath));
            }
            else
            {
                var path = Path.GetDirectoryName(sPath);
                if(string.IsNullOrEmpty(path) == false)
                {
                    Directory.CreateDirectory(path);
                }
                oConfig = new GlobalConfig();
                oConfig.configs = new List<ExchangeConfig>()
                {
                    new ExchangeConfig()
                    {
                        ExchangeCode = "Binance",
                        ApiKey = "ApiKey",
                        SecretKey = "SecretKey",
                        AdditionnalKey = ""
                    }
                };
                File.WriteAllText(sPath, JsonConvert.SerializeObject(oConfig));
            }
            oConfig.FiatMoneys = oConfig.FiatMoneys.GroupBy(c => c).Select(c => c.FirstOrDefault()).ToList();
            return oConfig;
        }

        public void SaveArchiveTradeToCache(Dictionary<string, List<GlobalTrade>> archiveTrades)
        {
            string sPath = GetPath("ArchiveTrades.json");
            string json = JsonConvert.SerializeObject(archiveTrades);

            File.WriteAllText(sPath, json);
        }

        public Dictionary<string, List<GlobalTrade>> LoadArchiveTradeFromCache()
        {
            string sPath = GetPath("ArchiveTrades.json");

            return File.Exists(sPath) ? JsonConvert.DeserializeObject<Dictionary<string, List<GlobalTrade>>>(File.ReadAllText(sPath)) : new Dictionary<string, List<GlobalTrade>>();
        }

        public List<GlobalTrade> LoadTradesFromCache(IExchange exchange)
        {
            string sPath = GetPath("Trades_" + exchange.ExchangeName + ".json");
            return File.Exists(sPath) ? JsonConvert.DeserializeObject<List<GlobalTrade>>(File.ReadAllText(sPath)) : new List<GlobalTrade>();
        }
        public void SaveTradesToCache(IEnumerable<GlobalTrade> trades)
        {
            var sJson = JsonConvert.SerializeObject(trades);
            string sPath = GetPath("Trades_" + trades?.FirstOrDefault()?.Exchange + ".json");
            File.WriteAllText(sPath, sJson);
        }

        public void SaveBalanceToCache(IExchange exchange, List<GlobalBalance> balance)
        {
            var sJson = JsonConvert.SerializeObject(balance);
            string sPath = GetPath("Balance" + exchange.ExchangeCode + ".json");
            File.WriteAllText(sPath, sJson);
        }

        public List<GlobalBalance> LoadBalanceFromCache(IExchange exchange)
        {
            string sPath = GetPath("Balance" + exchange.ExchangeCode + ".json");
            return File.Exists(sPath) ? JsonConvert.DeserializeObject<List<GlobalBalance>>(File.ReadAllText(sPath)) : new List<GlobalBalance>();
        }

        public async Task<List<IExchange>> LoadExchangesAsync()
        {

            List<IExchange> aExchanges = new List<IExchange>();
            var type = typeof(IExchange);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.IsClass)
                .ToList()
                .Select(t => Activator.CreateInstance(t) as IExchange)
                .Where(t => t != null);

            foreach (var oConfig in oGlobalConfig.configs)
            {
                var typeExchange = types.FirstOrDefault(t => t.ExchangeCode == oConfig.ExchangeCode);
                if (typeExchange == null)
                    continue; // oops

                var oExchange = Activator.CreateInstance(typeExchange.GetType()) as IExchange;
                if (oExchange == null)
                    continue; // re-oops

                if (oConfig.IsActive == false)
                    continue;

                oExchange.oConfig = oConfig;
                await oExchange.InitAsync(oGlobalConfig, oConfig);
                aExchanges.Add(oExchange);
            }

            return aExchanges;
        }

    }
}
