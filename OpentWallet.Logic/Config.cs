using Newtonsoft.Json;
using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OpentWallet.Logic
{
    public class Config
    {
        private static string sRootPath = "";
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
                oConfig.aConfigs = new List<ExchangeConfig>()
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

        public static List<List<GlobalTrade>> LoadGroupTrade()
        {
            string sFileName = "GroupTrades.json";

            var groupTrades = File.Exists(sFileName) ? JsonConvert.DeserializeObject<List<List<GlobalTrade>>>(File.ReadAllText(sFileName)) : new List<List<GlobalTrade>>();
            for(int i = groupTrades.Count - 1; i > 0; i--)
            {
                for(int j = i - 1; i >= 0; i--)
                {
                    if (i == j)
                        continue;

                    if(groupTrades[i].Any(gtSource => groupTrades[j].Any(gtDest => gtDest.InternalExchangeId == gtSource.InternalExchangeId)))
                    {
                        groupTrades.RemoveAt(i);
                        break;
                    }
                }
            }
            return groupTrades;
        }
        public static void SaveGroupTrade(List<List<GlobalTrade>> groupTrades)
        {
            string sFileName = "GroupTrades.json";
            string json = JsonConvert.SerializeObject(groupTrades);

            File.WriteAllText(sFileName, json);
        }

        public static void Init(string sFolderPath)
        {
            sRootPath = sFolderPath;
            //Xamarin.Essentials.FileSystem.AppDataDirectory
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

            foreach (var oConfig in Config.oGlobalConfig.aConfigs)
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
                oExchange.Init(Config.oGlobalConfig, oConfig);
                aExchanges.Add(oExchange);
            }

            return aExchanges;
        }

        public static List<CurrencySymbolPrice> GetCurrencries(List<IExchange> aExchanges)
        {

            List<CurrencySymbolPrice> aAllCurrencies = new List<CurrencySymbolPrice>();
            var aTaskCurrencies = aExchanges.Select(oExchange => Task.Run(oExchange.GetCurrencies)).ToList();

            foreach (var oTask in aTaskCurrencies)
            {
                var aBalance = oTask.GetAwaiter().GetResult();
                aAllCurrencies.AddRange(aBalance);
            }

            return aAllCurrencies;
        }

        public static List<CurrencySymbolPrice> LoadFiatisation(List<CurrencySymbolPrice> aAllCurrencies)
        {

            List<CurrencySymbolPrice> aFiatisation = new List<CurrencySymbolPrice>();
            // fiatisation (needs at least 2 fiats Moneys (first is the fiatisation TO)
            if (oGlobalConfig.FiatMoneys.Count() > 1)
            {
                string sTo = Config.oGlobalConfig.FiatMoneys.FirstOrDefault();
                aFiatisation = Config.oGlobalConfig.FiatMoneys.Skip(1).Select(fiat =>
                {
                    return new CurrencySymbolPrice(fiat, sTo, aAllCurrencies.GetCustomPrice(fiat, sTo), string.Empty, string.Empty);
                }).ToList();
            }

            return aFiatisation;
        }

        public static List<GlobalBalance> SetBitcoinFavCryptoValue(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies, List<GlobalBalance> aAllBalances)
        {

            foreach (var oBalance in aAllBalances)
            {
                var oExchange = aExchanges.FirstOrDefault(e => e.ExchangeCode == oBalance.Exchange);

                if (oExchange.oConfig.CurrenciesToIgnore?.Any(c => c == oBalance.Crypto) == true)
                    continue;

                oBalance.FavCrypto = Config.oGlobalConfig.FavoriteCurrency;
                if (oBalance.Exchange == "BSC")
                {
                    //oBalance.BitCoinValue = aAllCurrencies.GetBtcValue(oBalance);
                    oBalance.FavCryptoValue = aAllCurrencies.GetCustomValueFromBtc(oBalance, oBalance.FavCrypto);
                }
                else
                {
                    oBalance.BitCoinValue = aAllCurrencies.GetBtcValue(oBalance);
                    oBalance.FavCryptoValue = aAllCurrencies.GetCustomValue(oBalance, oBalance.FavCrypto);
                }
            }

            return aAllBalances;
        }

        public static async Task<List<GlobalBalance>> GetBalances(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies)
        {

            ConcurrentQueue<GlobalBalance> aAll = new ConcurrentQueue<GlobalBalance>();

            var aTasks = aExchanges.Select(oExchange => Task.Run(() =>
            {
                var aBalance = oExchange.GetBalance();

                var sJson = JsonConvert.SerializeObject(aBalance);
                File.WriteAllText("Balance" + oExchange.ExchangeCode + ".json", sJson);

                aBalance = SetBitcoinFavCryptoValue(aExchanges, aAllCurrencies, aBalance);
                foreach (var oBalance in aBalance)
                {
                    if (oExchange.oConfig.CurrenciesToIgnore?.Any(c => c == oBalance.Crypto) == true)
                        continue;

                    aAll.Enqueue(oBalance);
                }
            })).ToList();

            foreach (var oTask in aTasks)
            {
                await oTask;
            }



            return aAll.ToList();
        }
        public static List<GlobalBalance> LoadBalancesFromCacheOnly(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies)
        {

            ConcurrentQueue<GlobalBalance> aAll = new ConcurrentQueue<GlobalBalance>();


                var aBalance = aExchanges.Select(oExchange =>
                {
                    string sFileName = "Balance" + oExchange.ExchangeCode + ".json";

                    return File.Exists(sFileName) ? JsonConvert.DeserializeObject<List<GlobalBalance>>(File.ReadAllText(sFileName)) : new List<GlobalBalance>();
                }).SelectMany(b => b).ToList();


            aBalance = SetBitcoinFavCryptoValue(aExchanges, aAllCurrencies, aBalance);

            return aBalance;
        }

        public static List<GlobalTrade> LoadTradesFromCacheOnly(List<IExchange> aExchanges, List<GlobalBalance> aAllBalances, List<CurrencySymbolPrice> aAllCurrencies)
        {
            List<CurrencySymbolPrice> aFiatisation = Config.LoadFiatisation(aAllCurrencies);

            var aListTrades = aExchanges.Select(oExchange =>
            {
                string sFileName = "Trades_" + oExchange.ExchangeName + ".json";

                return File.Exists(sFileName) ? JsonConvert.DeserializeObject<List<GlobalTrade>>(File.ReadAllText(sFileName)) : new List<GlobalTrade>();
            })
                .SelectMany(s => s)
                .OrderByDescending(s => s.dtTrade)
                .GroupBy(l => l.InternalExchangeId + l.Exchange)
                .Select(l => l.FirstOrDefault())
                .ToList();


            aListTrades.ForEach(trade =>
            {
                FiatiseOneTrade(trade, aFiatisation);
            });

            aListTrades.ForEach(trade =>
            {
                CalculGainBack(trade, aAllCurrencies);
            });

            return aListTrades;
        }

        public static async Task<List<GlobalTrade>> LoadTrades(List<IExchange> aExchanges, List<GlobalBalance> aAllBalances, List<CurrencySymbolPrice> aAllCurrencies)
        {

            List<CurrencySymbolPrice> aFiatisation = Config.LoadFiatisation(aAllCurrencies);

            var aTasks3 = aExchanges.Select(oExchange =>
            {
                string sFileName = "Trades_" + oExchange.ExchangeName + ".json";

                var aTrades = File.Exists(sFileName) ? JsonConvert.DeserializeObject<List<GlobalTrade>>(File.ReadAllText(sFileName)) : new List<GlobalTrade>();
                return Task.Run(() => oExchange.GetTradeHistory(aTrades, aAllBalances));
            }).ToList();
            List<GlobalTrade> aListTrades = new List<GlobalTrade>();

            foreach (var oTask in aTasks3)
            {
                try
                {
                    var aBalance = await oTask;
                    if (aBalance.Any() == false)
                        continue;

                    var sJson = JsonConvert.SerializeObject(aBalance);
                    File.WriteAllText("Trades_" + aBalance?.FirstOrDefault()?.Exchange + ".json", sJson);

                    aListTrades.AddRange(aBalance);
                }
                catch
                {

                }
            }

            aListTrades = aListTrades
                .OrderByDescending(s => s.dtTrade)
                .GroupBy(l => l.InternalExchangeId + l.Exchange)
                .Select(l => l.FirstOrDefault())
                .ToList(); ;


            aListTrades.ForEach(trade =>
            {
                FiatiseOneTrade(trade, aFiatisation);
            });

            aListTrades.ForEach(trade =>
            {
                CalculGainBack(trade, aAllCurrencies);
            });

            return aListTrades;


        }
    /// <summary>
    /// not working
    /// </summary>
    /// <param name="oTrade"></param>
    /// <param name="aAllCurrencies"></param>
        private static void CalculGainBack(GlobalTrade oTrade, List<CurrencySymbolPrice> aAllCurrencies)
        {
            var oCurrencyCouple = aAllCurrencies.FirstOrDefault(c => c.From == oTrade.To && c.To == oTrade.From && c.Exchange == oTrade.Exchange);
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

        public static List<GlobalTrade> ConvertTradesToDailyTrades(List<GlobalTrade> aListTrades)
        {

            return aListTrades.GroupBy(l => l.From + "|" + l.To + "_" + l.dtTrade.ToString("_yyyy-MM-dd")).Select(l =>
            {
                var one = l.FirstOrDefault();
                double price = one.Couple.StartsWith(one.From) ? one.QuantityTo / one.QuantityFrom : one.QuantityFrom / one.QuantityTo;
                var oGlobalTrade = new GlobalTrade(one.From, one.To, price, one.Couple, one.Exchange);
                oGlobalTrade.CryptoFromId = one.CryptoFromId;
                oGlobalTrade.CryptoToId = one.CryptoToId;
                oGlobalTrade.SetQuantities(l.Sum(m => m.QuantityFrom), l.Sum(m => m.QuantityTo));

                oGlobalTrade.InternalExchangeId = one.InternalExchangeId;
                oGlobalTrade.dtTrade = one.dtTrade;

                return oGlobalTrade;
            }).OrderByDescending(l => l.dtTrade).OrderBy(t => t.Couple).ToList();

        }


        public static List<GlobalTrade> ConvertTradesToGlobalTrades(List<GlobalTrade> aListTrades)
        {
            return aListTrades.GroupBy(l => l.From + "|" + l.To).Select(l =>
            {
                var one = l.FirstOrDefault();
                var oGlobalTrade = new GlobalTrade(one.From, one.To, l.Sum(m => m.QuantityFrom) / l.Sum(m => m.QuantityTo), one.Couple, one.Exchange);
                oGlobalTrade.CryptoFromId = one.CryptoFromId;
                oGlobalTrade.CryptoToId = one.CryptoToId;
                oGlobalTrade.SetQuantities(l.Sum(m => m.QuantityFrom), l.Sum(m => m.QuantityTo));
                oGlobalTrade.InternalExchangeId = one.InternalExchangeId;
                oGlobalTrade.dtTrade = one.dtTrade;

                return oGlobalTrade;
            }).OrderBy(l => l.From).OrderBy(t => t.Couple).ToList();
        }
    }
}
