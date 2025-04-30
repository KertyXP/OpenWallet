//using System;
//using Newtonsoft.Json;
//using System.Threading.Tasks;
//using OpenWallet.Common;
//using System.Net;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text.RegularExpressions;
//using OpenWallet.Logic.Abstraction;
//using System.IO;

//namespace OpentWallet.Logic
//{
//    public class BscWallet : IExchange
//    {
//        ExchangeConfig IExchange.oConfig { get; set; }
//        private ExchangeConfig oConfig;
//        private GlobalConfig oGlobalConfig;

//        private const string host = "https://bscscan.com"; // put here your secret key
//        private const string getAddress = "address"; // put here your secret key
//        private const string getTrades = "txs?a="; // put here your secret key
//        private const string getTradeDetail = "tx"; // put here your secret key

//        public string ExchangeCode => "BSC";
//        public string ExchangeName => "BSC";

//        public BscWallet()
//        {
//        }
//        public List<CurrencySymbolPrice> GetCurrencies()
//        {
//            return new List<CurrencySymbolPrice>();
//            //var wc = new WebClient();
//            //var sResult = wc.DownloadString("https://api.coinmarketcap.com/data-api/v3/cryptocurrency/listing?start=1&limit=10000&sortBy=market_cap&sortType=desc&convert=usdt&cryptoType=tokens&tagType=all&aux=ath,atl,high24h,low24h,num_market_pairs,cmc_rank,date_added,tags,platform,max_supply,circulating_supply,total_supply,volume_7d,volume_30d");

//            //var oBcd = JsonConvert.DeserializeObject<BscTicker>(sResult);

//            //return oBcd.Data.CryptoCurrencyList.Select(c =>
//            //{

//            //    return new List<CurrencySymbolPrice>()
//            //        {
//            //            new CurrencySymbolPrice(c.Symbol, c.Quotes.FirstOrDefault().Name.ToUpper(), c.Quotes.FirstOrDefault().Price, ExchangeName),
//            //            new CurrencySymbolPriceReverted(c.Symbol, c.Quotes.FirstOrDefault().Name.ToUpper(), c.Quotes.FirstOrDefault().Price, ExchangeName),
//            //        };
//            //})
//            //.SelectMany(o => o)
//            //.Where(o => o != null)
//            //.ToList();
//        }

//        public void InitAsync(GlobalConfig oGlobalConfig, ExchangeConfig oConfig)
//        {
//            this.oConfig = oConfig;
//            this.oGlobalConfig = oGlobalConfig;
//        }


//        public List<GlobalBalance> GetBalance()
//        {

//            var wcBnbBtc = new WebClient();

//            var sHTMLBnbBtc = wcBnbBtc.DownloadString($"https://freecurrencyrates.com/fr/convert-BNB-BTC");
//            var oDocBnbBtc = new HtmlAgilityPack.HtmlDocument();
//            oDocBnbBtc.LoadHtml(sHTMLBnbBtc);
//            //<input type="text" id="value_from" class="thin cp-input" value="10">
//            var bnbFrom = oDocBnbBtc.DocumentNode.Descendants("input").FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value == "value_from").Attributes["value"].Value;
//            var btcTo = oDocBnbBtc.DocumentNode.Descendants("input").FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value == "value_to").Attributes["value"].Value;
//            var bnbBtcPrice = bnbFrom.ToDouble() / btcTo.ToDouble();
//            WebClient wc = new WebClient();
//            var sHTML = wc.DownloadString($"{host}/{getAddress}/{oConfig.ApiKey}");

//            var oDoc = new HtmlAgilityPack.HtmlDocument();
//            oDoc.LoadHtml(sHTML);

//            var aBalance = new List<GlobalBalance>();

//            var oSummary = oDoc
//                .DocumentNode
//                .Descendants("div")
//                .Where(x => x.Attributes.Contains("id") && x.Attributes["id"].Value == "ContentPlaceHolder1_divSummary")
//                .FirstOrDefault()
//                .InnerText
//                .Replace('\n', ' ');

//            var sBnb = Regex.Replace(oSummary, ".*Balance: (.*) BNB.* BNB Value.*", "$1");
//            aBalance.Add(new GlobalBalance()
//            {
//                Exchange = ExchangeName,
//                Crypto = "BNB",
//                Value = sBnb.ToDouble(),
//                BitCoinValue = sBnb.ToDouble() / bnbBtcPrice
//            });
//            var aTokens = oDoc.DocumentNode.Descendants("li").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "list-cutom list-cutom-BEP-20");
//            foreach (var oToken in aTokens)
//            {
//                var oAmount = oToken.Descendants("span").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.StartsWith("list-amount")).FirstOrDefault().InnerText;
//                var token = oToken.Descendants("a").FirstOrDefault().Attributes.FirstOrDefault(a => a.Name == "href").Value;
//                token = token.Split('/').LatorDefault();
//                token = token.Split('?').FirstOrDefault();
//                var oChart = PooCoin.GetChart(token);
//                var LastPrice = oChart.Data.Ethereum.DexTrades.LatorDefault();
//                var Amount = oAmount.Split(' ').FirstOrDefault().Replace(",", "").ToDouble();
//                var ValueInBtc = (LastPrice?.ClosePrice?.ToDouble() ?? 0) * Amount / bnbBtcPrice;
//                aBalance.Add(new GlobalBalance()
//                {
//                    Exchange = ExchangeName,
//                    Crypto = oAmount.Split(' ').LatorDefault(),
//                    CryptoId = token,
//                    Value = Amount,
//                    BitCoinValue = ValueInBtc
//                });
//            }

//            //List<GlobalBalance> oGlobalBalance = oBalance.Balances.Select(b =>
//            //{
//            //    return new GlobalBalance
//            //    {
//            //        Exchange = "Binance",
//            //        Crypto = b.Asset,
//            //        Value = b.Free.ToDouble() + b.Locked.ToDouble()
//            //    };
//            //}
//            //    ).Where(gb => gb.Value > 0).ToList();

//            //return oGlobalBalance;

//            return aBalance;
//        }

//        public List<GlobalTrade> GetTradeHitory(List<GlobalTrade> aCache, List<GlobalBalance> aAllBalances)
//        {
//            var oTempTrades = new List<GlobalTrade>();
//            var oTrades = new List<GlobalTrade>();
//            bool bOneFound = false;

//            // get lists of transactions
//            for (int i = 1; i < 100; i++)
//            {
//                bOneFound = false;
//                WebRequest wc = HttpWebRequest.Create(new Uri($"{host}/{getTrades}{oConfig.ApiKey}&p={i}"));
//                var response = wc.GetResponse();

//                Stream dataStream = response.GetResponseStream();
//                StreamReader sr = new StreamReader(dataStream);
//                string sHTML = sr.ReadToEnd();

//                var oDoc = new HtmlAgilityPack.HtmlDocument();
//                oDoc.LoadHtml(sHTML);



//                var tr = oDoc.DocumentNode.Descendants("tr").ToList();


//                foreach (var oRow in tr)
//                {

//                    var sTradeId = oRow.Descendants("a")
//                        .Where(x => x.Attributes.Contains("href") && x.Attributes["href"].Value?.StartsWith("/tx/") == true)
//                        .Select(a => a.Attributes["href"].Value.Split('/')
//                        .LatorDefault())
//                        .FirstOrDefault();


//                    if (sTradeId == null)
//                        continue;

//                    bOneFound = true;
//                    var oDate = oRow.Descendants("td").FirstOrDefault(td => td.Attributes.Contains("class") && td.Attributes["class"].Value.Trim() == "showAge")
//                        ?.Descendants("span").FirstOrDefault()?.Attributes["title"]?.Value;

//                    var dt = DateTime.ParseExact(oDate, "yyyy-MM-dd H:mm:ss", CultureInfo.InvariantCulture.DateTimeFormat).ToLocalTime();

//                    oTempTrades.Add(new GlobalTrade(string.Empty, string.Empty, 0f, string.Empty, ExchangeName)
//                    {
//                        InternalExchangeId = sTradeId,
//                        dtTrade = dt
//                    });
//                }

//                if (bOneFound == false)
//                    break;

//            }


//            foreach (var oTrade in oTempTrades.GroupBy(a => a.InternalExchangeId).Select(a => a.FirstOrDefault()))
//            {
//                aCache = aCache ?? new List<GlobalTrade>();
//                var oCache = aCache.FirstOrDefault(c => c.InternalExchangeId == oTrade.InternalExchangeId);
//                if (oCache != null)
//                {
//                    if(oCache.dtTrade != DateTime.MinValue)
//                    {
//                        oTrades.Add(oCache);
//                        continue;
//                    }
//                }
//                string sHTML = string.Empty;
//                while (true)
//                {
//                    WebRequest wc = HttpWebRequest.Create(new Uri($"{host}/{getTradeDetail}/{oTrade.InternalExchangeId}"));
//                    var response = wc.GetResponse();

//                    Stream dataStream = response.GetResponseStream();
//                    StreamReader sr = new StreamReader(dataStream);
//                    sHTML = sr.ReadToEnd();

//                    if (response.ResponseUri.AbsolutePath == "/busy")
//                    {
//                        // too many request?
//                        Task.Delay(1000).GetAwaiter().GetResult();
//                        continue;
//                    }

//                    break;
//                }

//                var oDoc = new HtmlAgilityPack.HtmlDocument();
//                oDoc.LoadHtml(sHTML);


//                var aDivs = oDoc.DocumentNode
//                    .Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.StartsWith("row align-items-center"))
//                    .ToList();

//                var aSwapRoute = oDoc.DocumentNode.Descendants("ul").Where(x => x.Attributes.Contains("id") && x.Attributes["id"].Value == "wrapperContent")
//                    .ElementAtOrDefault(1);
//                if (aSwapRoute == null) // happens when the transaction fails
//                {
//                    oTrades.Add(new GlobalTrade("F", "F", 1, "F_F", ExchangeName)
//                    {
//                        InternalExchangeId = oTrade.InternalExchangeId,
//                        dtTrade = oTrade.dtTrade
//                    }.SetQuantities(1, 1));

//                    continue;
//                }

//                var tokenFrom = aSwapRoute
//                    .Descendants("li").FirstOrDefault()
//                    .Descendants("a").LatorDefault(x => x.Attributes.Contains("href") && x.Attributes["href"].Value?.StartsWith("/token/") == true);
//                var tokenTo = aSwapRoute
//                    .Descendants("li").LatorDefault()
//                    .Descendants("a").LatorDefault(x => x.Attributes.Contains("href") && x.Attributes["href"].Value?.StartsWith("/token/") == true);

//                    var from = tokenFrom.InnerText;
//                    var to = tokenTo.InnerText;

//                if (tokenFrom.InnerText == tokenTo.InnerText) // Stack, ignore
//                {

//                    oTrades.Add(new GlobalTrade(from, to, 1, from + "_" + to, ExchangeCode)
//                    {
//                        InternalExchangeId = oTrade.InternalExchangeId,
//                        dtTrade = oTrade.dtTrade

//                    }.SetQuantities(0, 0));
//                    continue;
//                }



//                var tokenIdFrom = tokenFrom.Attributes["href"].Value.Split('/')
//                    .LatorDefault()
//                    .Split('?')
//                    .FirstOrDefault();
//                var tokenIdTo = tokenTo.Attributes["href"].Value.Split('/')
//                    .LatorDefault()
//                    .Split('?')
//                    .FirstOrDefault();

//                if (string.IsNullOrEmpty(tokenIdFrom)) // happens when the transaction fails
//                {
//                    oTrades.Add(new GlobalTrade("F", "F", 1, "F_F", ExchangeCode)
//                    {
//                        InternalExchangeId = oTrade.InternalExchangeId,
//                    }.SetQuantities(1, 1));

//                    continue;
//                }

//                string fromValue = Regex.Replace(aSwapRoute.InnerText, ".*For ([0-9,.]*).*[ ]+" + from.Replace("(", "\\(").Replace(")", "\\)") + ".*", "$1");
//                string toValue = Regex.Replace(aSwapRoute.InnerText, ".*For ([0-9,.]*)[ ]+.*" + to.Replace("(", "\\(").Replace(")", "\\)") + ".*", "$1");

//                double dFrom = fromValue.Replace(",", "").ToDouble();
//                double dTo = toValue.Replace(",", "").ToDouble();

//                string sCryptoFrom = Regex.Replace(from, ".*\\((.*)\\).*", "$1");
//                string sCryptoTo = Regex.Replace(to, ".*\\((.*)\\).*", "$1");

//                var oGlobalTrade = new GlobalTrade(sCryptoFrom, sCryptoTo, dTo / dFrom, sCryptoFrom + "_" + sCryptoTo, ExchangeCode);
//                oGlobalTrade.CryptoFromId = tokenIdFrom;
//                oGlobalTrade.CryptoToId = tokenIdTo;
//                oGlobalTrade.SetQuantities(dFrom, dTo);
//                oGlobalTrade.InternalExchangeId = oTrade.InternalExchangeId;
//                oGlobalTrade.dtTrade = oTrade.dtTrade;
//                oTrades.Add(oGlobalTrade);
//                //Name: "data-toggle", Value: "tooltip"
//            }



//            return oTrades;
//        }

//        public GlobalTrade PlaceMarketOrder(CurrencySymbol symbol, double quantity, SellBuy SellOrBuy, bool bTest)
//        {
//            throw new NotImplementedException();
//        }
//    }


//    public partial class BscTicker
//    {
//        [JsonProperty("data")]
//        public Data Data { get; set; }

//        [JsonProperty("status")]
//        public Status Status { get; set; }
//    }

//    public partial class Data
//    {
//        [JsonProperty("cryptoCurrencyList")]
//        public CryptoCurrencyList[] CryptoCurrencyList { get; set; }

//        [JsonProperty("totalCount")]
//        public string TotalCount { get; set; }
//    }

//    public partial class CryptoCurrencyList
//    {
//        [JsonProperty("id")]
//        public long Id { get; set; }

//        [JsonProperty("name")]
//        public string Name { get; set; }

//        [JsonProperty("symbol")]
//        public string Symbol { get; set; }

//        [JsonProperty("slug")]
//        public string Slug { get; set; }

//        [JsonProperty("tags")]
//        public string[] Tags { get; set; }

//        [JsonProperty("cmcRank")]
//        public long CmcRank { get; set; }

//        [JsonProperty("marketPairCount")]
//        public long MarketPairCount { get; set; }

//        [JsonProperty("circulatingSupply")]
//        public double CirculatingSupply { get; set; }

//        [JsonProperty("totalSupply")]
//        public double TotalSupply { get; set; }

//        [JsonProperty("ath")]
//        public double Ath { get; set; }

//        [JsonProperty("atl")]
//        public double Atl { get; set; }

//        [JsonProperty("high24h")]
//        public double High24H { get; set; }

//        [JsonProperty("low24h")]
//        public double Low24H { get; set; }

//        [JsonProperty("isActive")]
//        public long IsActive { get; set; }

//        [JsonProperty("lastUpdated")]
//        public DateTimeOffset LastUpdated { get; set; }

//        [JsonProperty("dateAdded")]
//        public DateTimeOffset DateAdded { get; set; }

//        [JsonProperty("quotes")]
//        public Quote[] Quotes { get; set; }

//        [JsonProperty("platform", NullValueHandling = NullValueHandling.Ignore)]
//        public Platform Platform { get; set; }

//        [JsonProperty("maxSupply", NullValueHandling = NullValueHandling.Ignore)]
//        public long? MaxSupply { get; set; }
//    }

//    public partial class Platform
//    {
//        [JsonProperty("id")]
//        public long Id { get; set; }

//        [JsonProperty("name")]
//        public string Name { get; set; }

//        [JsonProperty("symbol")]
//        public string Symbol { get; set; }

//        [JsonProperty("slug")]
//        public string Slug { get; set; }

//        [JsonProperty("token_address")]
//        public string TokenAddress { get; set; }
//    }

//    public partial class Quote
//    {
//        [JsonProperty("name")]
//        public string Name { get; set; }

//        [JsonProperty("price")]
//        public double Price { get; set; }

//        [JsonProperty("volume24h")]
//        public double Volume24H { get; set; }

//        [JsonProperty("volume7d")]
//        public double Volume7D { get; set; }

//        [JsonProperty("volume30d")]
//        public double Volume30D { get; set; }

//        [JsonProperty("marketCap")]
//        public double MarketCap { get; set; }

//        [JsonProperty("percentChange1h")]
//        public double PercentChange1H { get; set; }

//        [JsonProperty("percentChange24h")]
//        public double PercentChange24H { get; set; }

//        [JsonProperty("percentChange7d")]
//        public double PercentChange7D { get; set; }

//        [JsonProperty("lastUpdated")]
//        public DateTimeOffset LastUpdated { get; set; }

//        [JsonProperty("percentChange30d")]
//        public double PercentChange30D { get; set; }

//        [JsonProperty("percentChange60d")]
//        public double PercentChange60D { get; set; }

//        [JsonProperty("percentChange90d")]
//        public double PercentChange90D { get; set; }

//        [JsonProperty("fullyDilluttedMarketCap")]
//        public double FullyDilluttedMarketCap { get; set; }

//        [JsonProperty("dominance")]
//        public double Dominance { get; set; }

//        [JsonProperty("turnover")]
//        public double Turnover { get; set; }

//        [JsonProperty("ytdPriceChangePercentage")]
//        public double YtdPriceChangePercentage { get; set; }

//        [JsonProperty("tvl", NullValueHandling = NullValueHandling.Ignore)]
//        public double? Tvl { get; set; }
//    }

//    public partial class Status
//    {
//        [JsonProperty("timestamp")]
//        public DateTimeOffset Timestamp { get; set; }

//        [JsonProperty("error_code")]
//        public string ErrorCode { get; set; }

//        [JsonProperty("error_message")]
//        public string ErrorMessage { get; set; }

//        [JsonProperty("elapsed")]
//        public string Elapsed { get; set; }

//        [JsonProperty("credit_count")]
//        public long CreditCount { get; set; }
//    }

//    public enum Name { Btc, Eth, Usd };

//}
