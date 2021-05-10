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
    public class Bittrex : IExchange
    {
        private ExchangeConfig oConfig;
        private GlobalConfig oGlobalConfig;

        private const string hostname = "https://api.bittrex.com"; // put here your secret key
        private const string apiBalance = "/v3/balances"; // put here your secret key

        private static readonly HttpClient _httpClient = new HttpClient();

        public string GetExchangeName => "Bittrex";

        public Bittrex()
        {

        }

        public void Init(GlobalConfig oGlobalConfig, ExchangeConfig oConfig)
        {
            this.oConfig = oConfig;
            this.oGlobalConfig = oGlobalConfig;
        }

        internal class Payload
        {
            [JsonProperty("request")]
            public string Request { get; set; }

            [JsonProperty("nonce")]
            public string Nonce { get; set; }
        }
        internal class PayloadWithTicker : Payload
        {

            [JsonProperty("ticker")]
            public string Ticker { get; set; }
        }


        public List<CurrencySymbolPrice> GetCurrencies()
        {
            WebClient wc = new WebClient();
            var sData = wc.DownloadString($"{hostname}/v3/markets/tickers");

            var oCurrencies = JsonConvert.DeserializeObject<List<BittrexCurrencies>>(sData);
            return oCurrencies.Select(kvp =>
            {
                double Price = (kvp.AskRate.ToDouble() + kvp.BidRate.ToDouble()) / 2;
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

            string sApi = $"{hostname}{apiBalance}";

            var dataJsonStr = string.Empty;
            var hash = CalcHash(dataJsonStr);
            var signature = CalcSignature(nonce + sApi + "GET" + hash, oConfig.SecretKey);
                       

            WebClient wc = new WebClient();
            wc.Headers.Add("Api-Key", oConfig.ApiKey);
            wc.Headers.Add("Api-Timestamp", nonce);
            wc.Headers.Add("Api-Signature", signature);
            wc.Headers.Add("Api-Content-Hash", hash);
            //wc.Headers.Add("Api-Subaccount-Id", signature);
            wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");

            var responseBody = wc.DownloadString($"{hostname}{apiBalance}");



            var oBalance = JsonConvert.DeserializeObject<List<BittrexBalence>>(responseBody);
            List<GlobalBalance> oGlobalBalance = oBalance.Select(b =>
            {
                return new GlobalBalance
                {
                    Exchange = GetExchangeName,
                    Crypto = b.CurrencySymbol,
                    Value = b.Total.ToDouble(),
                };
            }
                ).Where(gb => gb.Value > 0).ToList();

            return oGlobalBalance;
        }

        public partial class BittrexCurrencies
        {
            [JsonProperty("symbol")]
            public string Symbol { get; set; }

            [JsonProperty("lastTradeRate")]
            public string LastTradeRate { get; set; }

            [JsonProperty("bidRate")]
            public string BidRate { get; set; }

            [JsonProperty("askRate")]
            public string AskRate { get; set; }
        }

        public partial class BittrexBalence
        {
            [JsonProperty("currencySymbol")]
            public string CurrencySymbol { get; set; }

            [JsonProperty("total")]
            public string Total { get; set; }

            [JsonProperty("available")]
            public string Available { get; set; }

            [JsonProperty("updatedAt")]
            public DateTimeOffset UpdatedAt { get; set; }
        }

        public string CalcSignature(string text, string apiSecret)
        {
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(apiSecret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            }
        }
        public string CalcHash(string text)
        {
            SHA512 shaM = new SHA512Managed();

            return BitConverter.ToString(shaM.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", string.Empty).ToLower();
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
}
