﻿using System;
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
using OpentWallet.Logic.Binance;
using OpenWallet.Logic.Abstraction;

namespace OpentWallet.Logic
{

    public class BinanceApi : IExchange
    {
        ExchangeConfig IExchange.oConfig { get; set; }

        private static List<BinanceCalls> ListCallsWeight = new List<BinanceCalls>()
        {
            new BinanceCalls(){Api = "/api/v3/account", eCall = BinanceCalls.ECalls.accountV3, Weight = 10},
            new BinanceCalls(){Api = "/api/v3/myTrades", eCall = BinanceCalls.ECalls.myTradesV3, Weight = 10},
            new BinanceCalls(){Api = "/api/v3/exchangeInfo", eCall = BinanceCalls.ECalls.ExchangeInfoV3, Weight = 10, PublicApi = true},
            new BinanceCalls(){Api = "/api/v3/ticker/price", eCall = BinanceCalls.ECalls.tickerPriceV3, Weight = 2, PublicApi = true},
            new BinanceCalls(){Api = "/sapi/v1/margin/allPairs", eCall = BinanceCalls.ECalls.allPairs, Weight = 1, PublicApi = true},
            new BinanceCalls(){Api = "/sapi/v1/lending/project/position/list", eCall = BinanceCalls.ECalls.earnings, Weight = 1},
            new BinanceCalls(){Api = "/api/v3/order", eCall = BinanceCalls.ECalls.placeOrder, Weight = 1, get = false},
            new BinanceCalls(){Api = "/api/v3/order/test", eCall = BinanceCalls.ECalls.placeOrderTest, Weight = 1, get = false},
        };

        private BinanceCalls GetCall(BinanceCalls.ECalls eCall) => ListCallsWeight.FirstOrDefault(bc => bc.eCall == eCall);

        private static List<BinanceCalls> _lastCalls = new List<BinanceCalls>();
        private BinanceExchangeInfo ExchangeInfo;

        private ExchangeConfig oConfig;
        private GlobalConfig oGlobalConfig;

        private const string hostname = "https://api.binance.com"; // put here your secret key

        private static readonly HttpClient _httpClient = new HttpClient();
        private BinanceAccount _oLastBalance;
        public string ExchangeName => "Binance";
        public string ExchangeCode => "Binance";
        public CurrenciesToCheck CurrenciesToCheck;

        public BinanceApi()
        {

        }

        public void Init(GlobalConfig oGlobalConfig, ExchangeConfig oConfig)
        {
            this.oConfig = oConfig;
            CurrenciesToCheck = JsonConvert.DeserializeObject<CurrenciesToCheck>(oConfig.LocalParams?.ToString());
            this.oGlobalConfig = oGlobalConfig;

            ExchangeInfo = GetExchangeInfo();

        }

        public List<CurrencySymbolPrice> GetCurrencies()
        {

            var oCurrencies = Call<List<BinanceCurrencies>>(BinanceCalls.ECalls.tickerPriceV3, string.Empty).Payload;
            var aPairs = Call<List<BinancePair>>(BinanceCalls.ECalls.allPairs, string.Empty).Payload.Select(p => new CurrencySymbol(p.Base, p.Quote, p.Symbol)).ToList();

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
                    new CurrencySymbolPrice(cur.From, cur.To, Price, cur.Couple, ExchangeName),
                    new CurrencySymbolPriceReverted(cur.From, cur.To, Price, cur.Couple, ExchangeName),
                };
            })
            .SelectMany(o => o)
            .Where(o => o != null)
            .ToList();

        }

        internal class LimitCalls
        {
            public DateTime StartCount { get; set; }
            public int Weight { get; set; }

            public static List<LimitCalls> GetLimits(BinanceExchangeInfo ExchangeInfo)
            {
                if (ExchangeInfo == null)
                    return new List<LimitCalls>();
                return ExchangeInfo.RateLimits.Select(rl =>
                {
                    if (rl.Interval == "MINUTE")
                    {
                        return new LimitCalls() { StartCount = DateTime.Now.AddMinutes(-rl.IntervalNum), Weight = rl.Limit };
                    }
                    if (rl.Interval == "DAY")
                    {
                        return new LimitCalls() { StartCount = DateTime.Now.AddDays(-rl.IntervalNum), Weight = rl.Limit };
                    }
                    if (rl.Interval == "SECOND")
                    {
                        return new LimitCalls() { StartCount = DateTime.Now.AddSeconds(-rl.IntervalNum), Weight = rl.Limit };
                    }
                    return null;
                })
                .Where(rl => rl != null)
                .ToList();
            }
        }

        public ApiResponse<T> Call<T>(BinanceCalls.ECalls Api, string dataJsonStr) where T : class, new()
        {


            var oCall = GetCall(Api);
            if (oCall == null)
                return new ApiResponse<T>();

            while (true)
            {
                if (Api == BinanceCalls.ECalls.ExchangeInfoV3)
                    break;

                if (LimitCalls.GetLimits(ExchangeInfo).Any(l =>
                {
                    return _lastCalls.Where(lc => lc.dtCall >= l.StartCount).Sum(lc => lc.Weight) > l.Weight;
                }))
                {
                    Task.Delay(250).GetAwaiter().GetResult();
                }
                else
                {
                    break;
                }
            }

            WebClient wc = new WebClient();
            string sApi = $"{hostname}{oCall.Api}";

            if (oCall.PublicApi == false)
            {
                // If the nonce is similar to or lower than the previous request number, you will receive the 'too many requests' error message
                // nonce is a number that is always higher than the previous request number
                var nonce = GetNonce();

                string sQueryParam = string.IsNullOrEmpty(dataJsonStr) ? $"timestamp={nonce}" : $"{dataJsonStr}&timestamp={nonce}";
                var signature = CalcSignature($"{sQueryParam}", oConfig.SecretKey);
                sApi += $"?{sQueryParam}&signature={signature}";
            }
            wc.Headers.Add("X-MBX-APIKEY", oConfig.ApiKey);

            //wc.Headers.Add("Api-Subaccount-Id", signature);
            wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            string responseBody = string.Empty;
            try
            {
                if (oCall.get)
                {
                    responseBody = wc.DownloadString($"{sApi}");

                }
                else
                {
                    //symbol=BTCUSDT&side=SELL&type=LIMIT&quantity=0.01&price=9000&timestamp=
                    responseBody = wc.UploadString($"{sApi}", "");

                }
                _lastCalls.Add(new BinanceCalls()
                {
                    Api = oCall.Api,
                    Weight = oCall.Weight,
                    eCall = oCall.eCall,
                    dtCall = DateTime.Now
                });
                return new ApiResponse<T>(JsonConvert.DeserializeObject<T>(responseBody));


            }
            catch (Exception ex)
            {
                return new ApiResponse<T>();
            }

        }

        public List<GlobalBalance> GetBalance()
        {
            //var o2 = Call<BinanceAccount>(BinanceCalls.ECalls.earnings, "asset=BNB");



            _oLastBalance = Call<BinanceAccount>(BinanceCalls.ECalls.accountV3, string.Empty).Payload;



            try
            {


                List<GlobalBalance> oGlobalBalance = _oLastBalance.Balances.Select(b =>
                {
                    double val = b.Free.ToDouble() + b.Locked.ToDouble();
                    if (val == 0)
                        return null;
                    return new GlobalBalance
                    {
                        Exchange = ExchangeName,
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

        public List<GlobalTrade> GetTradeHistory(List<GlobalTrade> aCache, List<GlobalBalance> aAllBalances)
        {
            //return aCache;
            var aListTrades = new List<GlobalTrade>(aCache);

            //if (_oLastBalance == null)
            //{
            //    return new List<GlobalTrade>();
            //}

            // pair from cache
            var aSymbols = aCache.GroupBy(c => c.Couple).Select(c => c.FirstOrDefault()).ToList();
            foreach (var oPair in aSymbols)
            {
                var aTrades = GetTradesFromCurrencies(oPair.From, oPair.To);
                aListTrades.AddRange(aTrades);
            }

            var aAllSymbols = ExchangeInfo.Symbols.OrderBy(s => s.SymbolSymbol).ToList();

            // pair from current balance
            foreach (var oPair in aAllSymbols)
            {
                if(oPair.BaseAsset == "THETA" || oPair.QuoteAsset == "THETA")
                {

                    if (aAllBalances.Any(ctc => ctc.Crypto == oPair.BaseAsset || ctc.Crypto == oPair.QuoteAsset))
                    {
                        var aTrades = GetTradesFromCurrencies(oPair.BaseAsset, oPair.QuoteAsset);
                        aListTrades.AddRange(aTrades.Where(t => aTrades.Any(t2 => t2.InternalExchangeId == t.InternalExchangeId) == false));

                    }
                }
            }

            // pair defined in config
            //foreach (var oPair in ExchangeInfo.Symbols)
            //{
            //    if (CurrenciesToCheck.aCurrenciesToCheck.Any(ctc => ctc == oPair.BaseAsset || ctc == oPair.QuoteAsset))
            //    {
            //        var aTrades = GetTradesFromCurrencies(oPair.BaseAsset, oPair.QuoteAsset);
            //            aListTrades.AddRange(aTrades);

            //    }
            //}

            return aListTrades;
        }

        private List<GlobalTrade> GetTradesFromCurrencies(string sFrom, string sTo)
        {
            var aListTrades = new List<GlobalTrade>();
            var oTradeList = Call<List<BinanceOrderHistory>>(BinanceCalls.ECalls.myTradesV3, $"symbol={sFrom}{sTo}").Payload;// + oPair.SymbolSymbol);
            foreach (var oTradeBinance in oTradeList)
            {

                var oGlobalTrade = new GlobalTrade();
                oGlobalTrade.Exchange = ExchangeName;
                if (oTradeBinance.IsBuyer)
                {
                    oGlobalTrade.From = sTo;
                    oGlobalTrade.To = sFrom;
                    oGlobalTrade.Price = 1 / oTradeBinance.Price.ToDouble();
                    oGlobalTrade.QuantityTo = oTradeBinance.Qty.ToDouble();
                    oGlobalTrade.QuantityFrom = oGlobalTrade.QuantityTo / oGlobalTrade.Price;
                }
                else
                {

                    oGlobalTrade.From = sFrom;
                    oGlobalTrade.To = sTo;
                    oGlobalTrade.Price = oTradeBinance.Price.ToDouble();
                    oGlobalTrade.QuantityFrom = oTradeBinance.Qty.ToDouble();
                    oGlobalTrade.QuantityTo = oGlobalTrade.QuantityFrom * oGlobalTrade.Price;
                }
                oGlobalTrade.InternalExchangeId = oTradeBinance.Id.ToString();
                oGlobalTrade.dtTrade = UnixTimeStampToDateTime(oTradeBinance.Time / 1000);
                aListTrades.Add(oGlobalTrade);

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

        private BinanceExchangeInfo GetExchangeInfo()
        {

            var oExchangeInfo = Call<BinanceExchangeInfo>(BinanceCalls.ECalls.ExchangeInfoV3, string.Empty).Payload;
            return oExchangeInfo;
        }

        string QuantityToString(double quantity, double tick)
        {
            var d2 = quantity.ToString(tick.ToString().Replace(',','.').Replace('0', '#').Replace('1', '#')).Replace(',', '.');
            return d2;

            string sQuantity = "";
            var start = 100.0;
            var decPart = 0;
            while (true)
            {
                if (quantity > start)
        {
                    sQuantity = quantity.ToString("F" + decPart);
                    break;
        }
                start /= 10;
                decPart++;
            }

            sQuantity = sQuantity.Replace(',', '.');
            if (sQuantity.Contains('.'))
            {
                sQuantity = sQuantity.TrimEnd('0', '.');
            }
            
            return sQuantity;
        }

        public bool PlaceMarketOrder(CurrencySymbol symbol, double quantity, SellBuy SellOrBuy, bool bTest)
        {
            var oSymbolFullInfo = ExchangeInfo.Symbols.FirstOrDefault(s => s.SymbolSymbol == symbol.Couple);
            var tick = oSymbolFullInfo.Filters.FirstOrDefault(f => f.FilterType == "LOT_SIZE")?.StepSize?.ToDouble();
            string sQuantity = QuantityToString(quantity, tick ?? 1.0);

            if (string.IsNullOrEmpty(sQuantity))
                return false;


            var oCall = bTest ? BinanceCalls.ECalls.placeOrderTest : BinanceCalls.ECalls.placeOrder;

            var oTradeList = Call<BinanceNewOrderResult>(oCall, $"symbol={symbol.Couple}&side={(SellOrBuy == SellBuy.Buy ? "BUY" : "SELL")}&type=MARKET&quantity={sQuantity}");

            return oTradeList.success;

        }
    }
}