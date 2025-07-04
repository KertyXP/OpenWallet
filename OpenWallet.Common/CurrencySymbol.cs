﻿namespace OpenWallet.Common
{
    public class CurrencySymbol
    {
        public string Couple { get; internal set; }
        public string CutomCouple => Couple.StartsWith(From) ? From + "-" + To : To + "-" + From;

        public CurrencySymbol()
        {

        }
        public CurrencySymbol(string from, string to, string couple)
        {
            Couple = couple;
            _from = from;
            _to = to;
            if(couple.StartsWith(From) || couple.EndsWith(To)) 
            {
                _realFrom = from;
                _realTo = to;
            }
            else
            {
                _realFrom = to;
                _realTo = from;
            }
        }


        private string _realFrom;
        private string _realTo;

        public string RealFrom => _realFrom;
        public string RealTo => _realTo;
        private string _from;
        public string From { get => _from; set 
            {
                _from = value;
                _realFrom = Couple.StartsWith(_from) ? _from : _to;
            }}
        private string _to;
        public string To
        {
            get => _to; set
            {
                _to = value;
                _realTo = Couple.StartsWith(_to) ? _from : _to;
            }
        }

    }
}
