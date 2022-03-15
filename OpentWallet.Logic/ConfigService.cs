using Newtonsoft.Json;
using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace OpentWallet.Logic
{
    public class ConfigService
    {
        private static string sRootPath = "";

        public static void Init(string sFolderPath)
        {
            sRootPath = sFolderPath;
        }

        public static GlobalConfig oGlobalConfig = LoadConfig();
        private static GlobalConfig LoadConfig()
        {
            string sPath = Path.Combine(sRootPath, "config.json");
            Console.WriteLine(sPath);
            GlobalConfig oConfig = null;
            if (File.Exists(sPath))
            {
                oConfig = JsonConvert.DeserializeObject<GlobalConfig>(File.ReadAllText(sPath));
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(sPath));
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

        public static void SaveArchiveTradeToCache(Dictionary<string, List<GlobalTrade>> archiveTrades)
        {
            string sFileName = "ArchiveTrades.json";
            string json = JsonConvert.SerializeObject(archiveTrades);

            File.WriteAllText(sFileName, json);
        }

        public static Dictionary<string, List<GlobalTrade>> LoadArchiveTradeFromCache()
        {
            string sFileName = "ArchiveTrades.json";

            return File.Exists(sFileName) ? JsonConvert.DeserializeObject<Dictionary<string, List<GlobalTrade>>>(File.ReadAllText(sFileName)) : new Dictionary<string, List<GlobalTrade>>();
        }

        public static List<GlobalTrade> LoadTradesFromCache(IExchange exchange)
        {
            string sFileName = "Trades_" + exchange.ExchangeName + ".json";
            return File.Exists(sFileName) ? JsonConvert.DeserializeObject<List<GlobalTrade>>(File.ReadAllText(sFileName)) : new List<GlobalTrade>();
        }
        public static void SaveTradesToCache(List<GlobalTrade> trades)
        {
            var sJson = JsonConvert.SerializeObject(trades);
            File.WriteAllText("Trades_" + trades?.FirstOrDefault()?.Exchange + ".json", sJson);
        }

        public static void SaveBalanceToCache(IExchange exchange, List<GlobalBalance> balance)
        {
            var sJson = JsonConvert.SerializeObject(balance);
            File.WriteAllText("Balance" + exchange.ExchangeCode + ".json", sJson);
        }

        public static List<GlobalBalance> LoadBalanceFromCache(IExchange exchange)
        {
            string sFileName = "Balance" + exchange.ExchangeCode + ".json";
            return File.Exists(sFileName) ? JsonConvert.DeserializeObject<List<GlobalBalance>>(File.ReadAllText(sFileName)) : new List<GlobalBalance>();
        }

        public static List<IExchange> LoadExchanges()
        {

            List<IExchange> aExchanges = new List<IExchange>();
            var type = typeof(IExchange);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.IsClass)
                .ToList()
                .Select(t => Activator.CreateInstance(t) as IExchange)
                .Where(t => t != null);

            foreach (var oConfig in ConfigService.oGlobalConfig.configs)
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
                oExchange.Init(ConfigService.oGlobalConfig, oConfig);
                aExchanges.Add(oExchange);
            }

            return aExchanges;
        }

    }
}
