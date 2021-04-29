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
    public partial class WhiteBit : IExchange
    {
        private ExchangeConfig oConfig;


        string request = "/api/v4/trade-account/balance"; // put here request path. For obtaining trading balance use: /api/v4/trade-account/balance
        string hostname = "https://whitebit.com"; // domain without last slash. Do not use whitebit.com/

        public WhiteBit()
        {

        }

        public void Init(ExchangeConfig oConfig)
        {
            this.oConfig = oConfig;
        }
        public async Task<List<CurrencySymbolPrice>> GetCurrencies()
        {
            WebClient wc = new WebClient();
            var sData = wc.DownloadString($"{hostname}/api/v4/public/ticker");

            var oWhiteBitCurrencies = JsonConvert.DeserializeObject<Dictionary<string, WhiteBitCurrencies>>(sData);
            return oWhiteBitCurrencies.Select(kvp =>
            {
                double Price = kvp.Value.LastPrice.ToDouble();
                return new List<CurrencySymbolPrice>()
                {
                    new CurrencySymbolPrice(kvp.Key.Split('_').FirstOrDefault(), kvp.Key.Split('_').Last(), Price),
                    new CurrencySymbolPriceReverted(kvp.Key.Split('_').FirstOrDefault(), kvp.Key.Split('_').Last(), Price),
                };
            })
            .SelectMany(o => o)
            .ToList();
        }

        public async Task<List<GlobalBalance>> GetBalance()
        {
            var oCurrencies = await GetCurrencies();
            // If the nonce is similar to or lower than the previous request number, you will receive the 'too many requests' error message
            // nonce is a number that is always higher than the previous request number
            var nonce = GetNonce();
            var data = new Payload
            {
                Nonce = nonce,
                Request = request
            };

            var dataJsonStr = JsonConvert.SerializeObject(data);
            var payload = dataJsonStr.Base64Encode();
            var signature = CalcSignature(payload, oConfig.SecretKey);

            var content = new StringContent(dataJsonStr, Encoding.UTF8, "application/json");
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{hostname}{request}")
            {
                Content = content
            };

            WebClient wc = new WebClient();
            wc.Headers.Add("X-TXC-APIKEY", oConfig.ApiKey);
            wc.Headers.Add("X-TXC-PAYLOAD", payload);
            wc.Headers.Add("X-TXC-SIGNATURE", signature);
            wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");

            var responseBody = wc.UploadString($"{hostname}{request}", dataJsonStr);

            Dictionary<string, Balance> oBalance = BalanceRoot.FromJson(responseBody);
            List<GlobalBalance> oGlobalBalance = oBalance.Select(keyValue =>
            {
                double val = keyValue.Value.Available.ToDouble() + keyValue.Value.Freeze.ToDouble();
                if (val <= 0)
                    return null;
                return new GlobalBalance
                {
                    Exchange = "WhiteBit",
                    Crypto = keyValue.Key,
                    Value = val,
                    BitCoinValue = oCurrencies.GetBtcValue(keyValue.Key, val)
                };
            }
                ).Where(gb => gb != null).ToList(); ;

            return oGlobalBalance;
        }


        public partial class WhiteBitCurrencies
        {
            [JsonProperty("base_id")]
            public long BaseId { get; set; }

            [JsonProperty("quote_id")]
            public long QuoteId { get; set; }

            [JsonProperty("last_price")]
            public string LastPrice { get; set; }

            [JsonProperty("quote_volume")]
            public string QuoteVolume { get; set; }

            [JsonProperty("base_volume")]
            public string BaseVolume { get; set; }

            [JsonProperty("isFrozen")]
            public bool IsFrozen { get; set; }

            [JsonProperty("change")]
            public string Change { get; set; }
        }

        public partial class BalanceRoot
        {
            public static Dictionary<string, Balance> FromJson(string json) => JsonConvert.DeserializeObject<Dictionary<string, Balance>>(json, WhiteBit.Converter.Settings);
        }
        public partial class Balance
        {
            [JsonProperty("available")]
            public string Available { get; set; }

            [JsonProperty("freeze")]
            public string Freeze { get; set; }
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


        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            };
        }

        public string CalcSignature(string text, string apiSecret)
        {
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(apiSecret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            }
        }
        private string GetNonce()
        {
            var milliseconds = (long)DateTime.Now.ToUniversalTime()
                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .TotalMilliseconds / 1000;

            return milliseconds.ToString();
        }
    }
}
