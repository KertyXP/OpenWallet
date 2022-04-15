using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using OpenWallet.Common;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using System.Globalization;
using System.Linq;
using OpenWallet.Logic.Abstraction;
using System.Threading.Tasks;

namespace OpentWallet.Logic
{
    public partial class WhiteBit : IExchange, IGetTradesData
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

        public async Task InitAsync(GlobalConfig oGlobalConfig, ExchangeConfig oConfig)
        {
            this.oConfig = oConfig;
            this.oGlobalConfig = oGlobalConfig;
        }
        public async Task<List<CurrencySymbolPrice>> GetCurrenciesAsync()
        {
            WebClient web = new WebClient();
            var sData = web.DownloadString($"{hostname}/api/v2/public/ticker");

            var oWhiteBitCurrencies = JsonConvert.DeserializeObject<BinanceTradeMarketResponse>(sData);
            return oWhiteBitCurrencies.Result.Select(result =>
            {
                double Price = result.LastPrice.ToDouble();
                return new List<CurrencySymbolPrice>()
                {
                    new CurrencySymbolPrice(result.TradingPairs.Split('_').FirstOrDefault(), result.TradingPairs.Split('_').Last(), Price, result.TradingPairs, ExchangeName),
                    new CurrencySymbolPriceReverted(result.TradingPairs.Split('_').FirstOrDefault(), result.TradingPairs.Split('_').Last(), Price, result.TradingPairs, ExchangeName),
                };
            })
            .SelectMany(o => o)
            .ToList();
        }

        private async Task<T> SendRequestAsync<T>(string sApi, Payload oPost = null) where T : new()
        {

            //var responseBodyHistory = web.UploadString($"{hostname}{"api/v4/main-account/history}",dataJsonStr);

            // If the nonce is similar to or lower than the previous request number, you will receive the 'too many requests' error message
            // nonce is a number that is always higher than the previous request number
            var nonce = GetNonce();

            //web.DefaultRequestHeaders.Add(HttpRequestHeader.ContentType, "application/json");

            try
            {
                string responseBody = string.Empty;

                if (oPost == null)
                {
                    HttpClient web = new HttpClient();

                    var response = await web.GetAsync($"{hostname}{sApi}");
                    responseBody = await response.Content.ReadAsStringAsync();
                }
                else
                {

                    oPost.Nonce = nonce;
                    oPost.Request = sApi;

                    var dataJsonStr = JsonConvert.SerializeObject(oPost);
                    var payload = dataJsonStr.Base64Encode();
                    var signature = CalcSignature(payload, oConfig.SecretKey);

                    HttpClient web = new HttpClient();
                    web.DefaultRequestHeaders.Add("X-TXC-APIKEY", oConfig.ApiKey);
                    web.DefaultRequestHeaders.Add("X-TXC-PAYLOAD", payload);
                    web.DefaultRequestHeaders.Add("X-TXC-SIGNATURE", signature);

                    var content = new StringContent(dataJsonStr, Encoding.UTF8, "application/json");

                    var response = await web.PostAsync($"{hostname}{sApi}", content);
                    responseBody = await response.Content.ReadAsStringAsync();
                }


                if (responseBody == "[]")
                    return new T();

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

        public async Task<List<GlobalTrade>> GetTradeHistoryAsync(List<GlobalTrade> aCache, List<GlobalBalance> aAllBalances)
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

                var oHistoryResponse = await SendRequestAsync<Dictionary<string, List<OrderHistory>>>("/api/v4/trade-account/executed-history", new PayloadOrderHistory() { Offset = nOffset, Limit = 100 });
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
                        GlobalTrade globalTrade = null;
                        if (oOrderHistory.Side == Side.Buy)
                        {
                            globalTrade = new GlobalTrade(cur.To, cur.From, oOrderHistory.Price.ToDouble(), cur.Couple, ExchangeName);
                            globalTrade.SetQuantities(oOrderHistory.Amount.ToDouble() / globalTrade.Price, oOrderHistory.Amount.ToDouble());
                        }
                        else
                        {
                            globalTrade = new GlobalTrade(cur.From, cur.To, oOrderHistory.Price.ToDouble(), cur.Couple, ExchangeName);
                            globalTrade.SetQuantities(oOrderHistory.Amount.ToDouble(), oOrderHistory.Amount.ToDouble() * globalTrade.Price);
                        }
                        globalTrade.InternalExchangeId = oOrderHistory.Id.ToString();
                        globalTrade.dtTrade = UnixTimeStampToDateTime(oOrderHistory.Time);
                        aListTrades.Add(globalTrade);

                    }
                }
                nOffset += 100;
            }



            return aListTrades;
        }

        private string IntervalToString(Interval interval)
        {
            switch (interval)
            {
                case Interval.Hour1: return "1h";
                case Interval.Hour2: return "2h";
                case Interval.Hour4: return "4h";
                case Interval.Hour8: return "8h";
                case Interval.Hour12: return "12h";
                case Interval.Hour24: return "1d";
            }
            return "4h";
        }

        public async Task<TradesData> GetTradeHistoryOneCoupleAsync(Interval interval, CurrencySymbolExchange symbol)
        {
            var oCall = BinanceCalls.ECalls.GetKLines;

            var end = GetTimeStamp(DateTime.Now);
            var start = GetTimeStamp(DateTime.Now.AddHours(-23 * 50));
            string intervalString = IntervalToString(interval);

            var tradeResponse = await SendRequestAsync<WhiteBitKLineSimple>($"/api/v1/public/kline?market={symbol.Couple}&interval={intervalString}&start={start}&end={end}");



            var result = tradeResponse
                .Result
                .Select(trade => new TradeData
                {
                    dtOpen = UnixTimeStampToDateTime(trade[0].ToDouble()),
                    openPrice = trade[1].ToDouble(),
                    highestPrice = trade[3].ToDouble(),
                    lowestPrice = trade[4].ToDouble(),
                    closePrice = trade[2].ToDouble(),
                    volume = trade[5].ToInt(0),
                    dtClose = UnixTimeStampToDateTime(trade[0].ToDouble()),
                    assetVolume = trade[6].ToInt(0),
                    numberOfTrades = 0,
                })
                .ToList();

            return new TradesData() { SymbolExchange = symbol, interval = interval, Trades = result };
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public async Task<List<GlobalBalance>> GetBalanceAsync()
        {

            var oBalance = await SendRequestAsync<Dictionary<string, Balance>>(request, new Payload() { });

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
        public partial class WhiteBitKLine
        {
            public bool Success { get; set; }
            public object Message { get; set; }
            public Result[] Result { get; set; }
        }

        public partial class WhiteBitKLineSimple
        {
            public bool Success { get; set; }
            public object Message { get; set; }
            public List<List<string>> Result { get; set; }
        }

        public partial class Result
        {
            public long Id { get; set; }
            public double Time { get; set; }
            public string Price { get; set; }
            public long Amount { get; set; }
            public string Type { get; set; }
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

        public partial class BinanceTradeMarketResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("message")]
            public object Message { get; set; }

            [JsonProperty("result")]
            public Result[] Result { get; set; }
        }

        public partial class Result
        {
            [JsonProperty("lastUpdateTimestamp")]
            public DateTimeOffset LastUpdateTimestamp { get; set; }

            [JsonProperty("tradingPairs")]
            public string TradingPairs { get; set; }

            [JsonProperty("lastPrice")]
            public string LastPrice { get; set; }

            [JsonProperty("lowestAsk")]
            public string LowestAsk { get; set; }

            [JsonProperty("highestBid")]
            public string HighestBid { get; set; }

            [JsonProperty("baseVolume24h")]
            public string BaseVolume24H { get; set; }

            [JsonProperty("quoteVolume24h")]
            public string QuoteVolume24H { get; set; }

            [JsonProperty("tradesEnabled")]
            public bool TradesEnabled { get; set; }
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
        private long GetTimeStamp(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime()
                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .TotalSeconds;
        }

        public Task<GlobalTrade> PlaceMarketOrderAsync(CurrencySymbol symbol, double quantity, SellBuy SellOrBuy, bool bTest)
        {
            throw new NotImplementedException();
        }
    }

}
