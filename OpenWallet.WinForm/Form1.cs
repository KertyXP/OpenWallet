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
        List<CurrencySymbolPrice> aAllCurrencies;
        List<GlobalBalance> aAllBalances;
        List<IExchange> aExchanges;

        public Form1()
        {
            var oUseless = new UseLess();
            InitializeComponent();

        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            Config.Init("");
            aExchanges = Config.LoadExchanges();

            aAllCurrencies = Config.GetCurrencries(aExchanges);

            aAllCurrencies.Where(c => c.Exchange == "Binance").ForEach(c =>
            {
                cb_From.Items.Add(c.From);
            });

            aAllBalances = await Config.GetBalances(aExchanges, aAllCurrencies);

            InsertCurrentBalanceInGrid(aAllBalances);


            List<GlobalTrade> aListTrades = await Config.LoadTrades(aExchanges, aAllBalances, aAllCurrencies);



            Config.ConvertTradesToDailyTrades(aListTrades).ForEach(t =>
            {
                dgv_trade_day.Rows.Add(t, t.Exchange, t.Couple, t.From, t.QuantityFrom, t.To, t.QuantityTo, t.dtTrade.ToString("yyyy-MM-dd"), t.QuantityBack);
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

            dgv_Balance.Columns[5].HeaderText = Config.oGlobalConfig.FavoriteCurrency;
            dgv_Balance.Rows.Insert(0, null, "TOTAL", Config.oGlobalConfig.FavoriteCurrency, dTotalSum, dTotalSumBtc, dTotalSum);
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (this.dgv_Balance.SelectedRows.Count <= 0)
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

        private void calculSwap()
        {
            nud_From.Enabled = rad_from.Checked;
            nud_To.Enabled = rad_to.Checked;
            var sFrom = cb_From.Text;
            var sTo = cb_to.Text;
            var pair = aAllCurrencies.FirstOrDefault(c => c.Exchange == "Binance" && c.From.ToUpper() == sFrom.ToUpper() && c.To.ToUpper() == sTo.ToUpper());
            lbl_qtty_from.Text = aAllBalances?.FirstOrDefault(c => c.Exchange == "Binance" && c.Crypto.ToUpper() == sFrom.ToUpper())?.Value.ToString();
            lbl_qtty_to.Text = aAllBalances?.FirstOrDefault(c => c.Exchange == "Binance" && c.Crypto.ToUpper() == sTo.ToUpper())?.Value.ToString();
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
    }
}
