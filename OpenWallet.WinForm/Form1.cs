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

            var test = new PooCoin();
            Config.Init("");
            var aExchanges = Config.LoadExchanges();

            List<CurrencySymbolPrice> aAllCurrencies = Config.GetCurrencries(aExchanges);

            var aAll = await Config.GetBalances(aExchanges, aAllCurrencies);

            InsertCurrentBalanceInGrid(aAll);


            List<GlobalTrade> aListTrades = await Config.LoadTrades(aExchanges, aAllCurrencies);



            Config.ConvertTradesToDailyTrades(aListTrades).ForEach(t =>
            {
                dgv_trade_day.Rows.Add(t, t.Exchange, t.Couple, t.From, t.QuantityFrom, t.To, t.QuantityTo, t.dtTrade.ToString("yyyy-MM-dd"));
            });


            Config.ConvertTradesToGlobalTrades(aListTrades).ForEach(t =>
            {
                dgv_Trades.Rows.Add(t, t.Exchange, t.Couple, t.From, t.QuantityFrom, t.To, t.QuantityTo);
            });
        }

        private void InsertCurrentBalanceInGrid(List<GlobalBalance> aAll)
        {
            foreach (var b in aAll)
            {
                dgv_Balance.Rows.Add(b, b.Exchange, b.Crypto, b.Value, b.BitCoinValue, b.FavCryptoValue);
            }


            var dTotalSumBtc = aAll.Sum(a => a.BitCoinValue);
            var dTotalSum = aAll.Sum(a => a.FavCryptoValue);

            dgv_Balance.Columns[4].HeaderText = Config.oGlobalConfig.FavoriteCurrency;
            dgv_Balance.Rows.Insert(0, "TOTAL", Config.oGlobalConfig.FavoriteCurrency, dTotalSum, dTotalSumBtc, dTotalSum);
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (this.dgv_Balance.SelectedRows.Count <= 0)
                return;

            var oGlobalBalance = this.dgv_Balance.SelectedRows[0].Cells[0].Value as GlobalBalance;
            if(oGlobalBalance != null)
            {
                for(int i = 0; i < dgv_trade_day.RowCount; i++)
                {
                    var trade = dgv_trade_day[0, i].Value as GlobalTrade;
                    if(trade != null)
                    {
                        if(trade.From == oGlobalBalance.Crypto || trade.To == oGlobalBalance.Crypto || trade.CryptoId == oGlobalBalance.CryptoId)
                        {
                            dgv_trade_day.Rows[i].Visible = true;
                        }
                        else
                        {
                            dgv_trade_day.Rows[i].Visible = false;
                        }
                    }
                }
            }

        }
    }

}
