using System;
using System.Collections.Generic;
using System.Text;

namespace OpenWallet.Common
{
    public class GlobalBalance
    {
        public string Exchange { get; set; }
        public string Crypto { get; set; }
        public double Value { get; set; }
        public double BitCoinValue { get; set; }
        public string FavCrypto { get; set; }
        public double FavCryptoValue { get; set; }
    }

}
