using Newtonsoft.Json;
using OpentWallet.Logic;
using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OpenWallet.WinForm
{
    public partial class Form1 : Form
    {
        List<CurrencySymbolPrice> allCurrencies;
        List<GlobalBalance> balances;
        List<IExchange> exchanges;
        List<GlobalTrade> trades;
        Dictionary<string, List<GlobalTrade>> archiveTrades;

        public Form1()
        {
            var oUseless = new UseLess();
            InitializeComponent();

        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            ConfigService.Init("");
            exchanges = await ConfigService.LoadExchangesAsync();

            allCurrencies = await BalanceService.GetCurrencriesAsync(exchanges);

            balances = BalanceService.LoadBalancesFromCacheOnly(exchanges, allCurrencies);

            InsertCurrentBalanceInGrid(balances);


            trades = TradeService.LoadTradesFromCacheOnly(exchanges, allCurrencies);

            archiveTrades = ConfigService.LoadArchiveTradeFromCache();

            RefreshTrades(trades.OrderByDescending(t => t.dtTrade).ToList());

        }


        const int dgvBalanceCustom = 4;

        private void InsertCurrentBalanceInGrid(List<GlobalBalance> aAll)
        {
            dgv_Balance.Rows.Clear();
            foreach (var b in aAll)
            {
                dgv_Balance.Rows.Add(b, b.Exchange, b.Crypto, b.Value, b.FavCryptoValue);
            }


            var dTotalSum = aAll.Sum(a => a.FavCryptoValue);

            dgv_Balance.Columns[dgvBalanceCustom].HeaderText = ConfigService.oGlobalConfig.FavoriteCurrency;
            dgv_Balance.Rows.Insert(0, null, "TOTAL", ConfigService.oGlobalConfig.FavoriteCurrency, dTotalSum, dTotalSum);
        }

        private async void bt_refreshBalance_Click(object sender, EventArgs e)
        {
            bt_refreshBalance.Enabled = false;

            allCurrencies = await BalanceService.GetCurrencriesAsync(exchanges);
            balances = await BalanceService.GetBalances(exchanges, allCurrencies);

            InsertCurrentBalanceInGrid(balances);

            bt_refreshBalance.Enabled = true;
        }


        private async void bt_refreshTrade_Click(object sender, EventArgs e)
        {

            bt_refreshTrade.Enabled = false;

            List<GlobalTrade> aListTrades2 = await TradeService.LoadTrades(exchanges, balances, allCurrencies);
            trades.Clear();
            trades.AddRange(aListTrades2);
            RefreshTrades(trades.OrderByDescending(t => t.dtTrade).ToList());

            bt_refreshTrade.Enabled = true;
        }

        private string _pairSelected = "<All>";
        private void RefreshFilterDropDown()
        {

            var couples = TradeService.GetCouplesFromTrade(dgv_trade_day.GetValuesAsT<GlobalTrade>(AcceptedState.All));
            
            _pairSelected = cb_Pair.SelectedItem?.ToString() ?? _pairSelected;
            cb_Pair.Items.Clear();
            cb_Pair.Items.Add("<All>");


            cb_Pair.Items.AddRange(couples.OrderBy(c => c).ToArray());
            cb_Pair.SelectedItem = _pairSelected;

            if (_pairSelected == "<All>")
            {
                lbl_AdvgBuy.Text = "";
                lbl_avg_sell.Text = "";
            }
            else
            {

                lbl_AdvgBuy.Text = "Avg Buy: " + TradeService.GetAverageBuy(trades.Where(t => t.CustomCouple == _pairSelected).ToList()).ToString();
                lbl_avg_sell.Text = "Avg Sell: " + TradeService.GetAverageSell(trades.Where(t => t.CustomCouple == _pairSelected).ToList()).ToString();
            }

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


            for (int i = 0; i < dgv_archive.RowCount; i++)
            {
                var trade = dgv_archive[0, i].Value as List<GlobalTrade>;

                if (trade.First()?.CustomCouple == _pairSelected || _pairSelected == "<All>")
                {
                    dgv_archive.Rows[i].Visible = true;
                }
                else
                {
                    if (dgv_archive.Rows[i].Selected)
                    {
                        dgv_archive.Rows[i].Selected = false;
                    }
                    dgv_archive.Rows[i].Visible = false;
                }

            }
        }

        private void RefreshTrades(List<GlobalTrade> aListTrades)
        {
            var tradeToShow = aListTrades;
            dgv_trade_day.Rows.Clear();
            tradeToShow.ForEach(t =>
            {
                bool tradeIsArchived = archiveTrades.GetOrDefault(t.CustomCouple)?.Any(g => g.InternalExchangeId == t.InternalExchangeId) == true;

                if (tradeIsArchived && cb_HideArchiveped.Checked)
                    return;

                if(t.From == t.To)
                    return;

                var delta = TradeService.GetDelta(t, allCurrencies);
                var isProfitable = TradeService.IsProfitable(t, delta);

                var sellStateBackColor = TradeService.GetSellStateBackColor(t);
                var sellStateBackColorSelected = TradeService.GetSellStateBackColorSelected(t);
                var archiveStateForeColor = TradeService.GetArchiveStateForeColor(tradeIsArchived);

                var deltaColor = TradeService.GetDeltaColor(isProfitable);
                var deltaColorSelected = TradeService.GetDeltaColorSelected(isProfitable);

                dgv_trade_day.Rows.Add(t, t.Exchange, t.Couple, t.RealFrom, t.RealQuantityFrom, t.RealTo, t.RealQuantityTo, t.RealPrice, delta.ToString("00.##"), t.dtTrade.ToString("yyyy-MM-dd"));
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[7].Style.BackColor = sellStateBackColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[7].Style.SelectionBackColor = sellStateBackColorSelected;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[8].Style.BackColor = deltaColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[8].Style.SelectionBackColor = deltaColorSelected;

                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[3].Style.ForeColor = archiveStateForeColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[4].Style.ForeColor = archiveStateForeColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[5].Style.ForeColor = archiveStateForeColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[6].Style.ForeColor = archiveStateForeColor;
            });


            dgv_archive.Rows.Clear();
            archiveTrades.Select(kvp => kvp.Key).ForEach(g =>
            {
                var trades = archiveTrades[g];
                if (trades.Any())
                {
                    var archiveTrade = TradeService.ArchiveTrades(trades);
                    dgv_archive.Rows.Add(trades, archiveTrade.from, archiveTrade.quantityFrom, archiveTrade.to, archiveTrade.quantityTo, trades.FirstOrDefault().dtTrade.ToString("yyyy-MM-dd"));
                }
            });

            RefreshFilterDropDown();
        }



        private void dgv_trade_day_SelectionChanged_1(object sender, EventArgs e)
        {
            dgv_archive.UnselectAllRows();
            dgv_Balance.UnselectAllRows();
            var archives = new List<GlobalTrade>();
            foreach (DataGridViewRow row in dgv_trade_day.SelectedRows)
            {
                if (dgv_trade_day.Rows[row.Index].Visible == false)
                    continue;

                var trade = row.Cells[0].Value as GlobalTrade;
                archives.Add(trade);
            }


            var archiveTrade = TradeService.ArchiveTrades(archives);
            lbl_preview_archive.Text = archiveTrade.from + " " + archiveTrade.quantityFrom + " || " + archiveTrade.to + " " + archiveTrade.quantityTo;

            archives.AddRange(archiveTrades.GetOrDefault(archives.FirstOrDefault()?.CustomCouple) ?? new List<GlobalTrade>());
            archiveTrade = TradeService.ArchiveTrades(archives);
            lbl_prev_2.Text = archiveTrade.from + " " + archiveTrade.quantityFrom + " || " + archiveTrade.to + " " + archiveTrade.quantityTo;


        }

        private void cb_Pair_SelectedIndexChanged(object sender, EventArgs e)
        {
            cb_Pair.SelectedIndexChanged -= cb_Pair_SelectedIndexChanged;
            RefreshFilterDropDown();
            cb_Pair.SelectedIndexChanged += cb_Pair_SelectedIndexChanged;
        }


        private void bt_archive_Click(object sender, EventArgs e)
        {
            var archive = new List<GlobalTrade>();
            foreach (DataGridViewRow row in dgv_trade_day.SelectedRows)
            {
                if (dgv_trade_day.Rows[row.Index].Visible == false)
                    continue;

                var trade = row.Cells[0].Value as GlobalTrade;
                archive.Add(trade);
            }

            var couples = archive.GroupBy(g => g.CustomCouple).Select(g => g.FirstOrDefault().CustomCouple);
            if (couples.Count() > 1)
                return;

            var oldArchive = archiveTrades.GetOrDefault(couples.First()) ?? new List<GlobalTrade>();
            oldArchive.AddRange(archive);
            archiveTrades.InsertOrUpdate(couples.First(), oldArchive);

            ConfigService.SaveArchiveTradeToCache(archiveTrades);
            RefreshTrades(trades.OrderByDescending(t => t.dtTrade).ToList());
        }

        private void bt_unarchive_Click(object sender, EventArgs e)
        {

            foreach (DataGridViewRow row in dgv_archive.SelectedRows)
            {

                if (dgv_archive.Rows[row.Index].Visible == false)
                    continue;

                var trade = row.Cells[0].Value as List<GlobalTrade>;
                archiveTrades.Remove(trade.First().CustomCouple);
            }

            RefreshTrades(trades.OrderByDescending(t => t.dtTrade).ToList());

            ConfigService.SaveArchiveTradeToCache(archiveTrades);
        }

        private void cb_HideArchive_CheckedChanged(object sender, EventArgs e)
        {
            RefreshTrades(trades.OrderByDescending(t => t.dtTrade).ToList());
        }

        private void dgv_trade_day_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int currentMouseOverRow = dgv_trade_day.HitTest(e.X, e.Y).RowIndex;

                if (currentMouseOverRow >= 0)
                {


                    ContextMenu m = new ContextMenu();
                    var trade = dgv_trade_day[0, currentMouseOverRow].Value as GlobalTrade;
                    if (trade == null)
                        return;

                    var exchange = exchanges.FirstOrDefault(ex => ex.ExchangeCode == trade.Exchange);
                    if (exchange is IRefreshOneCoupleTrade exchangeRefresh)
                    {

                        if (dgv_trade_day.GetSelectedRowIndexes().Contains(currentMouseOverRow) == false)
                        {
                            dgv_trade_day.ClearSelection();
                            dgv_trade_day.Rows[currentMouseOverRow].Selected = true;
                        }


                        var menuItem = new MenuItem("refresh couple " + trade.CustomCouple);
                        menuItem.Click += async (ob, ev) =>
                        {
                            var newTrades = await exchangeRefresh.GetTradeHistoryOneCoupleAsync(trades.Where(tr => tr.Exchange == trade.Exchange).ToList(), trade);
                            ConfigService.SaveTradesToCache(newTrades);

                            trades.RemoveAll(tr => tr.Exchange == trade.Exchange);
                            trades.AddRange(newTrades);
                            RefreshTrades(trades.OrderByDescending(tr => tr.dtTrade).ToList());
                        };
                        m.MenuItems.Add(menuItem);
                    }

                    m.Show(dgv_trade_day, new Point(e.X, e.Y));
                }

            }
        }

        private void dgv_archive_SelectionChanged(object sender, EventArgs e)
        {
            dgv_Balance.UnselectAllRows();
            dgv_trade_day.UnselectAllRows();
        }

        private void dgv_Balance_SelectionChanged(object sender, EventArgs e)
        {
            dgv_archive.UnselectAllRows();
            dgv_trade_day.UnselectAllRows();
        }
    }
}
