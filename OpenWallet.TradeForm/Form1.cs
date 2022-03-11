using OpentWallet.Logic;
using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenWallet.TradeForm
{
    public partial class Form1 : Form
    {
        List<CurrencySymbolPrice> aAllCurrencies;
        List<GlobalBalance> aAllBalances;
        List<IExchange> aExchanges;
        double quantityFrom = 0;

        public Form1()
        {
            InitializeComponent();
            this.cb_From.DrawMode = DrawMode.OwnerDrawFixed;
            this.cb_to.DrawMode = DrawMode.OwnerDrawFixed;

        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            Config.Init("");
            aExchanges = Config.LoadExchanges();
            await RefreshCurrenciesAndBalance();

        }

        private async Task RefreshCurrenciesAndBalance()
        {
            aAllCurrencies = Config.GetCurrencries(aExchanges);
            aAllBalances = await Config.GetBalances(aExchanges, aAllCurrencies);

            cb_From.Items.Clear();

            aAllCurrencies.Where(c => c.Exchange == "Binance").ForEach(c =>
            {
                var balance = GetBalance(c.From);
                if (balance <= 0)
                    return;

                if (cb_From.Items.Contains(c.From) == false)
                    cb_From.Items.Add(c.From);
            });
        }

        Font myFont = new Font("Aerial", 10, FontStyle.Regular);

        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            var cb = sender as ComboBox;
            var sText = cb.Items[e.Index].ToString();
            var balance = GetBalance(sText);

            var nOffset = 70;
            e.DrawBackground();
            e.Graphics.DrawString($"[{Math.Round(balance, 3)}]", myFont, Brushes.Black, e.Bounds);
            e.Graphics.DrawString(cb.Items[e.Index].ToString(), myFont, Brushes.Black, new Rectangle(e.Bounds.X + nOffset, e.Bounds.Y, e.Bounds.Width - nOffset, e.Bounds.Height));
            e.DrawFocusRectangle();
        }

        void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_From.SelectedIndex == 1 || cb_From.SelectedIndex == 4 || cb_From.SelectedIndex == 5)
                cb_From.SelectedIndex = -1;
        }

        double GetBalance(string sCrypto)
        {
            return GetTrade(sCrypto)?.Value ?? 0;
        }

        GlobalBalance GetTrade(string sCrypto)
        {
            return aAllBalances?.FirstOrDefault(c => c.Exchange == "Binance" && c.Crypto.ToUpper() == sCrypto.ToUpper());
        }

        private void calculSwap()
        {
            var valueFrom = quantityFrom;
            var sFrom = cb_From.Text;
            var sTo = cb_to.Text;
            var pair = aAllCurrencies.FirstOrDefault(c => c.Exchange == "Binance" && c.From.ToUpper() == sFrom.ToUpper() && c.To.ToUpper() == sTo.ToUpper());
            var oBalanceFrom = GetTrade(sFrom);
            lbl_qtty_from.Text = GetBalance(cb_From.Text).ToString();
            lbl_qtty_to.Text = GetBalance(cb_to.Text).ToString();
            lbl_couple.Text = pair?.Couple;
            lbl_valueUsdt.Text = aAllCurrencies.GetCustomValue(oBalanceFrom, oBalanceFrom.FavCrypto, valueFrom + 0).ToString() + " " + oBalanceFrom.FavCrypto;


            if (pair == null)
            {
                lbl_currentPrice.Text = "???";
            }
            else
            {
                lbl_currentPrice.Text = pair.Price.ToString();
            }

            if (pair == null)
            {
                nud_To.Value = 0;
            }
            else
            {
                nud_To.Value = (decimal)valueFrom * (decimal)pair.Price;
            }
        }

        private void cb_From_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sFrom = cb_From.Text;
            this.cb_to.Enabled = aAllCurrencies.Any(c => c.Exchange == "Binance" && c.From.ToUpper() == sFrom.ToUpper());
            this.cb_to.Items.Clear();
            aAllCurrencies.Where(c => c.Exchange == "Binance" && c.From.ToUpper() == sFrom.ToUpper())
                .ForEach(c => { cb_to.Items.Add(c.To); });
            if (this.cb_to.Items.Count == 0)
                return;

            this.cb_to.Text = this.cb_to.Items[0].ToString();
            calculSwap();
        }

        private void cb_to_SelectedIndexChanged(object sender, EventArgs e)
        {
            calculSwap();
        }

        private void bt_swap_Click(object sender, EventArgs e)
        {
            var quantityTo = nud_To.Text.ToDouble();
            var oBinanceApi = aExchanges.FirstOrDefault(exc => exc.ExchangeCode == "Binance");
            if (oBinanceApi == null)
                return;

            //var sFrom = cb_From.Text;
            //var sTo = cb_to.Text;
            var pair = aAllCurrencies.FirstOrDefault(c => c.Exchange == "Binance" && c.From.ToUpper() == cb_From.Text.ToUpper() && c.To.ToUpper() == cb_to.Text.ToUpper());

            bool CoupleOrderRespected = pair.Couple.StartsWith(cb_From.Text.ToUpper());
            double quantityToSwap = CoupleOrderRespected ? quantityFrom : quantityTo;
            double quantitySwapApprox = CoupleOrderRespected == false ? quantityFrom : quantityTo;
            var sellBuy = CoupleOrderRespected == true ? SellBuy.Sell : SellBuy.Buy;

            var result = oBinanceApi?.PlaceMarketOrder(pair, quantityToSwap, sellBuy, cb_Test.Checked);
            string sSuccess = result != null ? "Success!" : "FAILURE!!";

            MessageBox.Show($"{sSuccess} - Swapped {result?.QuantityFrom} {result?.From} with {result?.To} {result?.QuantityTo}");

        }

        private void cb_TextChanged(object sender, EventArgs e)
        {
            var cb = sender as ComboBox;
            int nIndex = 0;
            var currentText = cb.Text.ToUpper();

            foreach (var sItem in cb.Items)
            {
                if (sItem.ToString().ToUpper().StartsWith(currentText))
                {
                    cb.SelectedIndex = nIndex;
                    cb.SelectionStart = currentText.Length;
                    cb.SelectionLength = sItem.ToString().Length - cb.SelectionStart;
                    cb.Tag = sItem;
                    break;
                }
                nIndex++;
            }
        }

        private void cb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
            {
                var cb = sender as ComboBox;
                cb.TextChanged -= cb_TextChanged;
            }

            if (e.KeyCode == Keys.Enter)
            {
                bt_swap_Click(this, new EventArgs());
            }
        }

        private void cb_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Back)
            {
                var cb = sender as ComboBox;
                cb.TextChanged += cb_TextChanged;
            }
        }


        private void pb_refresh_Click(object sender, EventArgs e)
        {
            aAllCurrencies = Config.GetCurrencries(aExchanges);
            calculSwap();
        }

        private void nud_ValueChanged(object sender, EventArgs e)
        {
            nud_From.ValueChanged -= nud_ValueChanged;
            //var nud = sender as NumericUpDown;
            //var currentText = nud.Text.ToUpper();

            ////nud.Select(currentText.Length;
            ////nud.SelectionLength = sItem.ToString().Length - nud.SelectionStart;
            nud_From.Value = (decimal)quantityFrom;
            calculSwap();
            nud_From.ValueChanged += nud_ValueChanged;

        }

        private void nud_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Back)
            {
                var nud = sender as NumericUpDown;
                nud.TextChanged -= nud_ValueChanged;
            }

            if (e.KeyCode == Keys.Enter)
            {
                bt_swap_Click(this, new EventArgs());
            }
        }

        void InvokeOnMainThread(Action a)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => a()));
                return;
            }
            a();
        }

        private void nud_KeyUp(object sender, KeyEventArgs e)
        {
            quantityFrom = nud_From.Text.ToDouble();
            calculSwap();


            if (e.KeyCode == Keys.Back)
            {
                var nud = sender as NumericUpDown;
                nud.TextChanged += nud_ValueChanged;
            }
        }

        private void pb_swap_Click(object sender, MouseEventArgs e)
        {
            var sFrom = cb_to.Text;
            var sTo = cb_From.Text;
            var sQuantityFrom = nud_To.Value;
            cb_From.Text = sFrom;
            cb_to.Text = sTo;
            quantityFrom = (double)sQuantityFrom;
            nud_From.Text = sQuantityFrom.ToString();
            nud_From.Value = sQuantityFrom;
        }

        private void nud_From_Leave(object sender, EventArgs e)
        {
            var c = 0;
            while((double)nud_From.Value != quantityFrom)
            {
                c++;
                nud_From.Value = (decimal)quantityFrom;

            }

        }
    }
}
