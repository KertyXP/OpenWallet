﻿//using System;
//using Newtonsoft.Json;
//using System.Net.Http;
//using System.Security.Cryptography;
//using System.Text;
//using OpenWallet.Common;
//using System.Net;
//using System.Collections.Generic;
//using System.Linq;
//using OpenWallet.Logic.Abstraction;

//namespace OpentWallet.Logic
//{
//    public class Bittrex : IExchange
//    {
//        ExchangeConfig IExchange.oConfig { get; set; }
//        private ExchangeConfig oConfig;
//        private GlobalConfig oGlobalConfig;

//        private const string hostname = "https://api.bittrex.com"; // put here your secret key
//        private const string apiBalance = "/v3/balances"; // put here your secret key
//        private const string apiOrderHitory = "/v3/orders/closed"; // put here your secret key

//        private static readonly HttpClient _httpClient = new HttpClient();

//        public string ExchangeCode => "Bittrex";
//        public string ExchangeName => "Bittrex";

//        public Bittrex()
//        {

//        }

//        public void InitAsync(GlobalConfig oGlobalConfig, ExchangeConfig oConfig)
//        {
//            this.oConfig = oConfig;
//            this.oGlobalConfig = oGlobalConfig;
//        }

//        internal class Payload
//        {
//            [JsonProperty("request")]
//            public string Request { get; set; }

//            [JsonProperty("nonce")]
//            public string Nonce { get; set; }
//        }
//        internal class PayloadWithTicker : Payload
//        {

//            [JsonProperty("ticker")]
//            public string Ticker { get; set; }
//        }


//        public List<CurrencySymbolPrice> GetCurrencies()
//        {
//            WebClient wc = new WebClient();
//            var sData = wc.DownloadString($"{hostname}/v3/markets/tickers");

//            var oCurrencies = JsonConvert.DeserializeObject<List<BittrexCurrencies>>(sData);
//            return oCurrencies.Select(kvp =>
//            {
//                double Price = (kvp.AskRate.ToDouble() + kvp.BidRate.ToDouble()) / 2;
//                return new List<CurrencySymbolPrice>()
//                {
//                    new CurrencySymbolPrice(kvp.Symbol.Split('-').FirstOrDefault(), kvp.Symbol.Split('-').Last(), Price, kvp.Symbol, ExchangeName),
//                    new CurrencySymbolPriceReverted(kvp.Symbol.Split('-').FirstOrDefault(), kvp.Symbol.Split('-').Last(), Price, kvp.Symbol, ExchangeName),
//                };
//            })
//            .SelectMany(o => o)
//            .ToList();
//        }


//        public List<GlobalBalance> GetBalance()
//        {


//            // If the nonce is similar to or lower than the previous request number, you will receive the 'too many requests' error message
//            // nonce is a number that is always higher than the previous request number
//            var nonce = GetNonce();

//            string sApi = $"{hostname}{apiBalance}";

//            var dataJsonStr = string.Empty;
//            var hash = CalcHash(dataJsonStr);
//            var signature = CalcSignature(nonce + sApi + "GET" + hash, oConfig.SecretKey);


//            WebClient wc = new WebClient();
//            wc.Headers.Add("Api-Key", oConfig.ApiKey);
//            wc.Headers.Add("Api-Timestamp", nonce);
//            wc.Headers.Add("Api-Signature", signature);
//            wc.Headers.Add("Api-Content-Hash", hash);
//            //wc.Headers.Add("Api-Subaccount-Id", signature);
//            wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");

//            var responseBody = wc.DownloadString($"{hostname}{apiBalance}");



//            var oBalance = JsonConvert.DeserializeObject<List<BittrexBalence>>(responseBody);
//            List<GlobalBalance> oGlobalBalance = oBalance.Select(b =>
//            {
//                return new GlobalBalance
//                {
//                    Exchange = ExchangeName,
//                    Crypto = b.CurrencySymbol,
//                    Value = b.Total.ToDouble(),
//                };
//            }
//                ).Where(gb => gb.Value > 0).ToList();

//            return oGlobalBalance;
//        }

//        public List<GlobalTrade> GetTradeHitory(List<GlobalTrade> aCache, List<GlobalBalance> aAllBalances)
//        {
//            string sApi = $"{hostname}{apiOrderHitory}";
//            var nonce = GetNonce();


//            var dataJsonStr = string.Empty;
//            var hash = CalcHash(dataJsonStr);
//            var signature = CalcSignature(nonce + sApi + "GET" + hash, oConfig.SecretKey);


//            WebClient wc = new WebClient();
//            wc.Headers.Add("Api-Key", oConfig.ApiKey);
//            wc.Headers.Add("Api-Timestamp", nonce);
//            wc.Headers.Add("Api-Signature", signature);
//            wc.Headers.Add("Api-Content-Hash", hash);
//            //wc.Headers.Add("Api-Subaccount-Id", signature);
//            wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");

//            var responseBody = wc.DownloadString($"{sApi}");

//            List<GlobalTrade> aListTrades = new List<GlobalTrade>(aCache);


//            //var balance = JsonConvert.DeserializeObject<AscendexOrderHitory>(request2);


//            //foreach (var oOrderHitory in balance.Data)
//            //{

//            //    if (aListTrades.Any(lt => lt.InternalExchangeId == oOrderHitory.SeqNum))
//            //    {
//            //        continue;
//            //    }
//            //    var cur = new CurrencySymbol(oOrderHitory.Symbol.Split('/').FirstOrDefault(), oOrderHitory.Symbol.Split('/').LatorDefault());

//            //    var oGlobalTrade = new GlobalTrade();
//            //    oGlobalTrade.Exchange = ExchangeName;
//            //    if (oOrderHitory.Side == "Buy")
//            //    {
//            //        oGlobalTrade.From = cur.To;
//            //        oGlobalTrade.To = cur.From;
//            //        oGlobalTrade.Price = 1 / oOrderHitory.Price.ToDouble();
//            //        oGlobalTrade.QuantityTo = oOrderHitory.OrderQty.ToDouble();
//            //        oGlobalTrade.QuantityFrom = oGlobalTrade.QuantityTo / oGlobalTrade.Price;
//            //    }
//            //    else
//            //    {

//            //        oGlobalTrade.From = cur.From;
//            //        oGlobalTrade.To = cur.To;
//            //        oGlobalTrade.Price = oOrderHitory.Price.ToDouble();
//            //        oGlobalTrade.QuantityFrom = oOrderHitory.OrderQty.ToDouble();
//            //        oGlobalTrade.QuantityTo = oGlobalTrade.QuantityFrom * oGlobalTrade.Price;
//            //    }
//            //    oGlobalTrade.InternalExchangeId = oOrderHitory.SeqNum;
//            //    oGlobalTrade.dtTrade = UnixTimeStampToDateTime(oOrderHitory.LastExecTime / 1000);
//            //    aListTrades.Add(oGlobalTrade);

//            //}

//            return aListTrades;

//        }


//        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
//        {
//            // Unix timestamp is seconds past epoch
//            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
//            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
//            return dtDateTime;
//        }

//        public string CalcSignature(string text, string apiSecret)
//        {
//            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(apiSecret)))
//            {
//                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
//                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
//            }
//        }
//        public string CalcHash(string text)
//        {
//            SHA512 shaM = new SHA512Managed();

//            return BitConverter.ToString(shaM.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", string.Empty).ToLower();
//        }
//        private string GetNonce()
//        {
//            var milliseconds = (long)DateTime.Now.ToUniversalTime()
//                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
//                .TotalMilliseconds;

//            return milliseconds.ToString();
//        }

//        public GlobalTrade PlaceMarketOrder(CurrencySymbol symbol, double quantity, SellBuy SellOrBuy, bool bTest)
//        {
//            throw new NotImplementedException();
//        }
//    }


//    public partial class BittrexCurrencies
//    {
//        [JsonProperty("symbol")]
//        public string Symbol { get; set; }

//        [JsonProperty("lastTradeRate")]
//        public string LastTradeRate { get; set; }

//        [JsonProperty("bidRate")]
//        public string BidRate { get; set; }

//        [JsonProperty("askRate")]
//        public string AskRate { get; set; }
//    }

//    public partial class BittrexBalence
//    {
//        [JsonProperty("currencySymbol")]
//        public string CurrencySymbol { get; set; }

//        [JsonProperty("total")]
//        public string Total { get; set; }

//        [JsonProperty("available")]
//        public string Available { get; set; }

//        [JsonProperty("updatedAt")]
//        public DateTimeOffset UpdatedAt { get; set; }
//    }
//}
