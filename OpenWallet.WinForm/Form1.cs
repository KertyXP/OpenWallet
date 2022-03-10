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
            RefreshTrades(trades.OrderByDescending(t => t.dtTrade).ToList());

            button1.Enabled = true;
        }

        private string _pairSelected = "<All>";
        private void RefreshFilterDropDown()
        {


            var couples = new List<string>();
            for (int i = 0; i < dgv_trade_day.RowCount; i++)
            {
                var trade = dgv_trade_day[0, i].Value as GlobalTrade;

                if (couples.Any(c => c == trade.CustomCouple) == false)
                {
                    couples.Add(trade.CustomCouple);
                }

            }

            _pairSelected = cb_Pair.SelectedItem?.ToString() ?? _pairSelected;
            cb_Pair.Items.Clear();
            cb_Pair.Items.Add("<All>");


            cb_Pair.Items.AddRange(couples.OrderBy(c => c).ToArray());
            cb_Pair.SelectedItem = _pairSelected;


            for (int i = 0; i < dgv_trade_day.RowCount; i++)
            {
                var trade = dgv_trade_day[0, i].Value as GlobalTrade;

                if (trade.CustomCouple == _pairSelected || _pairSelected == "<All>")
                {
                    dgv_trade_day.Rows[i].Visible = true;
                }
                else
                {
                    if (dgv_trade_day.Rows[i].Selected)
                    {
                        dgv_trade_day.Rows[i].Selected = false;
                    }
                    dgv_trade_day.Rows[i].Visible = false;
                }

            }
        }

        private void RefreshTrades(List<GlobalTrade> aListTrades)
        {
            var tradeToShow = aListTrades.Where(t => groupTrades.SelectMany(g => g).Any(gt => gt.InternalExchangeId == t.InternalExchangeId) == false);
            dgv_trade_day.Rows.Clear();
            tradeToShow.ForEach(t =>
            {
                dgv_trade_day.Rows.Add(t, t.Exchange, t.Couple, t.RealFrom, t.RealQuantityFrom, t.RealTo, t.RealQuantityTo, t.RealPrice, t.dtTrade.ToString("yyyy-MM-dd"));
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[7].Style.BackColor = 
                t.IsBuy ? Color.FromArgb(255, 200, 255, 200) : Color.FromArgb(255, 255, 200, 200);
            });


            dgv_group.Rows.Clear();
            groupTrades.Where(g => g?.Any() == true).ForEach(g =>
            {
                var groupTrade = GroupTrades(g);
                dgv_group.Rows.Add(g, groupTrade.from, groupTrade.quantityFrom, groupTrade.to, groupTrade.quantityTo, g.FirstOrDefault().dtTrade.ToString("yyyy-MM-dd"));
            });

            RefreshFilterDropDown();
        }

        internal struct TradeGroupped
        {

            public string from, to;
            public double quantityFrom, quantityTo;
        }
        private static TradeGroupped GroupTrades(List<GlobalTrade> g)
        {
            TradeGroupped tradeGroupped;

            tradeGroupped.from = g.FirstOrDefault()?.From;
            tradeGroupped.to = g.FirstOrDefault()?.To;
            tradeGroupped.quantityFrom = g.Where(t => t.To == tradeGroupped.from).Sum(t => t.QuantityTo) - g.Where(t => t.From == tradeGroupped.from).Sum(t => t.QuantityFrom);
            tradeGroupped.quantityTo = g.Where(t => t.To == tradeGroupped.to).Sum(t => t.QuantityTo) - g.Where(t => t.From == tradeGroupped.to).Sum(t => t.QuantityFrom);

            return tradeGroupped;
        }

        private void dgv_trade_day_SelectionChanged(object sender, EventArgs e)
        {
            GlobalTrade t = (GlobalTrade)(sender as DataGridView).SelectedRows[0].Cells[0].Value;
            HistoryWalletToTrade(t);
        }


        private void dgv_trade_day_SelectionChanged_1(object sender, EventArgs e)
        {

            var group = new List<GlobalTrade>();
            foreach (DataGridViewRow row in dgv_trade_day.SelectedRows)
            {
                if (dgv_trade_day.Rows[row.Index].Visible == false)
                    continue;

                var trade = row.Cells[0].Value as GlobalTrade;
                group.Add(trade);
            }

            var groupTrade = GroupTrades(group);
            lbl_preview_group.Text = groupTrade.from + " " + groupTrade.quantityFrom + " || " + groupTrade.to + " " + groupTrade.quantityTo;
        }

        private void cb_Pair_SelectedIndexChanged(object sender, EventArgs e)
        {
            cb_Pair.SelectedIndexChanged -= cb_Pair_SelectedIndexChanged;
            RefreshFilterDropDown();
            cb_Pair.SelectedIndexChanged += cb_Pair_SelectedIndexChanged;
        }


        private void bt_group_Click(object sender, EventArgs e)
        {
            var group = new List<GlobalTrade>();
            foreach (DataGridViewRow row in dgv_trade_day.SelectedRows)
            {
                if (dgv_trade_day.Rows[row.Index].Visible == false)
                    continue;

                var trade = row.Cells[0].Value as GlobalTrade;
                group.Add(trade);
            }

            var couples = group.GroupBy(g => g.Couple).Select(g => g.FirstOrDefault().Couple);
            if (couples.Count() > 1)
                return;

            groupTrades.Add(group);
            Config.SaveGroupTrade(groupTrades);
            RefreshTrades(trades.OrderByDescending(t => t.dtTrade).ToList());
        }

        private void bt_ungroup_Click(object sender, EventArgs e)
        {

            foreach (DataGridViewRow row in dgv_group.SelectedRows)
            {
                var trade = row.Cells[0].Value as List<GlobalTrade>;
                groupTrades.Remove(trade);
            }

            RefreshTrades(trades.OrderByDescending(t => t.dtTrade).ToList());

            Config.SaveGroupTrade(groupTrades);
        }

        private void bt_regroup_Click(object sender, EventArgs e)
        {

            var newGroup = new List<GlobalTrade>();
            foreach (DataGridViewRow row in dgv_group.SelectedRows)
            {
                var trade = row.Cells[0].Value as List<GlobalTrade>;
                newGroup.AddRange(trade);
                groupTrades.Remove(trade);
            }

            groupTrades.Add(newGroup);
            Config.SaveGroupTrade(groupTrades);

            RefreshTrades(trades.OrderByDescending(t => t.dtTrade).ToList());
        }
    }
}
