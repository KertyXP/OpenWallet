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
using OpentWallet.Logic.Binance;
using OpenWallet.Logic.Abstraction;

namespace OpentWallet.Logic
{
    public class CurrenciesToCheck
    {
        [JsonProperty("currenciesToCheck")]
        public List<string> aCurrenciesToCheck { get; set; }
    }

    public class BinanceCalls
    {
        public enum ECalls { accountV3, myTradesV3, ExchangeInfoV3, tickerPriceV3, allPairs }
        public ECalls eCall { get; set; }
        public string Api { get; set; }
        public int Weight { get; set; }
        public DateTime dtCall { get; set; }
        public bool PublicApi { get; set; }
    }

    public class BinanceApi : IExchange
    {

        private static List<BinanceCalls> ListCallsWeight = new List<BinanceCalls>()
        {
            new BinanceCalls(){Api = "/api/v3/account", eCall = BinanceCalls.ECalls.accountV3, Weight = 10},
            new BinanceCalls(){Api = "/api/v3/myTrades", eCall = BinanceCalls.ECalls.myTradesV3, Weight = 10},
            new BinanceCalls(){Api = "/api/v3/exchangeInfo", eCall = BinanceCalls.ECalls.ExchangeInfoV3, Weight = 10, PublicApi = true},
            new BinanceCalls(){Api = "/api/v3/ticker/price", eCall = BinanceCalls.ECalls.tickerPriceV3, Weight = 2, PublicApi = true},
            new BinanceCalls(){Api = "/sapi/v1/margin/allPairs", eCall = BinanceCalls.ECalls.allPairs, Weight = 1, PublicApi = true},
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
            CurrenciesToCheck = JsonConvert.DeserializeObject<CurrenciesToCheck>(oConfig.LocalParams.ToString());
            this.oGlobalConfig = oGlobalConfig;

            ExchangeInfo = GetExchangeInfo();

        }

        public List<CurrencySymbolPrice> GetCurrencies()
        {

            var oCurrencies = Call<List<BinanceCurrencies>>(BinanceCalls.ECalls.tickerPriceV3, string.Empty);
            var aPairs = Call<List<BinancePair>>(BinanceCalls.ECalls.allPairs, string.Empty).Select(p => new CurrencySymbol(p.Base, p.Quote)).ToList();

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
                    new CurrencySymbolPrice(cur.From, cur.To, Price, ExchangeName),
                    new CurrencySymbolPriceReverted(cur.From, cur.To, Price, ExchangeName),
                };
            })
            .SelectMany(o => o)
            .Where(o => o != null)
            .ToList();

        }

        public T Call<T>(BinanceCalls.ECalls Api, string dataJsonStr) where T : class, new()
        {


            var oCall = GetCall(Api);
            if (oCall == null)
                return new T();


            if (ExchangeInfo != null)
            {
                ExchangeInfo.RateLimits
            }

            WebClient wc = new WebClient();
            string sApi = $"{hostname}{oCall.Api}";

            if (oCall.PublicApi == false)
            {
                // If the nonce is similar to or lower than the previous request number, you will receive the 'too many requests' error message
                // nonce is a number that is always higher than the previous request number
                var nonce = GetNonce();

                string sQueryParam = $"timestamp={nonce}";
                var signature = CalcSignature($"{sQueryParam}{dataJsonStr}", oConfig.SecretKey);
                sApi += $"?{sQueryParam}&signature={signature}";
            }
            wc.Headers.Add("X-MBX-APIKEY", oConfig.ApiKey);

            //wc.Headers.Add("Api-Subaccount-Id", signature);
            wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            string responseBody = string.Empty;
            try
            {

                responseBody = wc.DownloadString($"{sApi}");
                _lastCalls.Add(new BinanceCalls()
                {
                    Api = oCall.Api,
                    Weight = oCall.Weight,
                    eCall = oCall.eCall,
                    dtCall = DateTime.Now
                });
                return JsonConvert.DeserializeObject<T>(responseBody);


            }
            catch (Exception ex)
            {
                return new T();
            }

        }

        public List<GlobalBalance> GetBalance()
        {



            _oLastBalance = Call<BinanceAccount>(BinanceCalls.ECalls.accountV3, string.Empty);



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

        public List<GlobalTrade> GetTradeHistory(List<GlobalTrade> aCache)
        {
            var aTrades = new List<GlobalTrade>();

            //if (_oLastBalance == null)
            //{
            //    return new List<GlobalTrade>();
            //}


            //foreach (var oBalance in _oLastBalance.Balances)
            //{


            //    var cur = CurrencySymbol.AutoDiscoverCurrencySymbol(oBalance.Asset);
            //    if (cur == null)
            //        continue; // oops


            //    var nonce = GetNonce();


            //    var dataJsonStr = string.Empty;

            //    string sQueryParam = $"timestamp={nonce}";

            //    var signature = CalcSignature($"{sQueryParam}{dataJsonStr}", oConfig.SecretKey);

            //    string sApi = $"{hostname}{apiOrdersHistory}?{sQueryParam}&signature={signature}";


            //    WebClient wc = new WebClient();
            //    wc.Headers.Add("X-MBX-APIKEY", oConfig.ApiKey);
            //    //wc.Headers.Add("Api-Subaccount-Id", signature);
            //    wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            //    string responseBody = string.Empty;
            //    try
            //    {

            //        responseBody = wc.DownloadString($"{sApi}");


            //    }
            //    catch (Exception ex)
            //    {
            //        continue;
            //    }
            //    try
            //    {

            //        var oBinanceTrades = JsonConvert.DeserializeObject<List<BinanceTrades>>(responseBody);

            //        List<GlobalTrade> oGlobalBalance = oBinanceTrades.Select(oOrderHistory =>
            //                {

            //                    var oGlobalTrade = new GlobalTrade();
            //                    //oGlobalTrade.Exchange = ExchangeName;
            //                    //if (oOrderHistory.IsBuyer == true)
            //                    //{
            //                    //    oGlobalTrade.From = cur.To;
            //                    //    oGlobalTrade.To = cur.From;
            //                    //    oGlobalTrade.Price = 1 / oOrderHistory.Price.ToDouble();
            //                    //    oGlobalTrade.QuantityTo = oOrderHistory.Amount.ToDouble();
            //                    //    oGlobalTrade.QuantityFrom = oGlobalTrade.QuantityTo / oGlobalTrade.Price;
            //                    //}
            //                    //else
            //                    //{

            //                    //    oGlobalTrade.From = cur.From;
            //                    //    oGlobalTrade.To = cur.To;
            //                    //    oGlobalTrade.Price = oOrderHistory.Price.ToDouble();
            //                    //    oGlobalTrade.QuantityFrom = oOrderHistory.Amount.ToDouble();
            //                    //    oGlobalTrade.QuantityTo = oGlobalTrade.QuantityFrom * oGlobalTrade.Price;
            //                    //}
            //                    //oGlobalTrade.InternalExchangeId = oOrderHistory.ClientOrderId;
            //                    //oGlobalTrade.dtTrade = UnixTimeStampToDateTime(oOrderHistory.Time);
            //                    //aListTrades.Add(oGlobalTrade);
            //                    return oGlobalTrade;
            //                }
            //            )
            //            .Where(gb => gb != null)
            //            //.Where(gb => gb.Value > 0)
            //            .ToList();

            //        return oGlobalBalance;

            //    }
            //    catch (Exception ex)
            //    {
            //        return new List<GlobalTrade>();
            //    }
            //}

            return aTrades;
        }

        private BinanceExchangeInfo GetExchangeInfo()
        {

            var oExchangeInfo = Call<BinanceExchangeInfo>(BinanceCalls.ECalls.ExchangeInfoV3, string.Empty);
            return oExchangeInfo;
        }
    }
    public partial class BinanceTrades
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("orderId")]
        public long OrderId { get; set; }

        [JsonProperty("orderListId")]
        public long OrderListId { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("qty")]
        public string Qty { get; set; }

        [JsonProperty("quoteQty")]
        public string QuoteQty { get; set; }

        [JsonProperty("commission")]
        public string Commission { get; set; }

        [JsonProperty("commissionAsset")]
        public string CommissionAsset { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("isBuyer")]
        public bool IsBuyer { get; set; }

        [JsonProperty("isMaker")]
        public bool IsMaker { get; set; }

        [JsonProperty("isBestMatch")]
        public bool IsBestMatch { get; set; }
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
