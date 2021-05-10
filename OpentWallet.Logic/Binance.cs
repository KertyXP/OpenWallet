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
using OpenWallet.Logic.Abstraction;

namespace OpentWallet.Logic
{
    public class Binance : IExchange
    {
        private ExchangeConfig oConfig;
        private GlobalConfig oGlobalConfig;

        private const string hostname = "https://api.binance.com"; // put here your secret key
        private const string apiBalance = "/api/v3/account"; // put here your secret key

        private static readonly HttpClient _httpClient = new HttpClient();

        public string GetExchangeName => "Binance";

        public Binance()
        {

        }

        public void Init(GlobalConfig oGlobalConfig, ExchangeConfig oConfig)
        {
            this.oConfig = oConfig;
            this.oGlobalConfig = oGlobalConfig;
        }

        public List<CurrencySymbolPrice> GetCurrencies()
        {

            var wc = new WebClient();

            var sData = wc.DownloadString($"{hostname}/api/v3/ticker/price");
            var oCurrencies = JsonConvert.DeserializeObject<List<BinanceCurrencies>>(sData);

            wc.Headers.Add("X-MBX-APIKEY", oConfig.ApiKey);
            //wc.Headers.Add("Api-Subaccount-Id", signature);
            wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            var sPair = wc.DownloadString("https://api.binance.com/sapi/v1/margin/allPairs");
            var aPairs = JsonConvert.DeserializeObject<List<BinancePair>>(sPair).Select(p => new CurrencySymbol(p.Base, p.Quote)).ToList();


            return oCurrencies.Select(o =>
            {
                double Price = o.Price.ToDouble();

                var cur = aPairs.FirstOrDefault(c => c.From + c.To == o.Symbol);

                if (cur == null)
                    cur = CurrencySymbol.AutoDiscoverCurrencySymbol(o.Symbol);


                if (cur == null)
                {
                    return new List<CurrencySymbolPrice>();
                }
                return new List<CurrencySymbolPrice>()
                {
                    new CurrencySymbolPrice(cur.From, cur.To, Price, GetExchangeName),
                    new CurrencySymbolPriceReverted(cur.From, cur.To, Price, GetExchangeName),
                };
            })
            .SelectMany(o => o)
            .Where(o => o != null)
            .ToList();

        }


        public List<GlobalBalance> GetBalance()
        {
            // If the nonce is similar to or lower than the previous request number, you will receive the 'too many requests' error message
            // nonce is a number that is always higher than the previous request number
            var nonce = GetNonce();


            var dataJsonStr = string.Empty;

            string sQueryParam = $"timestamp={nonce}";

            var signature = CalcSignature($"{sQueryParam}{dataJsonStr}", oConfig.SecretKey);

            string sApi = $"{hostname}{apiBalance}?{sQueryParam}&signature={signature}";


            WebClient wc = new WebClient();
            wc.Headers.Add("X-MBX-APIKEY", oConfig.ApiKey);
            //wc.Headers.Add("Api-Subaccount-Id", signature);
            wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            string responseBody = string.Empty;
            try
            {

                responseBody = wc.DownloadString($"{sApi}");


            }
            catch (Exception ex)
            {
                return new List<GlobalBalance>();
            }
            try
            {

                var oBalance = JsonConvert.DeserializeObject<BinanceAccount>(responseBody);

                List<GlobalBalance> oGlobalBalance = oBalance.Balances.Select(b =>
                {
                    double val = b.Free.ToDouble() + b.Locked.ToDouble();
                    if (val == 0)
                        return null;
                    return new GlobalBalance
                    {
                        Exchange = GetExchangeName,
                        Crypto = b.Asset,
                        Value = val,
                    };
                }
                    )
                    .Where(gb => gb != null)
                    .Where(gb => gb.Value > 0)
                    .ToList();

                return oGlobalBalance;

            }
            catch (Exception ex)
            {
                return new List<GlobalBalance>();
            }
        }

        private object CalcSignature(string text, string apiSecret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            }
        }

        private string GetNonce()
        {
            var milliseconds = (long)DateTime.Now.ToUniversalTime()
                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .TotalMilliseconds;

            return milliseconds.ToString();
        }

        public List<GlobalTrade> GetTradeHistory(List<GlobalTrade> aCache)
        {
            return new List<GlobalTrade>();
        }
    }
    public partial class BinancePair
    {
        [JsonProperty("id")]
        public double Id { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("base")]
        public string Base { get; set; }

        [JsonProperty("quote")]
        public string Quote { get; set; }

        [JsonProperty("isMarginTrade")]
        public bool IsMarginTrade { get; set; }

        [JsonProperty("isBuyAllowed")]
        public bool IsBuyAllowed { get; set; }

        [JsonProperty("isSellAllowed")]
        public bool IsSellAllowed { get; set; }
    }
    public class BinanceCurrencies
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }
    }

    public partial class BinanceAccount
    {
        [JsonProperty("makerCommission")]
        public long MakerCommission { get; set; }

        [JsonProperty("takerCommission")]
        public long TakerCommission { get; set; }

        [JsonProperty("buyerCommission")]
        public long BuyerCommission { get; set; }

        [JsonProperty("sellerCommission")]
        public long SellerCommission { get; set; }

        [JsonProperty("canTrade")]
        public bool CanTrade { get; set; }

        [JsonProperty("canWithdraw")]
        public bool CanWithdraw { get; set; }

        [JsonProperty("canDeposit")]
        public bool CanDeposit { get; set; }

        [JsonProperty("updateTime")]
        public long UpdateTime { get; set; }

        [JsonProperty("accountType")]
        public string AccountType { get; set; }

        [JsonProperty("balances")]
        public Balance[] Balances { get; set; }

        [JsonProperty("permissions")]
        public string[] Permissions { get; set; }
    }

    public partial class Balance
    {
        [JsonProperty("asset")]
        public string Asset { get; set; }

        [JsonProperty("free")]
        public string Free { get; set; }

        [JsonProperty("locked")]
        public string Locked { get; set; }
    }
}
