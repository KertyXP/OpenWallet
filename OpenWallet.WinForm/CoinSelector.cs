using OpenWallet.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenWallet.WinForm
{
    public partial class CoinSelector : Form
    {
        public CoinSelector(List<CurrencySymbolPrice> allCurrencies)
        {
            InitializeComponent();

            this.cb_From.DrawMode = DrawMode.OwnerDrawFixed;
            this.cb_to.DrawMode = DrawMode.OwnerDrawFixed;
            _allCurrencies = allCurrencies;
        }

        private List<CurrencySymbolPrice> _allCurrencies { get; }
        private CurrencySymbolPrice symbolSelected;

        private void cb_From_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sFrom = cb_From.Text;
            this.cb_to.Enabled = _allCurrencies.Any(c => c.Exchange == "Binance" && c.From.ToUpper() == sFrom.ToUpper());
            this.cb_to.Items.Clear();
            _allCurrencies.Where(c => c.Exchange == "Binance" && c.From.ToUpper() == sFrom.ToUpper())
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

        private void calculSwap()
        {
            var sFrom = cb_From.Text;
            var sTo = cb_to.Text;
            var pair = _allCurrencies.FirstOrDefault(c => c.Exchange == "Binance" && c.From.ToUpper() == sFrom.ToUpper() && c.To.ToUpper() == sTo.ToUpper());
            symbolSelected = pair;
            lbl_couple.Text = pair?.Couple;
        }

        private void bt_ok_Click(object sender, EventArgs e)
        {
            Close();
        }

        public CurrencySymbolPrice GetCouple()
        {
            return symbolSelected;
        }

        private async void CoinSelector_Load(object sender, EventArgs e)
        {
            await RefreshCurrenciesAndBalance();
        }

        private async Task RefreshCurrenciesAndBalance()
        {
            cb_From.Items.Clear();

            _allCurrencies.Where(c => c.Exchange == "Binance").ForEach(c =>
            {
                if (cb_From.Items.Contains(c.From) == false)
                    cb_From.Items.Add(c.From);
            });
        }
    }
}
