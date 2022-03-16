namespace OpenWallet.Common
{
    public class CurrencySymbol
    {
        public string Couple { get; internal set; }
        public string CustomCouple => Couple.StartsWith(From) ? From + "-" + To : To + "-" + From;

        public CurrencySymbol()
        {

        }
        public CurrencySymbol(string from, string to, string couple)
        {
            From = from;
            To = to;
            Couple = couple;

            RealFrom = couple.StartsWith(from) ? from : to;
            RealTo = couple.StartsWith(to) ? from : to;
        }



        public string RealFrom { get; set; }
        public string RealTo { get; set; }
        public string CryptoFromId { get; set; }
        public string CryptoToId { get; set; }
        public string From { get; set; }
        public string To { get; set; }

    }
}
