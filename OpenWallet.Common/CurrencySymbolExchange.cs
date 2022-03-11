namespace OpenWallet.Common
{
    public class CurrencySymbolExchange : CurrencySymbol
    {
        public CurrencySymbolExchange()
        {

        }
        public CurrencySymbolExchange(string from, string to, string couple, string exchange) : base(from, to, couple)
        {
            Exchange = exchange;
        }

        public string Exchange { get; set; }


    }
}
