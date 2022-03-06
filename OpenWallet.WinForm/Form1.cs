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
        List<CurrencySymbolPrice> allCurrencies;
        List<GlobalBalance> balances;
        List<IExchange> exchanges;
        List<GlobalTrade> trades;
        List<List<GlobalTrade>> groupTrades;

        public Form1()
        {
            var oUseless = new UseLess();
            InitializeComponent();

        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            Config.Init("");
            exchanges = Config.LoadExchanges();

            allCurrencies = Config.GetCurrencries(exchanges);

            balances = await Config.GetBalances(exchanges, allCurrencies);

            InsertCurrentBalanceInGrid(balances);


            trades = Config.LoadTradesFromCacheOnly(exchanges, balances, allCurrencies);

            groupTrades = Config.LoadGroupTrade();

            RefreshTrades(trades.OrderByDescending(t => t.dtTrade).ToList());

        }

        private void HistoryWalletToTrade(GlobalTrade tradeToMoveTo)
        {
            var aNewBalance = JsonConvert.DeserializeObject<List<GlobalBalance>>(JsonConvert.SerializeObject(balances));

            foreach (var oTrade in trades)
            {
                if (oTrade.Exchange + oTrade.InternalExchangeId == tradeToMoveTo.Exchange + tradeToMoveTo.InternalExchangeId)
                    break;

                var from = aNewBalance.FirstOrDefault(b => b.Crypto == oTrade.From);
                var to = aNewBalance.FirstOrDefault(b => b.Crypto == oTrade.To);
                if (from == null)
                {
                    from = new GlobalBalance()
                    {
                        Crypto = oTrade.From,
                        Value = 0,
                        Exchange = oTrade.Exchange,
                        CryptoId = oTrade.CryptoFromId
                    };
                    aNewBalance.Add(from);
                }
                if (to == null)
                {
                    to = new GlobalBalance()
                    {
                        Crypto = oTrade.To,
                        Value = 0,
                        Exchange = oTrade.Exchange,
                        CryptoId = oTrade.CryptoToId
                    };
                    aNewBalance.Add(to);
                }
                from.Value += oTrade.QuantityFrom;
                to.Value -= oTrade.QuantityTo;
            }


            aNewBalance = Config.SetBitcoinFavCryptoValue(exchanges, allCurrencies, aNewBalance);

            InsertCurrentBalanceInGrid(aNewBalance);
        }

        private void InsertCurrentBalanceInGrid(List<GlobalBalance> aAll)
        {
            dgv_Balance.Rows.Clear();
            foreach (var b in aAll)
            {
                dgv_Balance.Rows.Add(b, b.Exchange, b.Crypto, b.Value, b.BitCoinValue, b.FavCryptoValue);
            }


            var dTotalSumBtc = aAll.Sum(a => a.BitCoinValue);
            var dTotalSum = aAll.Sum(a => a.FavCryptoValue);

            dgv_Balance.Columns[5].HeaderText = Config.oGlobalConfig.FavoriteCurrency;
            dgv_Balance.Rows.Insert(0, null, "TOTAL", Config.oGlobalConfig.FavoriteCurrency, dTotalSum, dTotalSumBtc, dTotalSum);
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (this.dgv_Balance.SelectedRows.Count <= 0)
                return;
            return;

            var oGlobalBalance = this.dgv_Balance.SelectedRows[0].Cells[0].Value as GlobalBalance;
            if (oGlobalBalance != null)
            {
                for (int i = 0; i < dgv_trade_day.RowCount; i++)
                {
                    var trade = dgv_trade_day[0, i].Value as GlobalTrade;
                    if (trade != null)
                    {
                        if ((string.IsNullOrEmpty(oGlobalBalance.CryptoId) == false && (trade.CryptoFromId == oGlobalBalance.CryptoId || trade.CryptoToId == oGlobalBalance.CryptoId))
                            || trade.To == oGlobalBalance.Crypto || trade.From == oGlobalBalance.Crypto)
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

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            List<GlobalTrade> aListTrades2 = await Config.LoadTrades(exchanges, balances, allCurrencies);
            trades.Clear();
            trades.AddRange(aListTrades2);
            RefreshTrades(trades);

            button1.Enabled = true;
        }

        private void RefreshTrades(List<GlobalTrade> aListTrades)
        {
            var tradeToShow = aListTrades.Where(t => groupTrades.SelectMany(g => g).Any(gt => gt.InternalExchangeId == t.InternalExchangeId) == false);
            dgv_trade_day.Rows.Clear();
            tradeToShow.ForEach(t =>
            {
                dgv_trade_day.Rows.Add(t, t.Exchange, t.Couple, t.From, t.QuantityFrom, t.To, t.QuantityTo, t.RealPrice, t.dtTrade.ToString("yyyy-MM-dd"), t.QuantityBack);
            });

            groupTrades.ForEach(g => 
            {
                var from = g.FirstOrDefault()?.From;
                var to = g.FirstOrDefault()?.To;
                var quantityFrom =  g.Where(t => t.To == from).Sum(t => t.QuantityTo) - g.Where(t => t.From == from).Sum(t => t.QuantityFrom);
                var quantityTo = g.Where(t => t.To == to).Sum(t => t.QuantityTo) - g.Where(t => t.From == to).Sum(t => t.QuantityFrom);
                dgv_group.Rows.Add(g, from, quantityFrom, to, quantityTo, g.FirstOrDefault().dtTrade.ToString("yyyy-MM-dd"));
            });

        }


        private void dgv_trade_day_SelectionChanged(object sender, EventArgs e)
        {
            GlobalTrade t = (GlobalTrade)(sender as DataGridView).SelectedRows[0].Cells[0].Value;
            HistoryWalletToTrade(t);
        }

        private void bt_group_Click(object sender, EventArgs e)
        {
            var group = new List<GlobalTrade>();
            foreach(DataGridViewRow row in dgv_trade_day.SelectedRows)
            {
                var trade = row.Cells[0].Value as GlobalTrade;
                group.Add(trade);
            }

            groupTrades.Add(group);
            Config.SaveGroupTrade(groupTrades);
            RefreshTrades(trades.OrderByDescending(t => t.dtTrade).ToList());
        }

    }
}
