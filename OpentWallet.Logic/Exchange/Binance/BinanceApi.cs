using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OpenWallet.Common;
using System.Collections.Generic;
using System.Linq;
using OpentWallet.Logic.Binance;
using OpenWallet.Logic.Abstraction;
using OpenWallet.Logic.Abstraction.Interfaces;

namespace OpentWallet.Logic
{

    public class BinanceApi : IExchange, IRefreshOneCoupleTrade, IGetTradesData
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
            new BinanceCalls(){Api = "/api/v3/klines", eCall = BinanceCalls.ECalls.GetKLines, Weight = 1, get = true, PublicApi = true},
        };

        private BinanceCalls GetCall(BinanceCalls.ECalls eCall) => ListCallsWeight.FirstOrDefault(bc => bc.eCall == eCall);

        private static List<BinanceCalls> _lastCalls = new List<BinanceCalls>();
        private BinanceExchangeInfo ExchangeInfo;

        private ExchangeConfig oConfig;
        private GlobalConfig oGlobalConfig;

        private const string hostname = "https://api.binance.com"; // put here your secret key
        private readonly IConfigService _configServie;
        private BinanceAccount _oLastBalance;
        public string ExchangeName => "Binance";
        public string ExchangeCode => "Binance";
        public BinanceLocalParams LocalParam;

        public BinanceApi(IConfigService configServie)
        {
            this._configServie = configServie;
        }

        public async Task InitAsync(GlobalConfig oGlobalConfig, ExchangeConfig oConfig)
        {
            this.oConfig = oConfig;
            LocalParam = JsonConvert.DeserializeObject<BinanceLocalParams>(oConfig.LocalParams?.ToString());
            this.oGlobalConfig = oGlobalConfig;

            ExchangeInfo = await GetExchangeInfoAsync();

        }

        public async Task<List<CurrencySymbolPrice>> GetCurrenciesAsync()
        {

            var oCurrencies = (await CallAsync<List<BinanceCurrencies>>(BinanceCalls.ECalls.tickerPriceV3, string.Empty)).Payload;
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

        private async Task<ApiResponse<T>> CallAsync<T>(BinanceCalls.ECalls Api, string dataJsonStr) where T : class, new()
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
                    await Task.Delay(250);
                }
                else
                {
                    break;
                }
            }


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
            else
            {
                sApi += $"?{dataJsonStr}";
            }

            HttpClient web = new HttpClient();

            //var web = new WebClient();
            web.DefaultRequestHeaders.Add("X-MBX-APIKEY", oConfig.ApiKey);

            //wc.Headers.Add("Api-Subaccount-Id", signature);
            //web.DefaultRequestHeaders.Add(HttpRequestHeader.ContentType, "application/json");
            try
            {
                string responseBody = string.Empty;
                if (oCall.get)
                {
                    var response = await web.GetAsync(sApi);
                    responseBody = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var response = await web.PostAsync(sApi, null);
                    responseBody = await response.Content.ReadAsStringAsync();
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

        public async Task<List<GlobalBalance>> GetBalanceAsync()
        {
            // no way actually to get the earn account
            //var o2 = Call<BinanceAccount>(BinanceCalls.ECalls.earnings, "asset=ADA");
            //var o3 = Call<BinanceAccount>(BinanceCalls.ECalls.lendingProjectList, "type=CUSTOMIZED_FIXED");

            _oLastBalance = (await CallAsync<BinanceAccount>(BinanceCalls.ECalls.accountV3, string.Empty)).Payload;

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

        public async Task<List<GlobalTrade>> GetTradeHistoryAsync(List<GlobalTrade> aCache, List<GlobalBalance> aAllBalances)
        {
            var aListTrades = new List<GlobalTrade>(aCache);


            var couples = ExchangeInfo.Symbols
                .Select(s => new { from = s.BaseAsset, to = s.QuoteAsset })
                .OrderBy(c => c.from)
                .ThenBy(c => c.to)
                .ToList();

            foreach(var c in couples)
            {
                aListTrades.AddRange(await GetTradesFromCurrenciesAsync(c.from, c.to));
            }

            return aListTrades;


            if (LocalParam.aPairsToCheck?.Any() == true)
            {
                foreach (var oPair in LocalParam.aPairsToCheck)
                {
                    var aTrades = await GetTradesFromCurrenciesAsync(oPair.Split('_').First(), oPair.Split('_').Last());
                    aListTrades.AddRange(aTrades);
                }
            }

            return aListTrades;
        }

        private async Task<List<GlobalTrade>> GetTradesFromCurrenciesAsync(string sFrom, string sTo)
        {


            var aListTrades = new List<GlobalTrade>();
            var oTradeList = (await CallAsync<List<BinanceOrderHistory>>(BinanceCalls.ECalls.myTradesV3, $"symbol={sFrom}{sTo}")).Payload;// + oPair.SymbolSymbol);
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

        private async Task<BinanceExchangeInfo> GetExchangeInfoAsync()
        {

            var oExchangeInfo = _configServie.LoadGenericFromCache<BinanceExchangeInfo>(this, "exchangeInfo");

            if(oExchangeInfo == null)
            {
                oExchangeInfo = (await CallAsync<BinanceExchangeInfo>(BinanceCalls.ECalls.ExchangeInfoV3, string.Empty)).Payload;
                _configServie.SaveGenericToCache(this, oExchangeInfo, "exchangeInfo");
            }


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

        public async Task<GlobalTrade> PlaceMarketOrderAsync(CurrencySymbol symbol, double quantity, SellBuy SellOrBuy, bool bTest)
        {
            var oSymbolFullInfo = ExchangeInfo.Symbols.FirstOrDefault(s => s.SymbolSymbol == symbol.Couple);
            var tick = oSymbolFullInfo.Filters.FirstOrDefault(f => f.FilterType == "LOT_SIZE")?.StepSize?.ToDouble();
            string sQuantity = QuantityToString(quantity, tick ?? 1.0);

            if (string.IsNullOrEmpty(sQuantity))
                return null;


            var oCall = bTest ? BinanceCalls.ECalls.placeOrderTest : BinanceCalls.ECalls.placeOrder;

            var oTradeResponse = await CallAsync<BinanceTradeMarketResponse>(oCall, $"symbol={symbol.Couple}&side={(SellOrBuy == SellBuy.Buy ? "BUY" : "SELL")}&type=MARKET&quantity={sQuantity}");

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
                    dtTrade = DateTime.Now,
                    InternalExchangeId = oTradeResponse.Payload.ClientOrderId,
                }.SetQuantities(quantityFrom?.ToDouble() ?? 0, quantityTo?.ToDouble() ?? 0);
            }
            return null;

        }

        public async Task<List<GlobalTrade>> GetTradeHistoryOneCoupleAsync(List<GlobalTrade> aCache, CurrencySymbol symbol)
        {
            var aListTrades = new List<GlobalTrade>(aCache);

            var aTrades = await GetTradesFromCurrenciesAsync(symbol.RealFrom, symbol.RealTo);
            aListTrades.RemoveAll(t => t.Exchange == ExchangeCode && t.Couple == symbol.Couple);
            aListTrades.AddRange(aTrades);

            return aListTrades;
        }

        public async Task<TradesData> GetTradeHistoryOneCoupleAsync(string interval, CurrencySymbolExchange symbol)
        {
            var oCall = BinanceCalls.ECalls.GetKLines;

            var tradeResponse = await CallAsync<List<List<string>>>(oCall, $"symbol={symbol.Couple}&interval={interval}");
            var result = tradeResponse
                .Payload
                .Select(trade => new TradeData
                {
                    dtOpen = UnixTimeStampToDateTime(trade[0].ToDouble() / 1000),
                    openPrice = trade[1].ToDouble(),
                    highestPrice = trade[2].ToDouble(),
                    lowestPrice = trade[3].ToDouble(),
                    closePrice = trade[4].ToDouble(),
                    volume = trade[5].ToInt(0),
                    dtClose = UnixTimeStampToDateTime(trade[6].ToDouble() / 1000),
                    assetVolume = trade[7].ToInt(0),
                    numberOfTrades = trade[7].ToInt(0),
                })
                .ToList();

            return new TradesData() { SymbolExchange = symbol, interval = interval, Trades = result };

        }

    }
}
