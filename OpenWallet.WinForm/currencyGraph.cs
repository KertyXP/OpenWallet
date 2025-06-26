using OpenWallet.Common;
using OpenWallet.Logic.Abstraction.Interfaces;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OpenWallet.WinForm
{
    public partial class currencyGraph : UserControl
    {
        Font smallFont = new Font(DefaultFont.Name, 5);
        private IBalanceService _balanceService;
        private ITradeService _tradeService;
        private IConfigService _configService;

        public currencyGraph()
        {
            InitializeComponent();
        }

        public void Init(IBalanceService balanceService, ITradeService tradeService, IConfigService configService)
        {

            _balanceService = balanceService;
            _tradeService = tradeService;
            _configService = configService;
        }

        private void pb_chart_Paint(object sender, PaintEventArgs e)
        {
            DrawCandlesOnGraph(e.Graphics, e.ClipRectangle, currentDataChart, currentTradeChart);
        }

        TradesData currentDataChart;
        CurrencySymbolExchange currentTradeChart;

        public void RefreshChart()
        {
            if (currentDataChart == null)
                return;

            pb.Invalidate();
        }
        public void DrawChart(TradesData dataChart, CurrencySymbolExchange trade)
        {
            currentDataChart = dataChart;
            currentTradeChart = trade;
            RefreshChart();
        }


        float GetYOfPrice(double price, double minPrice, double maxPrice, int Height, int YOffset)
        {
            return (float)(1 - (price - minPrice) / (maxPrice - minPrice)) * Height + YOffset;
        }


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




            var currentPrice = _balanceService.GetCurrencies().FirstOrDefault(c => c.Exchange == trade.Exchange && c.CutomCouple == trade.CutomCouple)?.Price ?? 0;
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

        private void DrawCandlesOnGraph(Graphics g, Rectangle r, TradesData dataChart, CurrencySymbolExchange trade)
        {
            var footerHeight = 20;
            var headerHeight = 10;
            var heightGraph = r.Height - footerHeight - headerHeight;
            var heightText = g.MeasureString("0123456789", DefaultFont).Height;
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
                minPrice = minPrice > data.closePrice ? data.closePrice : minPrice;
                minPrice = minPrice > data.openPrice ? data.openPrice : minPrice;
                maxPrice = maxPrice < data.highestPrice ? data.highestPrice : maxPrice;
                maxPrice = maxPrice < data.closePrice ? data.closePrice : maxPrice;
                maxPrice = maxPrice < data.openPrice ? data.openPrice : maxPrice;

            }

            var currentPrice = _balanceService.GetCurrencies().FirstOrDefault(c => c.Exchange == trade.Exchange && c.CutomCouple == trade.CutomCouple)?.Price ?? 0;

            minPrice = minPrice > currentPrice ? currentPrice : minPrice;
            maxPrice = maxPrice < currentPrice ? currentPrice : maxPrice;

            var realMinPrice = minPrice;
            var realMaxPrice = maxPrice;
            minPrice = Math.Min(maxPrice * 0.8, minPrice);

            string textToShow = trade.CutomCouple + " - Delta: " + (100 - minPrice / maxPrice * 100).ToString("0.##") + "%";
            g.DrawString(textToShow, DefaultFont, Brushes.White, new PointF(0, 0));
            x = r.X + r.Width - RightOffset;

            var tradetoShow = _tradeService.GetTrades().Where(t => t.dtTrade >= startTrade && t.CutomCouple == trade.CutomCouple).ToList();
            DateTime currentDay = DateTime.Now;


            g.DrawLine(new Pen(Brushes.Gray), r.X, heightGraph, r.Width, heightGraph);
            g.DrawLine(new Pen(Brushes.Gray), r.X + r.Width - RightOffset, 0, r.Width - RightOffset, r.Height);


            float yCurrentPrice = GetYOfPrice(currentPrice, minPrice, maxPrice, heightGraph, headerHeight);
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

                float y1 = GetYOfPrice(data.highestPrice, minPrice, maxPrice, heightGraph, headerHeight);
                float y2 = GetYOfPrice(data.lowestPrice, minPrice, maxPrice, heightGraph, headerHeight);
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


                y1 = GetYOfPrice(Math.Max(data.openPrice, data.closePrice), minPrice, maxPrice, heightGraph, headerHeight);
                y2 = GetYOfPrice(Math.Min(data.openPrice, data.closePrice), minPrice, maxPrice, heightGraph, headerHeight);
                h = y2 - y1;


                g.FillRectangle(new SolidBrush(color), new RectangleF(x, y1, width, h));

                var tradetoDraw = tradetoShow.Where(t => t.dtTrade > data.dtOpen);
                foreach (var tradeToDraw in tradetoDraw)
                {
                    bool tradeIsArchived = _configService.GetArchiveTrades().ContainsKey(tradeToDraw.Key);

                    //if (tradeIsArchived && _configService.GetHideArchive())
                    //    continue;

                    var yTrade = GetYOfPrice(tradeToDraw.RealPrice, minPrice, maxPrice, heightGraph, headerHeight);
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
                tradetoShow.RemoveAll(t => tradetoDraw.Any(td => td.InternalExchangeId == t.InternalExchangeId));


            }
        }

    }
}
