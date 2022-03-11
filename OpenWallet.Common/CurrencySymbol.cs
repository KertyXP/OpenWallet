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
        }



        public string CryptoFromId { get; set; }
        public string CryptoToId { get; set; }
        public string From { get; set; }
        public string To { get; set; }

    }
}
