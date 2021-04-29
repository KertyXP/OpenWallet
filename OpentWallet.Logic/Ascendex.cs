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
using System.Web;
using OpenWallet.Logic.Abstraction;

namespace OpentWallet.Logic
{
    public class Ascendex : IExchange
    {
        private ExchangeConfig oConfig;
        private const string hostname = "https://ascendex.com";

        private WebClient client;
        public Ascendex()
        {
            client = new WebClient();

        }

        public void Init(ExchangeConfig oConfig)
        {
            this.oConfig = oConfig;
        }

        public string CalcSignature(string text, string apiSecret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
                var b64 = Convert.ToBase64String(hash, 0, hash.Length);
                return b64;
            }
        }

        private string GetNonce()
        {
            var milliseconds = (long)DateTime.Now.ToUniversalTime()
                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .TotalMilliseconds;

            return milliseconds.ToString();
        }

        public async Task<List<CurrencySymbolPrice>> GetCurrencies()
        {
            WebClient wc = new WebClient();
            var sData = wc.DownloadString($"{hostname}/api/pro/v1/ticker");

            var oCurrencies = JsonConvert.DeserializeObject<AscendexCurrencies>(sData);
            return oCurrencies.Data.Select(kvp =>
            {
                double Price = (kvp.Ask.FirstOrDefault().ToDouble() + kvp.Bid.FirstOrDefault().ToDouble()) / 2;
                return new List<CurrencySymbolPrice>()
                {
                    new CurrencySymbolPrice(kvp.Symbol.Split('/').FirstOrDefault(), kvp.Symbol.Split('/').LastOrDefault(), Price),
                    new CurrencySymbolPriceReverted(kvp.Symbol.Split('/').FirstOrDefault(), kvp.Symbol.Split('/').LastOrDefault(), Price),
                };
            })
            .SelectMany(o => o)
            .ToList();
        }

        public async Task<List<GlobalBalance>> GetBalance()
        {
            var oCurrencies = await GetCurrencies();

            var timestampUtcMillisecond = GetNonce();

            var signature = CalcSignature(timestampUtcMillisecond + "+info", oConfig.SecretKey);

            string sApi = "/api/pro/v1/info";

            client.Headers.Add("x-auth-key", oConfig.ApiKey);
            client.Headers.Add("x-auth-timestamp", timestampUtcMillisecond);
            client.Headers.Add("x-auth-signature", signature);


            var request = client.DownloadString($"{hostname}{sApi}");

            var account = JsonConvert.DeserializeObject<AscendexAccount>(request);

            sApi = $"/{account.Data.AccountGroup}/api/pro/v1/cash/balance";

            timestampUtcMillisecond = GetNonce();

            signature = CalcSignature(timestampUtcMillisecond + "+balance", oConfig.SecretKey);

            client.Headers.Clear();
            client.Headers.Add("x-auth-key", oConfig.ApiKey);
            client.Headers.Add("x-auth-timestamp", timestampUtcMillisecond);
            client.Headers.Add("x-auth-signature", signature);


            var request2 = client.DownloadString($"{hostname}{sApi}");

            var balance = JsonConvert.DeserializeObject<AscendexBalence>(request2);


            return balance.Data.Select(b => new GlobalBalance()
            {
                Exchange = "Ascendex",
                Crypto = b.Asset,
                Value = b.TotalBalance.ToDouble(),
                BitCoinValue = oCurrencies.GetBtcValue(b.Asset, b.TotalBalance.ToDouble())
            }).Where(b => b.Value > 0).ToList();

            //var result = SendRequest<List<Logic.Account>>(GET_ACCOUNT);

            //var oAccount = JsonConvert.DeserializeObject<Account>(result);

            //var result2 = SendRequest<List<Logic.Account>>(GET_ACCOUNT + "/" + oAccount.Data.FirstOrDefault().Id + "/balance");
            //var oBalance = JsonConvert.DeserializeObject<Balance>(result2);

            //var aGlobalBalance = oBalance.Data.List
            //    .GroupBy(l => l.Currency)
            //    .Select(g =>
            //{
            //    return new GlobalBalance()
            //    {
            //        Exchange = "Huobi",
            //        Crypto = g.Key,
            //        Value = g.Sum(f => f.Balance.ToDouble())
            //    };
            //})
            //    .Where(gb => gb.Value > 0)
            //    .ToList();

            //return aGlobalBalance;

        }

        private double GetBtcValue(string Crypto, double Value, List<CurrencySymbolPrice> oCurrencies)
        {
            if (Crypto == "BTC")
                return Value;


            var oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == Crypto && o.To == "BTC");

            if (oCryptoFound != null)
            {
                return Value * oCryptoFound.Price;
            }

            var oCryptoFoundUsdtBtc = oCurrencies.FirstOrDefault(o => o.From == "USDT" && o.To == "BTC");
            oCryptoFound = oCurrencies.FirstOrDefault(o => o.From == Crypto && o.To == "USDT");

            if (oCryptoFound != null)
            {
                return Value * oCryptoFound.Price * oCryptoFoundUsdtBtc.Price;
            }

            return 0;
        }

    }
    public partial class AscendexCurrencies
    {
        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("data")]
        public List<AscendexCurrenciesData> Data { get; set; }
    }

    public partial class AscendexCurrenciesData
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("open")]
        public string Open { get; set; }

        [JsonProperty("close")]
        public string Close { get; set; }

        [JsonProperty("high")]
        public string High { get; set; }

        [JsonProperty("low")]
        public string Low { get; set; }

        [JsonProperty("volume")]
        public string Volume { get; set; }

        [JsonProperty("ask")]
        public string[] Ask { get; set; }

        [JsonProperty("bid")]
        public string[] Bid { get; set; }
    }

    public partial class AscendexBalence
    {
        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("data")]
        public AscendexData[] Data { get; set; }
    }

    public partial class AscendexData
    {
        [JsonProperty("asset")]
        public string Asset { get; set; }

        [JsonProperty("totalBalance")]
        public string TotalBalance { get; set; }

        [JsonProperty("availableBalance")]
        public string AvailableBalance { get; set; }
    }

    public partial class AscendexAccount
    {
        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("data")]
        public AscendexData Data { get; set; }
    }

    public partial class AscendexData
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("accountGroup")]
        public long AccountGroup { get; set; }

        [JsonProperty("viewPermission")]
        public bool ViewPermission { get; set; }

        [JsonProperty("tradePermission")]
        public bool TradePermission { get; set; }

        [JsonProperty("transferPermission")]
        public bool TransferPermission { get; set; }

        [JsonProperty("cashAccount")]
        public string[] CashAccount { get; set; }

        [JsonProperty("marginAccount")]
        public string[] MarginAccount { get; set; }

        [JsonProperty("futuresAccount")]
        public string[] FuturesAccount { get; set; }

        [JsonProperty("userUID")]
        public string UserUid { get; set; }

        [JsonProperty("expireTime")]
        public long ExpireTime { get; set; }

        [JsonProperty("allowedIps")]
        public object[] AllowedIps { get; set; }

        [JsonProperty("limitQuota")]
        public long LimitQuota { get; set; }
    }
}
