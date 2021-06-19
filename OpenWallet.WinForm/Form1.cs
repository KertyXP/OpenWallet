using Newtonsoft.Json;
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

            var aExchanges = Config.LoadExchanges();

            List<CurrencySymbolPrice> aAllCurrencies = Config.GetCurrencries(aExchanges);

            var aAll = await Config.GetBalances(aExchanges, aAllCurrencies);

            InsertCurrentBalanceInGrid(aAll);


            List<GlobalTrade> aListTrades = await Config.LoadTrades(aExchanges, aAllCurrencies);



            Config.ConvertTradesToDailyTrades(aListTrades).ForEach(t =>
            {
                dgv_trade_day.Rows.Add(t.Exchange, t.Couple, t.From, t.QuantityFrom, t.To, t.QuantityTo, t.dtTrade.ToString("yyyy-MM-dd"));
            });


            Config.ConvertTradesToGlobalTrades(aListTrades).ForEach(t =>
            {
                dgv_Trades.Rows.Add(t.Exchange, t.Couple, t.From, t.QuantityFrom, t.To, t.QuantityTo);
            });
        }

        private void InsertCurrentBalanceInGrid(List<GlobalBalance> aAll)
        {
            foreach (var b in aAll)
            {
                dataGridView1.Rows.Add(b.Exchange, b.Crypto, b.Value, b.BitCoinValue, b.FavCryptoValue);
            }


            var dTotalSumBtc = aAll.Sum(a => a.BitCoinValue);
            var dTotalSum = aAll.Sum(a => a.FavCryptoValue);

            dataGridView1.Columns[4].HeaderText = Config.oGlobalConfig.FavoriteCurrency;
            dataGridView1.Rows.Insert(0, "TOTAL", Config.oGlobalConfig.FavoriteCurrency, dTotalSum, dTotalSumBtc, dTotalSum);
        }
    }

}
