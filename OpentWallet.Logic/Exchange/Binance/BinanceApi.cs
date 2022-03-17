using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OpenWallet.Common;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using OpentWallet.Logic.Binance;
using OpenWallet.Logic.Abstraction;

namespace OpentWallet.Logic
{

    public class BinanceApi : IExchange, IRefreshOneCoupleTrade
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
            new BinanceCalls(){Api = "/sapi/v1/lending/project/list", eCall = BinanceCalls.ECalls.lendingProjectList, Weight = 1},
            //new BinanceCalls(){Api = "/sapi/v1/lending/daily/token/position", eCall = BinanceCalls.ECalls.earnings, Weight = 1},
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
        public BinanceLocalParams LocalParam;

        public BinanceApi()
        {

        }

        public void Init(GlobalConfig oGlobalConfig, ExchangeConfig oConfig)
        {
            this.oConfig = oConfig;
            LocalParam = JsonConvert.DeserializeObject<BinanceLocalParams>(oConfig.LocalParams?.ToString());
            this.oGlobalConfig = oGlobalConfig;

            ExchangeInfo = GetExchangeInfo();

        }

        public List<CurrencySymbolPrice> GetCurrencies()
        {

            var oCurrencies = Call<List<BinanceCurrencies>>(BinanceCalls.ECalls.tickerPriceV3, string.Empty).Payload;
            var aPairs = ExchangeInfo.Symbols.Select(p => new CurrencySymbol(p.BaseAsset, p.QuoteAsset, p.SymbolSymbol)).ToList();

            return oCurrencies.Select(o =>
            {
                double Price = o.Price.ToDouble();

                var cur = aPairs.FirstOrDefault(c => c.From + c.To == o.Symbol);

                if (cur == null)
                {
                    return new List<CurrencySymbolPrice>();
                }
                return new List<CurrencySymbolPrice>()
                {
                    new CurrencySymbolPrice(cur.From, cur.To, Price,  cur.Couple, ExchangeName),
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
            // no way actually to get the earn account
            //var o2 = Call<BinanceAccount>(BinanceCalls.ECalls.earnings, "asset=ADA");
            //var o3 = Call<BinanceAccount>(BinanceCalls.ECalls.lendingProjectList, "type=CUSTOMIZED_FIXED");

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
            //var aSymbols = aCache.GroupBy(c => c.Couple).Select(c => c.FirstOrDefault()).ToList();
            //foreach (var oPair in aSymbols)
            //{
            //    var aTrades = GetTradesFromCurrencies(oPair.From, oPair.To);
            //    aListTrades.AddRange(aTrades);
            //}

            if (LocalParam.aPairsToCheck?.Any() == true)
            {

                // pair from config
                foreach (var oPair in LocalParam.aPairsToCheck)
                {
                    var aTrades = GetTradesFromCurrencies(oPair.Split('_').First(), oPair.Split('_').Last());
                    aListTrades.AddRange(aTrades);
                }
            }


            var aAllSymbols = ExchangeInfo.Symbols.OrderBy(s => s.SymbolSymbol).ToList();

            //// pair from current balance
            //foreach (var oPair in aAllSymbols)
            //{
            //    if (aAllBalances.Any(ctc => ctc.Crypto == oPair.BaseAsset || ctc.Crypto == oPair.QuoteAsset))
            //    {
            //        var aTrades = GetTradesFromCurrencies(oPair.BaseAsset, oPair.QuoteAsset);
            //        aListTrades.AddRange(aTrades.Where(t => aTrades.Any(t2 => t2.InternalExchangeId == t.InternalExchangeId) == false));

            //    }
            //}

            //return aListTrades;
            // pair defined in config
            if (LocalParam.checkcurrenciesToCheck)
            {

                foreach (var oPair in ExchangeInfo.Symbols)
                {
                    if (LocalParam.aCurrenciesToCheck.Any(ctc => ctc == oPair.BaseAsset || ctc == oPair.QuoteAsset))
                    {
                        var aTrades = GetTradesFromCurrencies(oPair.BaseAsset, oPair.QuoteAsset);
                        aListTrades.AddRange(aTrades);

                    }
                }
            }

            return aListTrades;
        }

        private List<GlobalTrade> GetTradesFromCurrencies(string sFrom, string sTo)
        {
            var aListTrades = new List<GlobalTrade>();
            var oTradeList = Call<List<BinanceOrderHistory>>(BinanceCalls.ECalls.myTradesV3, $"symbol={sFrom}{sTo}").Payload;// + oPair.SymbolSymbol);
            foreach (var oTradeBinance in oTradeList)
            {
                GlobalTrade globalTrade = null;
                if (oTradeBinance.IsBuyer)
                {
                    globalTrade = new GlobalTrade(sTo, sFrom, oTradeBinance.Price.ToDouble(), oTradeBinance.Symbol, ExchangeName);
                    globalTrade.SetQuantities(oTradeBinance.Qty.ToDouble() / globalTrade.Price, oTradeBinance.Qty.ToDouble());
                }
                else
                {
                    globalTrade = new GlobalTrade(sFrom, sTo, oTradeBinance.Price.ToDouble(), oTradeBinance.Symbol, ExchangeName);
                    globalTrade.SetQuantities(oTradeBinance.Qty.ToDouble(), oTradeBinance.Qty.ToDouble() * globalTrade.Price);
                }
                globalTrade.InternalExchangeId = oTradeBinance.Id.ToString();
                globalTrade.dtTrade = UnixTimeStampToDateTime(oTradeBinance.Time / 1000);
                aListTrades.Add(globalTrade);

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
            var d2 = quantity.ToString(tick.ToString().Replace(',', '.').Replace('0', '#').Replace('1', '#')).Replace(',', '.');
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

        public GlobalTrade PlaceMarketOrder(CurrencySymbol symbol, double quantity, SellBuy SellOrBuy, bool bTest)
        {
            var oSymbolFullInfo = ExchangeInfo.Symbols.FirstOrDefault(s => s.SymbolSymbol == symbol.Couple);
            var tick = oSymbolFullInfo.Filters.FirstOrDefault(f => f.FilterType == "LOT_SIZE")?.StepSize?.ToDouble();
            string sQuantity = QuantityToString(quantity, tick ?? 1.0);

            if (string.IsNullOrEmpty(sQuantity))
                return null;


            var oCall = bTest ? BinanceCalls.ECalls.placeOrderTest : BinanceCalls.ECalls.placeOrder;

            var oTradeResponse = Call<BinanceTradeMarketResponse>(oCall, $"symbol={symbol.Couple}&side={(SellOrBuy == SellBuy.Buy ? "BUY" : "SELL")}&type=MARKET&quantity={sQuantity}");

            if (bTest == true)
            {
                if (symbol.Couple == "ADAUSDT")
                {
                    if (SellOrBuy == SellBuy.Sell)
                    {
                        oTradeResponse.Payload = JsonConvert.DeserializeObject<BinanceTradeMarketResponse>("{\"symbol\":\"ADAUSDT\",\"orderId\":2628222193,\"orderListId\":-1,\"clientOrderId\":\"pS7CVFM3cdWhDIpOyGqc2j\",\"transactTime\":1637439625778,\"price\":\"0.00000000\",\"origQty\":\"10.00000000\",\"executedQty\":\"10.00000000\",\"cummulativeQuoteQty\":\"19.04000000\",\"status\":\"FILLED\",\"timeInForce\":\"GTC\",\"type\":\"MARKET\",\"side\":\"SELL\",\"fills\":[{\"price\":\"1.90400000\",\"qty\":\"10.00000000\",\"commission\":\"0.00002392\",\"commissionAsset\":\"BNB\",\"tradeId\":320123814}]}");
                    }
                    else
                    {
                        oTradeResponse.Payload = JsonConvert.DeserializeObject<BinanceTradeMarketResponse>("{\"symbol\":\"ADAUSDT\",\"orderId\":2628245736,\"orderListId\":-1,\"clientOrderId\":\"xdYJZn2UPkxayFJfN8zepE\",\"transactTime\":1637440243937,\"price\":\"0.00000000\",\"origQty\":\"7.90000000\",\"executedQty\":\"10.00000000\",\"cummulativeQuoteQty\":\"19.04000000\",\"status\":\"FILLED\",\"timeInForce\":\"GTC\",\"type\":\"MARKET\",\"side\":\"BUY\",\"fills\":[{\"price\":\"1.90300000\",\"qty\":\"7.90000000\",\"commission\":\"0.00001884\",\"commissionAsset\":\"BNB\",\"tradeId\":320128037}]}");
                    }
                }
            }


            if (oTradeResponse.success)
            {
                string quantityFrom = SellOrBuy == SellBuy.Buy ? oTradeResponse.Payload.CummulativeQuoteQty : oTradeResponse.Payload.ExecutedQty;
                string quantityTo = SellOrBuy == SellBuy.Buy ? oTradeResponse.Payload.ExecutedQty : oTradeResponse.Payload.CummulativeQuoteQty;
                return new GlobalTrade(symbol.From, symbol.To, oTradeResponse.Payload.Price?.ToDouble() ?? 0, symbol.Couple, ExchangeCode)
                {
                    CryptoFromId = symbol.CryptoFromId,
                    CryptoToId = symbol.CryptoToId,
                    dtTrade = DateTime.Now,
                    InternalExchangeId = oTradeResponse.Payload.ClientOrderId,
                }.SetQuantities(quantityFrom?.ToDouble() ?? 0, quantityTo?.ToDouble() ?? 0);
            }
            return null;

        }

        public List<GlobalTrade> GetTradeHistoryOneCouple(List<GlobalTrade> aCache, CurrencySymbol symbol)
        {
            var aListTrades = new List<GlobalTrade>(aCache);

            var aTrades = GetTradesFromCurrencies(symbol.RealFrom, symbol.RealTo);
            aListTrades.RemoveAll(t => t.Exchange == ExchangeCode && t.Couple == symbol.Couple);
            aListTrades.AddRange(aTrades);

            return aListTrades;
        }
    }
}
