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

        WhiteBit wb = new WhiteBit();
        Huobi huobi = new Huobi();
        Ascendex ascendex = new Ascendex();
        Bittrex bittrex = new Bittrex();
        Binance binance = new Binance();
        Kucoin kucoin = new Kucoin();
        BscWallet bscWallet = new BscWallet();

        public Form1()
        {
            InitializeComponent();

            GlobalConfig oGlobalConfig = null;
            if (File.Exists("config.json"))
            {
                oGlobalConfig = JsonConvert.DeserializeObject<GlobalConfig>(File.ReadAllText("config.json"));
            }
            else
            {
                oGlobalConfig = new GlobalConfig();
                oGlobalConfig.aConfigs = new List<ExchangeConfig>()
                {
                    new ExchangeConfig()
                    {
                        Exchange = "Binance",
                        ApiKey = "ApiKey",
                        SecretKey = "SecretKey",
                        AdditionnalKey = ""
                    }
                };
                File.WriteAllText("config.json", JsonConvert.SerializeObject(oGlobalConfig));
            }

            List<IExchange> aEchanges = new List<IExchange>();
            var type = typeof(IExchange);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p))
                .ToList();

            foreach (var oConfig in oGlobalConfig.aConfigs)
            {
                var typeExchange = types.FirstOrDefault(t => t.Name == oConfig.Exchange);
                if (typeExchange == null)
                    continue; // oops

                var oExchange = Activator.CreateInstance(typeExchange) as IExchange;
                if (oExchange == null)
                    continue; // re-oops

                oExchange.Init(oConfig);
                aEchanges.Add(oExchange);
            }

            var aTasks = aEchanges.Select(oExchange => oExchange.GetBalance()).ToList();
            List<GlobalBalance> aAll = new List<GlobalBalance>();

            foreach (var oTask in aTasks)
            {
                var aBalance = oTask.GetAwaiter().GetResult();
                aAll.AddRange(aBalance);
            }


            var dTotalSum = aAll.Sum(a => a.BitCoinValue);
            dataGridView1.Rows.Add("TOTAL", "BTC", dTotalSum, dTotalSum);

            aAll.ForEach(b =>
            {
                dataGridView1.Rows.Add(b.Exchange, b.Crypto, b.Value, b.BitCoinValue);
            });
        }
    }

}
