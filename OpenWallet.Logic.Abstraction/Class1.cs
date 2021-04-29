using OpenWallet.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWallet.Logic.Abstraction
{
    public interface IExchange
    {
        Task<List<GlobalBalance>> GetBalance();
        void Init(ExchangeConfig oConfig);
    }
}
