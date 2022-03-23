using Newtonsoft.Json;
using OpentWallet.Logic;
using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using OpenWallet.Logic.Abstraction.Interfaces;
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
        private ITradeService _tradeService { get; }
        private IConfigService _configService { get; }
        private IBalanceService _balanceService { get; }

        Dictionary<string, List<GlobalTrade>> archiveTrades;

        public Form1(ITradeService tradeService, IConfigService configService, IBalanceService balanceService)
        {
            var oUseless = new UseLess();
            InitializeComponent();
            _tradeService = tradeService;
            _configService = configService;
            _balanceService = balanceService;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            _configService.Init("");
            exchanges = await _configService.LoadExchangesAsync();

            allCurrencies = await _balanceService.GetCurrencriesAsync(exchanges);

            balances = _balanceService.LoadBalancesFromCacheOnly(exchanges, allCurrencies);

            InsertCurrentBalanceInGrid(balances);


            trades = _tradeService.LoadTradesFromCacheOnly(exchanges, allCurrencies);

            archiveTrades = _configService.LoadArchiveTradeFromCache();

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

            dgv_Balance.Columns[dgvBalanceCustom].HeaderText = _configService.oGlobalConfig.FavoriteCurrency;
            dgv_Balance.Rows.Insert(0, null, "TOTAL", _configService.oGlobalConfig.FavoriteCurrency, dTotalSum, dTotalSum);
        }

        private async void bt_refreshBalance_Click(object sender, EventArgs e)
        {
            bt_refreshBalance.Enabled = false;

            allCurrencies = await _balanceService.GetCurrencriesAsync(exchanges);
            balances = await _balanceService.GetBalancesAsync(exchanges, allCurrencies);

            InsertCurrentBalanceInGrid(balances);

            bt_refreshBalance.Enabled = true;
        }


        private async void bt_refreshTrade_Click(object sender, EventArgs e)
        {

            bt_refreshTrade.Enabled = false;

            List<GlobalTrade> aListTrades2 = await _tradeService.LoadTrades(exchanges, balances, allCurrencies);
            trades.Clear();
            trades.AddRange(aListTrades2);
            RefreshTrades(trades.OrderByDescending(t => t.dtTrade).ToList());

            bt_refreshTrade.Enabled = true;
        }

        private string _pairSelected = "<All>";


        private void RefreshFilterDropDown()
        {

            var couples = _tradeService.GetCouplesFromTrade(dgv_trade_day.GetValuesAsT<GlobalTrade>(AcceptedState.All));
            
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

                lbl_AdvgBuy.Text = "Avg Buy: " + _tradeService.GetAverageBuy(trades.Where(t => t.CustomCouple == _pairSelected).ToList()).ToString();
                lbl_avg_sell.Text = "Avg Sell: " + _tradeService.GetAverageSell(trades.Where(t => t.CustomCouple == _pairSelected).ToList()).ToString();
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

                if (tradeIsArchived && cb_HideArchived.Checked)
                    return;

                if(t.From == t.To)
                    return;

                var globalTradeUI = _tradeService.GetGlobalTradeUI(t, allCurrencies, tradeIsArchived);

                dgv_trade_day.Rows.Add(t, t.Exchange, t.Couple, t.RealFrom, t.RealQuantityFrom, t.RealTo, t.RealQuantityTo, t.RealPrice, globalTradeUI.Delta.ToString("00.##"), t.dtTrade.ToString("yyyy-MM-dd"));
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[7].Style.BackColor = globalTradeUI.SellStateBackColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[7].Style.SelectionBackColor = globalTradeUI.SellStateBackColorSelected;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[8].Style.BackColor = globalTradeUI.DeltaColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[8].Style.SelectionBackColor = globalTradeUI.DeltaColorSelected;

                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[3].Style.ForeColor = globalTradeUI.ArchiveStateForeColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[4].Style.ForeColor = globalTradeUI.ArchiveStateForeColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[5].Style.ForeColor = globalTradeUI.ArchiveStateForeColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[6].Style.ForeColor = globalTradeUI.ArchiveStateForeColor;
            });


            dgv_archive.Rows.Clear();
            archiveTrades.Select(kvp => kvp.Key).ForEach(g =>
            {
                var trades = archiveTrades[g];
                if (trades.Any())
                {
                    var archiveTrade = _tradeService.ArchiveTrades(trades);
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


            var archiveTrade = _tradeService.ArchiveTrades(archives);
            lbl_preview_archive.Text = archiveTrade.from + " " + archiveTrade.quantityFrom + " || " + archiveTrade.to + " " + archiveTrade.quantityTo;

            archives.AddRange(archiveTrades.GetOrDefault(archives.FirstOrDefault()?.CustomCouple) ?? new List<GlobalTrade>());
            archiveTrade = _tradeService.ArchiveTrades(archives);
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

            _configService.SaveArchiveTradeToCache(archiveTrades);
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

            _configService.SaveArchiveTradeToCache(archiveTrades);
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
                            _configService.SaveTradesToCache(newTrades);

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
