﻿using System;

namespace OpentWallet.Logic
{
    public class BinanceCalls
    {
        public enum ECalls { accountV3, myTradesV3, ExchangeInfoV3, Time, tickerPriceV3, allPairs, earnings, lendingProjectList, placeOrder, placeOrderTest, GetKLines }
        public ECalls eCall { get; set; }
        public string Api { get; set; }
        public int Weight { get; set; }
        public DateTime dtCall { get; set; }
        public bool PublicApi { get; set; }
        public bool get { get; set; } = true;
    }
}
