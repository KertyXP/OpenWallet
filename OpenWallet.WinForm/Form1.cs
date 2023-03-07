using OpentWallet.Logic;
using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using OpenWallet.Logic.Abstraction.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenWallet.WinForm
{
    public partial class Form1 : Form
    {
        List<CurrencySymbolPrice> allCurrencies;
        List<GlobalBalance> balances;
        List<IExchange> exchanges;
        List<CurrencySymbolPrice> fiatisations;
        List<GlobalTrade> trades;
        private ITradeService _tradeService { get; }
        private IConfigService _configService { get; }
        private IBalanceService _balanceService { get; }

        Dictionary<string, List<GlobalTrade>> archiveTrades;
        const int dgvBalanceCustom = 4;


        public Form1(ITradeService tradeService, IConfigService configService, IBalanceService balanceService)
        {
            var oUseless = new UseLess();
            InitializeComponent();
            _tradeService = tradeService;
            _configService = configService;
            _balanceService = balanceService;

            cb_interval.Items.Add(Interval.Minute15);
            cb_interval.Items.Add(Interval.Hour1);
            cb_interval.Items.Add(Interval.Hour2);
            cb_interval.Items.Add(Interval.Hour4);
            cb_interval.Items.Add(Interval.Hour8);
            cb_interval.Items.Add(Interval.Hour12);
            cb_interval.Items.Add(Interval.Hour24);
            cb_interval.Text = Interval.Hour8;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            _configService.Init("");
            exchanges = await _configService.LoadExchangesAsync();

            allCurrencies = await _balanceService.GetCurrencriesAsync(exchanges);

            fiatisations = _balanceService.LoadFiatisation(allCurrencies);

            balances = _balanceService.LoadBalancesFromCacheOnly(exchanges, allCurrencies);

            InsertCurrentBalanceInGrid(balances);


            trades = _tradeService.LoadTradesFromCacheOnly(exchanges, allCurrencies);

            archiveTrades = _configService.LoadArchiveTradeFromCache();

            RefreshTrades(trades.OrderByDescending(t => t.dtTrade).ToList());

        }



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
            button1.Enabled = false;

            allCurrencies = await _balanceService.GetCurrencriesAsync(exchanges);
            balances = await _balanceService.GetBalancesAsync(exchanges, allCurrencies);


            InsertCurrentBalanceInGrid(balances);

            foreach (var balance in balances)
            {
                var couple = GetCoupleFromBalance(balance, "USDT");
                if (couple == null)
                    continue;

                var exchange = exchanges.FirstOrDefault(ex => ex.ExchangeCode == balance.Exchange);
                if (exchange is IGetTradesData getTradesData)
                {
                    try
                    {
                        var data = await getTradesData.GetTradeHistoryOneCoupleAsync(cb_interval.Text, couple);
                        _dataCharts.InsertOrUpdate(couple.CustomCouple, data);
                    }
                    catch
                    {

                    }

                }

            }

            bt_refreshBalance.Enabled = true;
            button1.Enabled = true;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            bt_refreshBalance.Enabled = false;
            bt_refreshBalance.Enabled = false;

            RefreshTrades(trades.OrderByDescending(t => t.dtTrade).ToList());

            bt_refreshBalance.Enabled = true;
            button1.Enabled = true;


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


        private async void RefreshFilterDropDown()
        {

            var couples = _tradeService.GetCouplesFromTrade(dgv_trade_day.GetValuesAsT<GlobalTrade>(AcceptedState.All));

            _pairSelected = cb_Pair.SelectedItem?.ToString() ?? _pairSelected;
            cb_Pair.Items.Clear();
            cb_Pair.Items.Add("<All>");


            cb_Pair.Items.AddRange(couples.OrderBy(c => c).ToArray());
            cb_Pair.SelectedItem = _pairSelected;

            await DrawChartFromPair(_pairSelected);

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

        private async Task<bool> DrawChartFromPair(string customCouple)
        {

            if (customCouple == "<All>")
            {
                lbl_AdvgBuy.Text = "";
                lbl_avg_sell.Text = "";
            }
            else
            {
                for (int i = 0; i < dgv_trade_day.RowCount; i++)
                {
                    var trade = dgv_trade_day[0, i].Value as GlobalTrade;
                    if (trade.CustomCouple == customCouple)
                    {

                        var exchange = exchanges.FirstOrDefault(ex => ex.ExchangeCode == trade.Exchange);
                        if (exchange is IGetTradesData getTradesData)
                        {
                            var dataChart = await getTradesData.GetTradeHistoryOneCoupleAsync(cb_interval.Text, trade);
                            DrawChart(dataChart, trade);

                            break;
                        }
                    }
                }

                lbl_AdvgBuy.Text = "Avg Buy: " + _tradeService.GetAverageBuy(trades.Where(t => t.CustomCouple == customCouple).ToList()).ToString();
                lbl_avg_sell.Text = "Avg Sell: " + _tradeService.GetAverageSell(trades.Where(t => t.CustomCouple == customCouple).ToList()).ToString();
            }

            return true;
        }

        TradesData currentDataChart;
        CurrencySymbolPrice currentTradeChart;
        private void DrawChart(TradesData dataChart, CurrencySymbolPrice trade)
        {
            currentDataChart = dataChart;
            currentTradeChart = trade;
            pb_chart.Invalidate();
        }

        private void pb_chart_Paint(object sender, PaintEventArgs e)
        {
            DrawCandlesOnGraph(e.Graphics, e.ClipRectangle, currentDataChart, currentTradeChart);
        }

        float GetYOfPrice(double price, double minPrice, double maxPrice, int Height, int YOffset)
        {
            return (float)(1 - (price - minPrice) / (maxPrice - minPrice)) * Height + YOffset;
        }

        Font smallFont = new Font(DefaultFont.Name, 5);

        private void DrawLineOnGraph(Graphics g, Rectangle r, TradesData dataChart, CurrencySymbolPrice trade)
        {

            var x = r.X + r.Width;

            var width = 5;

            double minPrice = double.MaxValue;
            double maxPrice = double.MinValue;
            DateTime startTrade = DateTime.MinValue;
            DateTime currentDay = DateTime.Now;
            foreach (var data in dataChart.Trades.OrderByDescending(t => t.dtClose))
            {

                x -= width;
                if (x < r.X)
                {
                    startTrade = data.dtOpen;
                    break;
                }


                if (data.dtOpen.DayOfYear % 2 == 1)
                {
                    g.FillRectangle(new SolidBrush(Color.Gray.SetAlpha(100)), x, r.Y, width, r.Height);
                    currentDay = data.dtOpen;
                }

                minPrice = minPrice > data.closePrice ? data.closePrice : minPrice;
                maxPrice = maxPrice < data.closePrice ? data.closePrice : maxPrice;

            }

            var realMinPrice = minPrice;
            var realMaxPrice = maxPrice;
            minPrice = Math.Min(maxPrice * 0.95, minPrice);

            string textToShow = (100 - minPrice / maxPrice * 100).ToString("0.##") + "%";
            g.DrawString(textToShow, smallFont, Brushes.White, new PointF(r.X, r.Y));


            x = r.X + r.Width;




            var currentPrice = allCurrencies.FirstOrDefault(c => c.Exchange == trade.Exchange && c.CustomCouple == trade.CustomCouple)?.Price ?? 0;
            float yCurrentPrice = GetYOfPrice(currentPrice, minPrice, maxPrice, r.Height, r.Y);
            g.DrawLine(new Pen(Brushes.Gray), r.X, yCurrentPrice, r.X + r.Width, yCurrentPrice);

            var lastPoint = new Point(-1, -1);

            foreach (var data in dataChart.Trades.OrderByDescending(t => t.dtClose))
            {

                x -= width;
                if (x < r.X)
                {
                    break;
                }

                var y1 = GetYOfPrice(data.closePrice, minPrice, maxPrice, r.Height, r.Y);

                var currentPoint = new Point(x, (int)y1);


                if (lastPoint.X > -1 && lastPoint.Y > -1)
                {
                    g.DrawLine(new Pen(new SolidBrush(Color.Gray.SetAlpha(100)), 2), lastPoint, currentPoint);
                }
                lastPoint = currentPoint;

                var circleSize = 4;

                if (realMaxPrice == data.closePrice)
                {
                    g.FillEllipse(new SolidBrush(Color.Green.SetAlpha(100)), new RectangleF(x - circleSize, y1 - circleSize, circleSize * 2, circleSize * 2));
                    //g.FillRectangle(new SolidBrush(Color.Green), new RectangleF(x, y1, r.Right - x, 1));
                }
                if (realMinPrice == data.closePrice)
                {
                    g.FillEllipse(new SolidBrush(Color.Red.SetAlpha(100)), new RectangleF(x - circleSize, y1 - circleSize, circleSize * 2, circleSize * 2));
                    //g.FillRectangle(new SolidBrush(Color.Red), new RectangleF(x, y1, r.Right - x, 1));
                }


            }


        }

        private void DrawCandlesOnGraph(Graphics g, Rectangle r, TradesData dataChart, CurrencySymbolPrice trade)
        {

            var heightGraph = r.Height - 30;
            var heightText = g.MeasureString("0123456789", DefaultFont).Height;
            var YOffset = r.Y + 10;
            var RightOffset = 50;

            g.FillRectangle(Brushes.Black, r);

            if (dataChart == null || trade == null)
                return;


            var x = r.X + r.Width - RightOffset;
            var gap = 3;
            var width = 5;

            double minPrice = double.MaxValue;
            double maxPrice = double.MinValue;
            DateTime startTrade = DateTime.MinValue;
            foreach (var data in dataChart.Trades.OrderByDescending(t => t.dtClose))
            {
                x -= gap + width;
                if (x < 0)
                {
                    startTrade = data.dtOpen;
                    break;
                }

                minPrice = minPrice > data.lowestPrice ? data.lowestPrice : minPrice;
                maxPrice = maxPrice < data.highestPrice ? data.highestPrice : maxPrice;

            }

            var realMinPrice = minPrice;
            var realMaxPrice = maxPrice;
            minPrice = Math.Min(maxPrice * 0.8, minPrice);

            string textToShow = trade.CustomCouple + " - Delta: " + (100 - minPrice / maxPrice * 100).ToString("0.##") + "%";
            g.DrawString(textToShow, DefaultFont, Brushes.White, new PointF(0, 0));


            x = r.X + r.Width - RightOffset;

            var tradesToShow = trades.Where(t => t.dtTrade >= startTrade && t.CustomCouple == trade.CustomCouple).ToList();
            DateTime currentDay = DateTime.Now;


            g.DrawLine(new Pen(Brushes.Gray), r.X, heightGraph, r.Width, heightGraph);
            g.DrawLine(new Pen(Brushes.Gray), r.X + r.Width - RightOffset, 0, r.Width - RightOffset, r.Height);


            var currentPrice = allCurrencies.FirstOrDefault(c => c.Exchange == trade.Exchange && c.CustomCouple == trade.CustomCouple)?.Price ?? 0;
            float yCurrentPrice = GetYOfPrice(currentPrice, minPrice, maxPrice, heightGraph, YOffset);
            g.DrawLine(new Pen(Brushes.Gray), r.X, yCurrentPrice, r.X + r.Width - RightOffset, yCurrentPrice);
            g.FillRectangle(new SolidBrush(Color.Gray), new RectangleF(r.X + r.Width - RightOffset, yCurrentPrice - heightText / 2 - 3, RightOffset, heightText + 4));
            g.DrawString(currentPrice.ToString(), DefaultFont, Brushes.White, new PointF(r.X + r.Width - RightOffset, yCurrentPrice - heightText / 2));


            foreach (var data in dataChart.Trades.OrderByDescending(t => t.dtClose))
            {

                if (data.dtOpen.DayOfYear != currentDay.DayOfYear)
                {
                    g.DrawLine(new Pen(new SolidBrush(Color.Gray.SetAlpha(100)), 1), x + 2, 0, x + 2, heightGraph);
                    g.DrawString(currentDay.Day.ToString(), DefaultFont, Brushes.White, new PointF(x - 2, heightGraph + 2));
                    currentDay = data.dtOpen;
                }

                x -= gap + width;
                if (x < 0)
                    break;

                var color = data.closePrice < data.openPrice ? Color.Red : Color.Green;

                float y1 = GetYOfPrice(data.highestPrice, minPrice, maxPrice, heightGraph, YOffset);
                float y2 = GetYOfPrice(data.lowestPrice, minPrice, maxPrice, heightGraph, YOffset);
                float h = y2 - y1;

                g.FillRectangle(new SolidBrush(color), new RectangleF(x + 2, y1, width - 4, h));


                if (realMaxPrice == data.highestPrice)
                {
                    g.FillRectangle(new SolidBrush(Color.Green), new RectangleF(x, y1, r.Width - x - RightOffset, 1));

                    g.FillRectangle(new SolidBrush(Color.Green), new RectangleF(r.Width - RightOffset, y1 - heightText / 2 - 3, RightOffset, heightText + 4));
                    g.DrawString(realMaxPrice.ToString(), DefaultFont, Brushes.White, new PointF(r.Width - RightOffset, y1 - heightText / 2));
                }
                if (realMinPrice == data.lowestPrice)
                {
                    g.FillRectangle(new SolidBrush(Color.Red), new RectangleF(x, y2, r.Width - x - RightOffset, 1));

                    g.FillRectangle(new SolidBrush(Color.Red), new RectangleF(r.Width - RightOffset, y2 - heightText / 2 - 3, RightOffset, heightText + 4));
                    g.DrawString(realMinPrice.ToString(), DefaultFont, Brushes.White, new PointF(r.Width - RightOffset, y2 - heightText / 2));
                }


                y1 = GetYOfPrice(Math.Max(data.openPrice, data.closePrice), minPrice, maxPrice, heightGraph, YOffset);
                y2 = GetYOfPrice(Math.Min(data.openPrice, data.closePrice), minPrice, maxPrice, heightGraph, YOffset);
                h = y2 - y1;


                g.FillRectangle(new SolidBrush(color), new RectangleF(x, y1, width, h));

                var tradesToDraw = tradesToShow.Where(t => t.dtTrade > data.dtOpen);
                foreach (var tradeToDraw in tradesToDraw)
                {
                    bool tradeIsArchived = archiveTrades.GetOrDefault(tradeToDraw.CustomCouple)?.Any(at => at.InternalExchangeId == tradeToDraw.InternalExchangeId) == true;

                    if (tradeIsArchived && cb_HideArchived.Checked)
                        continue;

                    var yTrade = GetYOfPrice(tradeToDraw.RealPrice, minPrice, maxPrice, heightGraph, YOffset);
                    var colorTrade = tradeToDraw.IsBuy ? Color.LightGreen : Color.Pink;

                    if (tradeToDraw.IsBuy)
                    {

                        PointF[] UP = new PointF[] { new PointF(x - 3, yTrade + 3), new PointF(x + 8, yTrade + 3), new PointF(x + 2, yTrade - 2) };
                        g.FillPolygon(new SolidBrush(tradeIsArchived ? Color.Gray : Color.White), UP);

                    }
                    else
                    {
                        PointF[] UP = new PointF[] { new PointF(x - 3, yTrade - 2), new PointF(x + 8, yTrade - 2), new PointF(x + 2, yTrade + 3) };
                        g.FillPolygon(new SolidBrush(tradeIsArchived ? Color.Gray : Color.White), UP);

                    }



                }
                tradesToShow.RemoveAll(t => tradesToDraw.Any(td => td.InternalExchangeId == t.InternalExchangeId));


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

                if (t.From == t.To)
                    return;

                var globalTradeUI = _tradeService.GetGlobalTradeUI(t, allCurrencies, fiatisations, tradeIsArchived);

                var currentPrice = allCurrencies.FirstOrDefault(c => c.Exchange == t.Exchange && c.CustomCouple == t.CustomCouple)?.Price;

                dgv_trade_day.Rows.Add(t, t.Exchange, t.Couple, t.RealFrom, t.RealQuantityFrom, t.RealTo, t.RealQuantityTo, t.RealPrice, currentPrice, globalTradeUI.Delta.ToString("00.##"), t.dtTrade.ToString("yyyy-MM-dd"));
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[7].Style.BackColor = globalTradeUI.SellStateBackColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[7].Style.SelectionBackColor = globalTradeUI.SellStateBackColorSelected;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[9].Style.BackColor = globalTradeUI.DeltaColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[9].Style.SelectionBackColor = globalTradeUI.DeltaColorSelected;

                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[3].Style.ForeColor = globalTradeUI.ArchiveStateForeColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[4].Style.ForeColor = globalTradeUI.ArchiveStateForeColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[5].Style.ForeColor = globalTradeUI.ArchiveStateForeColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[6].Style.ForeColor = globalTradeUI.ArchiveStateForeColor;
                dgv_trade_day.Rows[dgv_trade_day.Rows.Count - 1].Cells[7].Style.ForeColor = globalTradeUI.ArchiveStateForeColor;
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
            lbl_preview_archive.Text = "Selected: " + archiveTrade.from + " " + archiveTrade.quantityFrom + " || " + archiveTrade.to + " " + archiveTrade.quantityTo;

            archives.AddRange(archiveTrades.GetOrDefault(archives.FirstOrDefault()?.CustomCouple) ?? new List<GlobalTrade>());
            archiveTrade = _tradeService.ArchiveTrades(archives);
            lbl_prev_2.Text = "All: " + archiveTrade.from + " " + archiveTrade.quantityFrom + " || " + archiveTrade.to + " " + archiveTrade.quantityTo;


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
            if (currentTradeChart != null)
                DrawChart(currentDataChart, currentTradeChart);
        }

        private async void dgv_trade_day_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            int currentMouseOverRow = dgv_trade_day.HitTest(e.X, e.Y).RowIndex;

            if (currentMouseOverRow >= 0)
            {
                var trade = dgv_trade_day[0, currentMouseOverRow].Value as GlobalTrade;
                if (trade == null)
                    return;

                _pairSelected = trade.CustomCouple;
                this.cb_Pair.SelectedItem = _pairSelected;

                await DrawChartFromPair(trade.CustomCouple);
            }
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
                    else
                    {


                        if (dgv_trade_day.GetSelectedRowIndexes().Contains(currentMouseOverRow) == false)
                        {
                            dgv_trade_day.ClearSelection();
                            dgv_trade_day.Rows[currentMouseOverRow].Selected = true;
                        }


                        var menuItem = new MenuItem("refresh full trades from " + exchange.ExchangeName);
                        menuItem.Click += async (ob, ev) =>
                        {
                            var newTrades = await exchange.GetTradeHistoryAsync(trades.Where(tr => tr.Exchange == trade.Exchange).ToList(), balances);
                            _configService.SaveTradesToCache(newTrades);

                            trades.RemoveAll(tr => tr.Exchange == trade.Exchange);
                            trades.AddRange(newTrades);
                            RefreshTrades(trades.OrderByDescending(tr => tr.dtTrade).ToList());
                        };
                        m.MenuItems.Add(menuItem);
                    }



                    var menuItemFocus = new MenuItem("Focus on " + trade.CustomCouple);
                    menuItemFocus.Click += async (ob, ev) =>
                    {
                        _pairSelected = trade.CustomCouple;
                        this.cb_Pair.SelectedItem = _pairSelected;

                    };
                    m.MenuItems.Add(menuItemFocus);

                    m.Show(dgv_trade_day, new Point(e.X, e.Y));
                }

            }
        }

        private void dgv_archive_SelectionChanged(object sender, EventArgs e)
        {
            return;
            dgv_Balance.UnselectAllRows();
            dgv_trade_day.UnselectAllRows();
        }

        private void dgv_Balance_SelectionChanged(object sender, EventArgs e)
        {
            return;
            dgv_archive.UnselectAllRows();
            dgv_trade_day.UnselectAllRows();
        }

        private void pb_chart_Resize(object sender, EventArgs e)
        {
            pb_chart.Invalidate();
        }

        private async void bt_GetCoin_Click(object sender, EventArgs e)
        {
            var coinSelector = new CoinSelector(allCurrencies);
            coinSelector.ShowDialog();
            var couple = coinSelector.GetCouple();


            var exchange = exchanges.FirstOrDefault(ex => ex.ExchangeCode == "Binance");
            if (exchange is IRefreshOneCoupleTrade exchangeRefresh)
            {
                var newTrades = await exchangeRefresh.GetTradeHistoryOneCoupleAsync(trades.Where(tr => tr.Exchange == exchange.ExchangeCode).ToList(), couple);
                _configService.SaveTradesToCache(newTrades);

                trades.RemoveAll(tr => tr.Exchange == "Binance");
                trades.AddRange(newTrades);
                RefreshTrades(trades.OrderByDescending(tr => tr.dtTrade).ToList());
            }

        }

        private Dictionary<string, TradesData> _dataCharts = new Dictionary<string, TradesData>();

        private CurrencySymbolExchange GetCoupleFromBalance(GlobalBalance balance, string cryptoTo)
        {

            var exchange = exchanges.FirstOrDefault(ex => ex.ExchangeCode == balance.Exchange);
            if (exchange is IGetTradesData getTradesData)
            {
                var trade = allCurrencies.FirstOrDefault(c => c.Exchange == balance.Exchange && ((c.RealFrom == balance.Crypto && c.RealTo == cryptoTo) || c.RealTo == balance.Crypto && c.RealFrom == cryptoTo));

                return trade;
            }

            return null;
        }

        private void dgv_Balance_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            if (e.ColumnIndex == 5)
            {
                var cell = ((DataGridView)sender)[e.ColumnIndex, e.RowIndex] as DataGridViewImageCell;

                var g = e.Graphics;
                string cryptoToCompare = dgv_Balance.Columns[e.ColumnIndex].HeaderText;
                string cryptoFromCompare = dgv_Balance[2, e.RowIndex].Value.ToString();
                string exchangeCode = dgv_Balance[1, e.RowIndex].Value.ToString();

                var exchange = exchanges.FirstOrDefault(ex => ex.ExchangeCode == exchangeCode);
                if (exchange is IGetTradesData getTradesData)
                {
                    var trade = allCurrencies.FirstOrDefault(c => c.Exchange == exchangeCode && ((c.RealFrom == cryptoFromCompare && c.RealTo == cryptoToCompare) || c.RealTo == cryptoFromCompare && c.RealFrom == cryptoToCompare));
                    if (trade == null)
                        return;

                    if (_dataCharts.TryGetValue(trade.CustomCouple, out var dataChart))
                    {

                        e.Handled = true;

                        g.FillRectangle(Brushes.Black, e.CellBounds);

                        DrawLineOnGraph(g, new Rectangle(e.CellBounds.X + 2, e.CellBounds.Y + 2, e.CellBounds.Width - 4, e.CellBounds.Height - 4), dataChart, trade);
                    }

                }
            }
        }

        private async void cb_interval_SelectedIndexChanged(object sender, EventArgs e)
        {
            await DrawChartFromPair(_pairSelected);
        }
    }
}
