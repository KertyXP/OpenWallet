﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpentWallet.Logic;
using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenWallet.WinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            var oUseless = new UseLess();
            InitializeComponent();

        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            await Task.Delay(1000);
            GlobalConfig oGlobalConfig = null;
            if (File.Exists("config.json"))
            {
                oGlobalConfig = JsonConvert.DeserializeObject<GlobalConfig>(File.ReadAllText("config.json"));
            }
            else
            {
                oGlobalConfig = new GlobalConfig();
                oGlobalConfig.aConfigs = new List<ExchangeConfig>()
                {
                    new ExchangeConfig()
                    {
                        ExchangeCode = "Binance",
                        ApiKey = "ApiKey",
                        SecretKey = "SecretKey",
                        AdditionnalKey = ""
                    }
                };
                File.WriteAllText("config.json", JsonConvert.SerializeObject(oGlobalConfig));
            }
            oGlobalConfig.FiatMoneys = oGlobalConfig.FiatMoneys.GroupBy(c => c).Select(c => c.FirstOrDefault()).ToList();

            List<IExchange> aEchanges = new List<IExchange>();
            var type = typeof(IExchange);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.IsClass)
                .ToList()
                .Select(t => Activator.CreateInstance(t) as IExchange)
                .Where(t => t != null);

            foreach (var oConfig in oGlobalConfig.aConfigs)
            {
                var typeExchange = types.FirstOrDefault(t => t.ExchangeCode == oConfig.ExchangeCode);
                if (typeExchange == null)
                    continue; // oops

                var oExchange = Activator.CreateInstance(typeExchange.GetType()) as IExchange;
                if (oExchange == null)
                    continue; // re-oops

                oExchange.Init(oGlobalConfig, oConfig);
                aEchanges.Add(oExchange);
            }



            var aTasks = aEchanges.Select(oExchange => Task.Run(oExchange.GetBalance)).ToList();
            List<GlobalBalance> aAll = new List<GlobalBalance>();

            //foreach (var oTask in aTasks)
            //{
            //    if(oTask.Status == TaskStatus.WaitingForActivation)
            //    {
            //        oTask.Start();
            //    }
            //}

            foreach (var oTask in aTasks)
            {
                var aBalance = oTask.GetAwaiter().GetResult();
                aAll.AddRange(aBalance);
            }

            List<CurrencySymbolPrice> aAllCurrencies = new List<CurrencySymbolPrice>();
            var aTaskCurrencies = aEchanges.Select(oExchange => Task.Run(oExchange.GetCurrencies)).ToList();

            foreach (var oTask in aTaskCurrencies)
            {
                var aBalance = oTask.GetAwaiter().GetResult();
                aAllCurrencies.AddRange(aBalance);
            }

            List<CurrencySymbolPrice> aFiatisation = new List<CurrencySymbolPrice>();
            // fiatisation (needs at least 2 fiats Moneys (first is the fiatisation TO)
            if (oGlobalConfig.FiatMoneys.Count() > 1)
            {
                string sTo = oGlobalConfig.FiatMoneys.FirstOrDefault();
                aFiatisation = oGlobalConfig.FiatMoneys.Skip(1).Select(fiat =>
                {
                    return new CurrencySymbolPrice()
                    {
                        From = fiat,
                        To = sTo,
                        Price = aAllCurrencies.GetCustomPrice(fiat, sTo)
                    };
                }).ToList();
            }

            aAll.ForEach(b =>
            {
                b.BitCoinValue = aAllCurrencies.GetBtcValue(b);
                b.FavCrypto = oGlobalConfig.FavoriteCurrency;
                b.FavCryptoValue = aAllCurrencies.GetCustomValue(b, b.FavCrypto);

                dataGridView1.Rows.Add(b.Exchange, b.Crypto, b.Value, b.BitCoinValue, b.FavCryptoValue);
            });

            dataGridView1.Columns[4].HeaderText = oGlobalConfig.FavoriteCurrency;

            var dTotalSumBtc = aAll.Sum(a => a.BitCoinValue);
            var dTotalSum = aAll.Sum(a => a.FavCryptoValue);

            dataGridView1.Rows.Insert(0, "TOTAL", oGlobalConfig.FavoriteCurrency, dTotalSum, dTotalSumBtc, dTotalSum);




            var aTasks3 = aEchanges.Select(oExchange =>
            {
                string sFileName = "Trades_" + oExchange.ExchangeName + ".json";

                var aTrades = File.Exists(sFileName) ? JsonConvert.DeserializeObject<List<GlobalTrade>>(File.ReadAllText(sFileName)) : new List<GlobalTrade>();
                return Task.Run(() => oExchange.GetTradeHistory(aTrades));
            }).ToList();
            List<GlobalTrade> aListTrades = new List<GlobalTrade>();

            foreach (var oTask in aTasks3)
            {
                try
                {
                    var aBalance = await oTask;
                    if (aBalance.Any() == false)
                        continue;

                    File.WriteAllText("Trades_" + aBalance?.FirstOrDefault()?.Exchange + ".json", JsonConvert.SerializeObject(aBalance));

                    aListTrades.AddRange(aBalance);
                }
                catch
                {

                }
            }

            aListTrades.ForEach(trade =>
            {
                var fiat = aFiatisation.FirstOrDefault(f => f.From == trade.From);
                if (fiat != null)
                {
                    trade.From = fiat.To;
                    trade.Price = trade.Price / fiat.Price;
                    trade.QuantityFrom = trade.QuantityFrom * fiat.Price;
                    return;
                }

                fiat = aFiatisation.FirstOrDefault(f => f.From == trade.To);
                if (fiat != null)
                {
                    trade.To = fiat.To;
                    trade.Price = trade.Price * fiat.Price;
                    trade.QuantityFrom = trade.QuantityFrom / fiat.Price;
                    return;
                }
            });


            var aListTrades2 = aListTrades.GroupBy(l => l.From + "|" + l.To + "_" + l.dtTrade.ToString("_yyyy-MM-dd")).Select(l =>
            {
                var one = l.FirstOrDefault();
                var oGlobalTrade = new GlobalTrade();
                oGlobalTrade.Exchange = one.Exchange;
                oGlobalTrade.From = one.From;
                oGlobalTrade.To = one.To;
                oGlobalTrade.QuantityFrom = l.Sum(m => m.QuantityFrom);
                oGlobalTrade.QuantityTo = l.Sum(m => m.QuantityTo);
                oGlobalTrade.InternalExchangeId = one.InternalExchangeId;
                oGlobalTrade.dtTrade = one.dtTrade;

                oGlobalTrade.Price = oGlobalTrade.QuantityFrom / oGlobalTrade.QuantityTo;

                return oGlobalTrade;
            }).OrderBy(l => l.dtTrade).ToList();

            aListTrades2.OrderBy(t => t.Couple).ToList().ForEach(t =>
            {
                dgv_trade_day.Rows.Add(t.Exchange, t.Couple, t.From, t.QuantityFrom, t.To, t.QuantityTo, t.dtTrade.ToString("yyyy-MM-dd"));
            });

            var aListTrades3 = aListTrades.GroupBy(l => l.From + "|" + l.To).Select(l =>
            {
                var one = l.FirstOrDefault();
                var oGlobalTrade = new GlobalTrade();
                oGlobalTrade.Exchange = one.Exchange;
                oGlobalTrade.From = one.From;
                oGlobalTrade.To = one.To;
                oGlobalTrade.QuantityFrom = l.Sum(m => m.QuantityFrom);
                oGlobalTrade.QuantityTo = l.Sum(m => m.QuantityTo);
                oGlobalTrade.InternalExchangeId = one.InternalExchangeId;
                oGlobalTrade.dtTrade = one.dtTrade;

                oGlobalTrade.Price = oGlobalTrade.QuantityFrom / oGlobalTrade.QuantityTo;

                return oGlobalTrade;
            }).OrderBy(l => l.From).ToList();

            aListTrades3.OrderBy(t => t.Couple).ToList().ForEach(t =>
            {
                dgv_Trades.Rows.Add(t.Exchange, t.Couple, t.From, t.QuantityFrom, t.To, t.QuantityTo);
            });
        }
    }

}
