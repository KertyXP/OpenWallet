﻿namespace OpenWallet.Common
{

    public class CurrencySymbolPrice : CurrencySymbolExchange
    {
        public CurrencySymbolPrice(string from, string to, double realPrice, string couple, string exchange) : base(from, to, couple, exchange)
        {
            RealPrice = realPrice;
            Price = couple.StartsWith(from) ? realPrice : 1 / realPrice;
            IsBuy = couple.StartsWith(to);
        }

        public double Price { get; set; }
        public double RealPrice { get; set; }
        public bool IsBuy { get; set; }


    }

    public class CurrencySymbolPriceReverted : CurrencySymbolPrice
    {
        public CurrencySymbolPriceReverted(string from, string to, double realPrice, string couple, string exchange) : base(to, from, realPrice, couple, exchange)
        {
        }
    }
}
