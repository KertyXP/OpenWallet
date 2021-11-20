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
using System.Web;
using OpenWallet.Logic.Abstraction;

namespace OpentWallet.Logic
{
    public class Huobi : IExchange
    {
        private ExchangeConfig oConfig;
        private GlobalConfig oGlobalConfig;
        ExchangeConfig IExchange.oConfig { get; set; }

        private readonly string hostname = "https://api.huobi.pro";
        private readonly string HUOBI_HOST = "api.huobi.pro";

        private const string HUOBI_SIGNATURE_METHOD = "HmacSHA256";
        private const int HUOBI_SIGNATURE_VERSION = 2;


        private const string GET_CONTRACT_INFO = "/api/v1/contract_contract_info";
        private const string GET_CONTRACT_INDEX = "/api/v1/contract_index";
        private const string GET_CONTRACT_PRICE_LIMIT = "/api/v1/contract_price_limit";
        private const string GET_CONTRACT_OPEN_INTEREST = "/api/v1/contract_open_interest";
        private const string GET_ACCOUNT = "/v1/account/accounts";
        private const string GET_CONTRACT_DEPTH = "/market/depth";
        private const string GET_CONTRACT_KLINE = "/market/history/kline";
        private const string POST_CANCEL_ORDER = "/api/v1/contract_cancel";
        private const string POST_ORDER_INFO = "/api/v1/contract_order_info";
        private const string POST_ORDER_DETAIL = "/api/v1/contract_order_detail";
        private const string POST_PLACE_ORDER = "/api/v1/contract_order";
        private const string POST_POSITION_ORDER = "/api/v1/contract_position_info";


        private WebClient client;

        public string ExchangeCode => "Huobi";
        public string ExchangeName => "Huobi";

        public Huobi()
        {
            client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.71 Safari/537.36");
        }

        public void Init(GlobalConfig oGlobalConfig, ExchangeConfig oConfig)
        {
            this.oConfig = oConfig;
            this.oGlobalConfig = oGlobalConfig;
        }


        public List<CurrencySymbolPrice> GetCurrencies()
        {
            WebClient wc = new WebClient();
            var sData = wc.DownloadString($"{hostname}/market/tickers");

            var oCurrencies = JsonConvert.DeserializeObject<HuobiTicker>(sData);
            return oCurrencies.Data.Select(kvp =>
            {
                double Price = (kvp.Ask + kvp.Bid) / 2;
                var oSymbol = CurrencySymbol.AutoDiscoverCurrencySymbol(kvp.Symbol.ToUpper());
                if (oSymbol != null)
                {
                    return new List<CurrencySymbolPrice>()
                    {
                        new CurrencySymbolPrice(oSymbol.From, oSymbol.To, Price, kvp.Symbol, ExchangeName),
                        new CurrencySymbolPriceReverted(oSymbol.From, oSymbol.To, Price, kvp.Symbol, ExchangeName),
                    };
                }
                return new List<CurrencySymbolPrice>();
            })
            .SelectMany(o => o)
            .Where(o => o != null)
            .ToList();
        }

        public List<GlobalBalance> GetBalance()
        {

            var result = SendRequest<List<HBAccount>>(GET_ACCOUNT);

            var oAccount = JsonConvert.DeserializeObject<HBAccount>(result);

            var result2 = SendRequest<List<HBAccount>>(GET_ACCOUNT + "/" + oAccount.Data.FirstOrDefault().Id + "/balance");
            var oBalance = JsonConvert.DeserializeObject<Balance>(result2);

            var aGlobalBalance = oBalance.Data.List
                .GroupBy(l => l.Currency)
                .Select(g =>
            {
                double val = g.Sum(f => f.Balance.ToDouble());
                if (val == 0)
                    return null;
                return new GlobalBalance()
                {
                    Exchange = ExchangeName,
                    Crypto = g.Key.ToUpper(),
                    Value = val,
                };
            })
                .Where(gb => gb != null && gb.Value > 0)
                .ToList();

            return aGlobalBalance;
        }

        public partial class HuobiTicker
        {
            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("ts")]
            public long Ts { get; set; }

            [JsonProperty("data")]
            public List<TickerData> Data { get; set; }
        }

        public partial class TickerData
        {
            [JsonProperty("symbol")]
            public string Symbol { get; set; }

            [JsonProperty("open")]
            public double Open { get; set; }

            [JsonProperty("high")]
            public double High { get; set; }

            [JsonProperty("low")]
            public double Low { get; set; }

            [JsonProperty("close")]
            public double Close { get; set; }

            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("vol")]
            public double Vol { get; set; }

            [JsonProperty("count")]
            public long Count { get; set; }

            [JsonProperty("bid")]
            public double Bid { get; set; }

            [JsonProperty("bidSize")]
            public double BidSize { get; set; }

            [JsonProperty("ask")]
            public double Ask { get; set; }

            [JsonProperty("askSize")]
            public double AskSize { get; set; }
        }

        public partial class Balance
        {
            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("data")]
            public Data Data { get; set; }
        }

        public partial class Data
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("state")]
            public string State { get; set; }

            [JsonProperty("list")]
            public List[] List { get; set; }
        }

        public partial class List
        {
            [JsonProperty("currency")]
            public string Currency { get; set; }

            [JsonProperty("type")]
            public TypeEnum Type { get; set; }

            [JsonProperty("balance")]
            public string Balance { get; set; }
        }
        public enum TypeEnum { Frozen, Trade };

        public partial class HBAccount
        {
            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("data")]
            public Datum[] Data { get; set; }
        }
        public partial class Datum
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("subtype")]
            public string Subtype { get; set; }

            [JsonProperty("state")]
            public string State { get; set; }
        }

        private string SendRequest<T>(string resourcePath, string parameters = "") where T : new()
        {
            parameters = UriEncodeParameterValue(GetCommonParameters() + parameters);
            var sign = GetSignatureStr(HttpMethod.Get, HUOBI_HOST, resourcePath, parameters);
            parameters += $"&Signature={sign}";

            var url = $"{hostname}{resourcePath}?{parameters}";
            Console.WriteLine(url);
            var request = client.DownloadString(url);

            return request;
        }
        //private HBResponse<T> SendRequest<T, P>(string resourcePath, P postParameters) where T : new()
        //{
        //    var parameters = UriEncodeParameterValue(GetCommonParameters());//请求参数
        //    var sign = GetSignatureStr(HttpMethod.Post, HUOBI_HOST, resourcePath, parameters);//签名
        //    parameters += $"&Signature={sign}";

        //    var url = $"{HUOBI_HOST_URL}{resourcePath}?{parameters}";
        //    Console.WriteLine(url);
        //    var request = new RestRequest(url, Method.POST);
        //    request.AddJsonBody(postParameters);
        //    foreach (var item in request.Parameters)
        //    {
        //        item.Value = item.Value.ToString();
        //    }
        //    var result = client.Execute<HBResponse<T>>(request);
        //    return result.Data;
        //}

        private string GetCommonParameters()
        {
            return $"AccessKeyId={oConfig.ApiKey}&SignatureMethod={HUOBI_SIGNATURE_METHOD}&SignatureVersion={HUOBI_SIGNATURE_VERSION}&Timestamp={DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")}";
        }
        private string UriEncodeParameterValue(string parameters)
        {
            var sb = new StringBuilder();
            var paraArray = parameters.Split('&');
            var sortDic = new SortedDictionary<string, string>();
            foreach (var item in paraArray)
            {
                var para = item.Split('=');
                sortDic.Add(para.First(), UrlEncode(para.Last()));
            }
            foreach (var item in sortDic)
            {
                sb.Append(item.Key).Append("=").Append(item.Value).Append("&");
            }
            return sb.ToString().TrimEnd('&');
        }
        public string UrlEncode(string str)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in str)
            {
                if (HttpUtility.UrlEncode(c.ToString(), Encoding.UTF8).Length > 1)
                {
                    builder.Append(HttpUtility.UrlEncode(c.ToString(), Encoding.UTF8).ToUpper());
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }
        private static string CalculateSignature256(string text, string secretKey)
        {
            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                return Convert.ToBase64String(hashmessage);
            }
        }
        private string GetSignatureStr(HttpMethod method, string host, string resourcePath, string parameters)
        {
            var sign = string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.Append(method.ToString().ToUpper()).Append("\n")
                .Append(host).Append("\n")
                .Append(resourcePath).Append("\n");
            //参数排序
            var paraArray = parameters.Split('&');
            List<string> parametersList = new List<string>();
            foreach (var item in paraArray)
            {
                parametersList.Add(item);
            }
            parametersList.Sort(delegate (string s1, string s2) { return string.CompareOrdinal(s1, s2); });
            foreach (var item in parametersList)
            {
                sb.Append(item).Append("&");
            }
            sign = sb.ToString().TrimEnd('&');
            sign = CalculateSignature256(sign, oConfig.SecretKey);
            return UrlEncode(sign);
        }

        public List<GlobalTrade> GetTradeHistory(List<GlobalTrade> aCache, List<GlobalBalance> aAllBalances)
        {
            return new List<GlobalTrade>();
        }

        public GlobalTrade PlaceMarketOrder(CurrencySymbol symbol, double quantity, SellBuy SellOrBuy, bool bTest)
        {
            throw new NotImplementedException();
        }
    }


    public class HBResponse<T> where T : new()
    {
        public string Status { get; set; }
        public T Data { get; set; }
    }

    public class HBAccount
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        [JsonProperty(PropertyName = "user-id")]
        public long UserId { get; set; }
    }
}
