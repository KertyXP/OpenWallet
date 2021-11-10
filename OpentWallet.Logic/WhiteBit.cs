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
using Newtonsoft.Json.Linq;

namespace OpentWallet.Logic
{
    public partial class WhiteBit : IExchange
    {
        ExchangeConfig IExchange.oConfig { get; set; }
        private ExchangeConfig oConfig;
        private GlobalConfig oGlobalConfig;


        string request = "/api/v4/trade-account/balance"; // put here request path. For obtaining trading balance use: /api/v4/trade-account/balance
        string hostname = "https://whitebit.com"; // domain without last slash. Do not use whitebit.com/

        public string ExchangeCode => "WhiteBit";
        public string ExchangeName => "WhiteBit";

        public WhiteBit()
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
            var sData = wc.DownloadString($"{hostname}/api/v4/public/ticker");

            var oWhiteBitCurrencies = JsonConvert.DeserializeObject<Dictionary<string, WhiteBitCurrencies>>(sData);
            return oWhiteBitCurrencies.Select(kvp =>
            {
                double Price = kvp.Value.LastPrice.ToDouble();
                return new List<CurrencySymbolPrice>()
                {
                    new CurrencySymbolPrice(kvp.Key.Split('_').FirstOrDefault(), kvp.Key.Split('_').Last(), Price, kvp.Key, ExchangeName),
                    new CurrencySymbolPriceReverted(kvp.Key.Split('_').FirstOrDefault(), kvp.Key.Split('_').Last(), Price, kvp.Key, ExchangeName),
                };
            })
            .SelectMany(o => o)
            .ToList();
        }

        private T SendRequest<T>(string sApi, Payload oPost) where T : new()
        {

            //var responseBodyHistory = wc.UploadString($"{hostname}{"api/v4/main-account/history}",dataJsonStr);

            // If the nonce is similar to or lower than the previous request number, you will receive the 'too many requests' error message
            // nonce is a number that is always higher than the previous request number
            var nonce = GetNonce();
            oPost.Nonce = nonce;
            oPost.Request = sApi;

            var dataJsonStr = JsonConvert.SerializeObject(oPost);
            var payload = dataJsonStr.Base64Encode();
            var signature = CalcSignature(payload, oConfig.SecretKey);

            var content = new StringContent(dataJsonStr, Encoding.UTF8, "application/json");


            WebClient wc = new WebClient();
            wc.Headers.Add("X-TXC-APIKEY", oConfig.ApiKey);
            wc.Headers.Add("X-TXC-PAYLOAD", payload);
            wc.Headers.Add("X-TXC-SIGNATURE", signature);
            wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");

            try
            {

                var responseBody = wc.UploadString($"{hostname}{sApi}", dataJsonStr);

                try
                {
                    T oBalance = JsonConvert.DeserializeObject<T>(responseBody);
                    return oBalance;
                }
                catch (Exception ex)
                {
                    return new T();
                }
            }
            catch (Exception ex)
            {
                return new T();
            }

        }

        public List<GlobalTrade> GetTradeHistory(List<GlobalTrade> aCache, List<GlobalBalance> aAllBalances)
        {

            List<GlobalTrade> aListTrades = new List<GlobalTrade>(aCache ?? new List<GlobalTrade>());

            bool bOrderFoundInCache = false;

            int nOffset = 0;
            while (true)
            {
                if (bOrderFoundInCache == true)
                {
                    //return aListTrades;
                }

                var oHistoryResponse = SendRequest<Dictionary<string, List<OrderHistory>>>("/api/v4/trade-account/executed-history", new PayloadOrderHistory() { Offset = nOffset, Limit = 100 });
                if (oHistoryResponse.Any() == false)
                    break;
                foreach (var kvpResponse in oHistoryResponse)
                {
                    var cur = new CurrencySymbolExchange(kvpResponse.Key.Split('_').FirstOrDefault(), kvpResponse.Key.Split('_').Last(), kvpResponse.Key, ExchangeName);
                    if (cur == null)
                        continue; // oops

                    foreach (var oOrderHistory in kvpResponse.Value)
                    {

                        if (aListTrades.Any(lt => lt.InternalExchangeId == oOrderHistory.Id.ToString()))
                        {
                            bOrderFoundInCache = true;
                            continue;
                        }

                        var oGlobalTrade = new GlobalTrade();
                        oGlobalTrade.Exchange = ExchangeName;
                        if (oOrderHistory.Side == Side.Buy)
                        {
                            oGlobalTrade.From = cur.To;
                            oGlobalTrade.To = cur.From;
                            oGlobalTrade.Price = 1 / oOrderHistory.Price.ToDouble();
                            oGlobalTrade.QuantityTo = oOrderHistory.Amount.ToDouble();
                            oGlobalTrade.QuantityFrom = oGlobalTrade.QuantityTo / oGlobalTrade.Price;
                        }
                        else
                        {

                            oGlobalTrade.From = cur.From;
                            oGlobalTrade.To = cur.To;
                            oGlobalTrade.Price = oOrderHistory.Price.ToDouble();
                            oGlobalTrade.QuantityFrom = oOrderHistory.Amount.ToDouble();
                            oGlobalTrade.QuantityTo = oGlobalTrade.QuantityFrom * oGlobalTrade.Price;
                        }
                        oGlobalTrade.InternalExchangeId = oOrderHistory.Id.ToString();
                        oGlobalTrade.dtTrade = UnixTimeStampToDateTime(oOrderHistory.Time);
                        aListTrades.Add(oGlobalTrade);

                    }
                }
                nOffset += 100;
            }



            return aListTrades;
        }
        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public List<GlobalBalance> GetBalance()
        {

            var oBalance = SendRequest<Dictionary<string, Balance>>(request, new Payload() { });

            // this one does not work???
            //var oHistory = SendRequest<JObject>("/api/v4/main-account/history", new PayloadWithdrawDepositHistory() { Offset = 0, Limit = 100, TransactionMethod = "1" });


            List<GlobalBalance> oGlobalBalance = oBalance.Select(keyValue =>
        {
            double val = keyValue.Value.Available.ToDouble() + keyValue.Value.Freeze.ToDouble();
            if (val <= 0)
                return null;
            return new GlobalBalance
            {
                Exchange = ExchangeName,
                Crypto = keyValue.Key,
                Value = val,
            };
        }
            ).Where(gb => gb != null).ToList(); ;

            return oGlobalBalance;
        }

        public partial class OrderHistory
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("clientOrderId")]
            public string ClientOrderId { get; set; }

            [JsonProperty("time")]
            public double Time { get; set; }

            [JsonProperty("side")]
            public Side Side { get; set; }

            [JsonProperty("role")]
            public long Role { get; set; }

            [JsonProperty("amount")]
            public string Amount { get; set; }

            [JsonProperty("price")]
            public string Price { get; set; }

            [JsonProperty("deal")]
            public string Deal { get; set; }

            [JsonProperty("fee")]
            public string Fee { get; set; }
        }

        public enum Side { Buy, Sell };

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
        internal class PayloadWithdrawDepositHistory : Payload
        {

            [JsonProperty("transactionMethod ")]
            public string TransactionMethod { get; set; }

            [JsonProperty("limit")]
            public int Limit { get; set; }

            [JsonProperty("offset")]
            public int Offset { get; set; }
        }
        internal class PayloadOrderHistory : Payload
        {

            //[JsonProperty("market ")]
            //public string Market { get; set; }

            [JsonProperty("limit")]
            public int Limit { get; set; }

            [JsonProperty("offset")]
            public int Offset { get; set; }
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
                .TotalMilliseconds;

            return milliseconds.ToString();
        }

        public string PlaceMarketOrder(CurrencySymbol symbol, double quantity, bool buy)
        {
            throw new NotImplementedException();
        }
    }
}
