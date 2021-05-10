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
    public class Kucoin : IExchange
    {
        private ExchangeConfig oConfig;
        private GlobalConfig oGlobalConfig;

        private const string apiVersion = "2"; // put here your public key

        private const string hostname = "https://api.kucoin.com"; // put here your secret key
        private const string apiaccounts = "/api/v1/accounts"; // put here your secret key

        private static readonly HttpClient _httpClient = new HttpClient();

        public string GetExchangeName => "Kucoin";

        public Kucoin()
        {

        }

        public void Init(GlobalConfig oGlobalConfig, ExchangeConfig oConfig)
        {
            this.oConfig = oConfig;
            this.oGlobalConfig = oGlobalConfig;
        }

        public List<CurrencySymbolPrice> GetCurrencies()
        {
            WebClient wc = new WebClient();
            var sData = wc.DownloadString($"{hostname}/api/v1/market/allTickers");

            var oCurrencies = JsonConvert.DeserializeObject<KucoinTicker>(sData);
            return oCurrencies.Data.Ticker.Select(kvp =>
            {
                double Price = (kvp.Buy.ToDouble() + kvp.Sell.ToDouble()) / 2;
                return new List<CurrencySymbolPrice>()
                {
                    new CurrencySymbolPrice(kvp.Symbol.Split('-').FirstOrDefault(), kvp.Symbol.Split('-').Last(), Price, GetExchangeName),
                    new CurrencySymbolPriceReverted(kvp.Symbol.Split('-').FirstOrDefault(), kvp.Symbol.Split('-').Last(), Price, GetExchangeName),
                };
            })
            .SelectMany(o => o)
            .ToList();
        }

        public List<GlobalBalance> GetBalance()
        {

            // If the nonce is similar to or lower than the previous request number, you will receive the 'too many requests' error message
            // nonce is a number that is always higher than the previous request number
            var nonce = GetNonce();


            var dataJsonStr = string.Empty;

            var signature = CalcSignature($"{nonce}GET{apiaccounts}", oConfig.SecretKey);
            var signaturePassPhrase = CalcSignature(oConfig.AdditionnalKey, oConfig.SecretKey);

            string sApi = $"{hostname}{apiaccounts}";


            WebClient wc = new WebClient();
            wc.Headers.Add("KC-API-KEY", oConfig.ApiKey);
            wc.Headers.Add("KC-API-SIGN", signature);
            wc.Headers.Add("KC-API-TIMESTAMP", nonce);
            wc.Headers.Add("KC-API-PASSPHRASE", signaturePassPhrase);
            wc.Headers.Add("KC-API-KEY-VERSION", apiVersion);
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

                var oBalance = JsonConvert.DeserializeObject<KucoinAccount>(responseBody);

                List<GlobalBalance> oGlobalBalance = oBalance.Data.Select(b =>
                {
                    return new GlobalBalance
                    {
                        Exchange = GetExchangeName,
                        Crypto = b.Currency,
                        Value = b.Available.ToDouble(),
                    };
                }
                    ).Where(gb => gb.Value > 0).ToList();

                return oGlobalBalance;

            }
            catch (Exception ex)
            {
                return new List<GlobalBalance>();
            }
        }

        private string CalcSignature(string text, string apiSecret)
        {
            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret)))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                return Convert.ToBase64String(hashmessage);
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

    public partial class KucoinTicker
    {
        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("data")]
        public TickerData Data { get; set; }
    }
    public partial class TickerData
    {

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("ticker")]
        public List<Ticker> Ticker { get; set; }
    }
    public partial class Ticker
    {
        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("symbolName")]
        public string SymbolName { get; set; }

        [JsonProperty("buy")]
        public string Buy { get; set; }

        [JsonProperty("sell")]
        public string Sell { get; set; }

        [JsonProperty("changeRate")]
        public string ChangeRate { get; set; }

        [JsonProperty("changePrice")]
        public string ChangePrice { get; set; }

        [JsonProperty("high")]
        public string High { get; set; }

        [JsonProperty("low")]
        public string Low { get; set; }

        [JsonProperty("vol")]
        public string Vol { get; set; }

        [JsonProperty("volValue")]
        public string VolValue { get; set; }

        [JsonProperty("last")]
        public string Last { get; set; }

        [JsonProperty("averagePrice")]
        public string AveragePrice { get; set; }

        [JsonProperty("takerFeeRate")]
        public string TakerFeeRate { get; set; }

        [JsonProperty("makerFeeRate")]
        public string MakerFeeRate { get; set; }

        [JsonProperty("takerCoefficient")]
        public string TakerCoefficient { get; set; }

        [JsonProperty("makerCoefficient")]
        public string MakerCoefficient { get; set; }
    }

    public partial class KucoinAccount
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("data")]
        public KucoinData[] Data { get; set; }
    }

    public partial class KucoinData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("balance")]
        public string Balance { get; set; }

        [JsonProperty("available")]
        public string Available { get; set; }

        [JsonProperty("holds")]
        public string Holds { get; set; }
    }


}
