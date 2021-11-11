using OpentWallet.Logic;
using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OpenWallet.TradeForm
{
    public partial class Form1 : Form
    {
        List<CurrencySymbolPrice> aAllCurrencies;
        List<GlobalBalance> aAllBalances;
        List<IExchange> aExchanges;

        public Form1()
        {
            var oUseless = new UseLess();
            InitializeComponent();
            this.cb_From.DrawMode = DrawMode.OwnerDrawFixed;
            this.cb_to.DrawMode = DrawMode.OwnerDrawFixed;

        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            Config.Init("");
            aExchanges = Config.LoadExchanges();

            aAllCurrencies = Config.GetCurrencries(aExchanges);
            aAllBalances = await Config.GetBalances(aExchanges, aAllCurrencies);

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
            return aAllBalances?.FirstOrDefault(c => c.Exchange == "Binance" && c.Crypto.ToUpper() == sCrypto.ToUpper())?.Value ?? 0;
        }

        private void calculSwap()
        {
            nud_From.Enabled = rad_from.Checked;
            nud_To.Enabled = rad_to.Checked;
            var sFrom = cb_From.Text;
            var sTo = cb_to.Text;
            var pair = aAllCurrencies.FirstOrDefault(c => c.Exchange == "Binance" && c.From.ToUpper() == sFrom.ToUpper() && c.To.ToUpper() == sTo.ToUpper());
            lbl_qtty_from.Text = GetBalance(cb_From.Text).ToString();
            lbl_qtty_to.Text = GetBalance(cb_to.Text).ToString();
            lbl_couple.Text = pair.Couple;

            if (pair == null)
            {
                lbl_currentPrice.Text = "???";
            }
            else
            {
                lbl_currentPrice.Text = pair.Price.ToString();
            }

            nud_From.ValueChanged -= nud_To_ValueChanged;
            nud_To.ValueChanged -= nud_To_ValueChanged;

            if (rad_from.Checked)
            {
                if (pair == null)
                {
                    nud_To.Value = 0;
                }
                else
                {
                    nud_To.Value = nud_From.Value * (decimal)pair.Price;
                }
            }
            else if (rad_to.Checked)
            {
                if (pair == null)
                {
                    nud_From.Value = 0;
                }
                else
                {
                    nud_From.Value = nud_To.Value / (decimal)pair.Price;
                }
            }

            nud_To.ValueChanged += nud_To_ValueChanged;
            nud_From.ValueChanged += nud_To_ValueChanged;
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

        private void nud_From_ValueChanged(object sender, EventArgs e)
        {
            calculSwap();
        }

        private void nud_To_ValueChanged(object sender, EventArgs e)
        {
            calculSwap();
        }

        private void rad_from_CheckedChanged(object sender, EventArgs e)
        {
            if (rad_from.Checked)
                rad_to.Checked = false;
            calculSwap();
        }

        private void rad_to_CheckedChanged(object sender, EventArgs e)
        {

            if (rad_to.Checked)
                rad_from.Checked = false;
            calculSwap();
        }

        private void cb_to_TextUpdate(object sender, EventArgs e)
        {
            calculSwap();
        }

        private void bt_swap_Click(object sender, EventArgs e)
        {
            var oBinanceApi = aExchanges.FirstOrDefault(exc => exc.ExchangeCode == "Binance");
            if (oBinanceApi == null)
                return;

            //var sFrom = cb_From.Text;
            //var sTo = cb_to.Text;
            var pair = aAllCurrencies.FirstOrDefault(c => c.Exchange == "Binance" && c.From.ToUpper() == cb_From.Text.ToUpper() && c.To.ToUpper() == cb_to.Text.ToUpper());

            bool CoupleOrderRespected = pair.Couple.StartsWith(cb_From.Text.ToUpper());
            double quantityToSwap = CoupleOrderRespected ? (double)nud_From.Value : (double)nud_To.Value;
            double quantitySwapApprox = CoupleOrderRespected == false ? (double)nud_From.Value : (double)nud_To.Value;
            var sellBuy = CoupleOrderRespected == rad_from.Checked ? SellBuy.Sell : SellBuy.Buy;

            bool? bResult = oBinanceApi?.PlaceMarketOrder(pair, quantityToSwap, sellBuy, cb_Test.Checked);
            string sSuccess = bResult == true ? "Success!" : "FAILURE!!";

            if (CoupleOrderRespected)
                MessageBox.Show($"{sSuccess} - {sellBuy} {quantityToSwap} {pair.From} with {quantitySwapApprox} {pair.To}");
            else
                MessageBox.Show($"{sSuccess} - {sellBuy} {quantityToSwap} {pair.To} with {quantitySwapApprox} {pair.From}");

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
            if(e.KeyCode == Keys.Back)
            {
                var cb = sender as ComboBox;
                cb.TextChanged -= cb_TextChanged;
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
    }
}
