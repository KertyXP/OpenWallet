using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OpenWallet.Common;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using OpenWallet.Logic.Abstraction;

namespace OpentWallet.Logic
{
    public class BscWallet : IExchange
    {
        private ExchangeConfig oConfig;

        private const string host = "https://bscscan.com"; // put here your secret key
        private const string getAddress = "address"; // put here your secret key


        public BscWallet()
        {

        }

        public void Init(ExchangeConfig oConfig)
        {
            this.oConfig = oConfig;
        }


        public async Task<List<GlobalBalance>> GetBalance()
        {

            WebClient wc = new WebClient();
            var sHTML = wc.DownloadString($"{host}/{getAddress}/{oConfig.ApiKey}");

            var oDoc = new HtmlAgilityPack.HtmlDocument();
            oDoc.LoadHtml(sHTML);

            var aBalance = new List<GlobalBalance>();

            var oSummary = oDoc
                .DocumentNode
                .Descendants("div")
                .Where(x => x.Attributes.Contains("id") && x.Attributes["id"].Value == "ContentPlaceHolder1_divSummary")
                .FirstOrDefault()
                .InnerText
                .Replace('\n', ' ');

            var sBnb = Regex.Replace(oSummary, ".*Balance: (.*) BNB.* BNB Value.*", "$1");
            aBalance.Add(new GlobalBalance()
            {
                Exchange = "BSC",
                Crypto = "BNB",
                Value = sBnb.ToDouble()
            });
            var aTokens = oDoc.DocumentNode.Descendants("li").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "list-custom list-custom-BEP-20");
            foreach(var oToken in aTokens)
            {
                var oAmount = oToken.Descendants("span").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.StartsWith("list-amount")).FirstOrDefault().InnerText;
                aBalance.Add(new GlobalBalance()
                {
                    Exchange = "BSC",
                    Crypto = oAmount.Split(' ').LastOrDefault(),
                    Value = oAmount.Split(' ').FirstOrDefault().Replace(",", "").ToDouble()
                });
            }

            //List<GlobalBalance> oGlobalBalance = oBalance.Balances.Select(b =>
            //{
            //    return new GlobalBalance
            //    {
            //        Exchange = "Binance",
            //        Crypto = b.Asset,
            //        Value = b.Free.ToDouble() + b.Locked.ToDouble()
            //    };
            //}
            //    ).Where(gb => gb.Value > 0).ToList();

            //return oGlobalBalance;

            return aBalance;
        }
    }

}
