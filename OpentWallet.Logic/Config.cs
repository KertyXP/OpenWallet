﻿using Newtonsoft.Json;
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

                if(oConfig.IsActive == false)
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
                    return new CurrencySymbolPrice()
                    {
                        From = fiat,
                        To = sTo,
                        Price = aAllCurrencies.GetCustomPrice(fiat, sTo)
                    };
                }).ToList();
            }

            return aFiatisation;
        }

        public static async Task<List<GlobalBalance>> GetBalances(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies)
        {

            ConcurrentQueue<GlobalBalance> aAll = new ConcurrentQueue<GlobalBalance>();

            var aTasks = aExchanges.Select(oExchange => Task.Run(() =>
            {
                var aBalance = oExchange.GetBalance();
                foreach (var oBalance in aBalance)
                {
                    if (oExchange.oConfig.CurrenciesToIgnore?.Any(c => c == oBalance.Crypto) == true)
                        continue;

                        oBalance.FavCrypto = Config.oGlobalConfig.FavoriteCurrency;
                    if(oBalance.Exchange == "BSC")
                    {
                        //oBalance.BitCoinValue = aAllCurrencies.GetBtcValue(oBalance);
                        oBalance.FavCryptoValue = aAllCurrencies.GetCustomValueFromBtc(oBalance, oBalance.FavCrypto);
                    }
                    else
                    {
                        oBalance.BitCoinValue = aAllCurrencies.GetBtcValue(oBalance);
                        oBalance.FavCryptoValue = aAllCurrencies.GetCustomValue(oBalance, oBalance.FavCrypto);
                    }
                    aAll.Enqueue(oBalance);
                }
            })).ToList();

            foreach (var oTask in aTasks)
            {
                await oTask;
            }

            return aAll.ToList();
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

            aListTrades = aListTrades.OrderByDescending(t => t.dtTrade).ToList();


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

        private static void CalculGainBack(GlobalTrade oTrade, List<CurrencySymbolPrice> aAllCurrencies)
        {
            var oCurrencyCouple = aAllCurrencies.FirstOrDefault(c => c.From == oTrade.To && c.To == oTrade.From && c.Exchange == oTrade.Exchange);
            oTrade.QuantityBack = oTrade.QuantityTo * oCurrencyCouple?.Price ?? 0;
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
                oTrade.QuantityFrom = oTrade.QuantityFrom * fiatFrom.Price;
                return;
            }

            if (fiatTo != null)
            {
                oTrade.To = fiatTo.To;
                oTrade.Price = oTrade.Price * fiatTo.Price;
                oTrade.QuantityTo = oTrade.QuantityTo * fiatTo.Price;
                return;
            }
        }

        public static List<GlobalTrade> ConvertTradesToDailyTrades(List<GlobalTrade> aListTrades)
        {

            return aListTrades.GroupBy(l => l.From + "|" + l.To + "_" + l.dtTrade.ToString("_yyyy-MM-dd")).Select(l =>
            {
                var one = l.FirstOrDefault();
                var oGlobalTrade = new GlobalTrade();
                oGlobalTrade.Exchange = one.Exchange;
                oGlobalTrade.QuantityBack = one.QuantityBack;
                oGlobalTrade.CryptoFromId = one.CryptoFromId;
                oGlobalTrade.CryptoToId = one.CryptoToId;
                oGlobalTrade.From = one.From;
                oGlobalTrade.To = one.To;
                oGlobalTrade.QuantityFrom = l.Sum(m => m.QuantityFrom);
                oGlobalTrade.QuantityTo = l.Sum(m => m.QuantityTo);
                oGlobalTrade.InternalExchangeId = one.InternalExchangeId;
                oGlobalTrade.dtTrade = one.dtTrade;

                oGlobalTrade.Price = oGlobalTrade.QuantityFrom / oGlobalTrade.QuantityTo;

                return oGlobalTrade;
            }).OrderBy(l => l.dtTrade).OrderBy(t => t.Couple).ToList();

        }


        public static List<GlobalTrade> ConvertTradesToGlobalTrades(List<GlobalTrade> aListTrades)
        {
            return aListTrades.GroupBy(l => l.From + "|" + l.To).Select(l =>
            {
                var one = l.FirstOrDefault();
                var oGlobalTrade = new GlobalTrade();
                oGlobalTrade.Exchange = one.Exchange;
                oGlobalTrade.CryptoFromId = one.CryptoFromId;
                oGlobalTrade.QuantityBack = one.QuantityBack;
                oGlobalTrade.CryptoToId = one.CryptoToId;
                oGlobalTrade.From = one.From;
                oGlobalTrade.To = one.To;
                oGlobalTrade.QuantityFrom = l.Sum(m => m.QuantityFrom);
                oGlobalTrade.QuantityTo = l.Sum(m => m.QuantityTo);
                oGlobalTrade.InternalExchangeId = one.InternalExchangeId;
                oGlobalTrade.dtTrade = one.dtTrade;

                oGlobalTrade.Price = oGlobalTrade.QuantityFrom / oGlobalTrade.QuantityTo;

                return oGlobalTrade;
            }).OrderBy(l => l.From).OrderBy(t => t.Couple).ToList();
        }
    }
}
