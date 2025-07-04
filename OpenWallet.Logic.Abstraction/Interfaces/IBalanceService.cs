﻿using OpenWallet.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWallet.Logic.Abstraction.Interfaces
{
    public interface IBalanceService
    {
        List<CurrencySymbolPrice> GetCurrencies();

        Task LoadCurrencriesAsync(List<IExchange> aExchanges);

        List<CurrencySymbolPrice> LoadFiatisation(List<CurrencySymbolPrice> aAllCurrencies);

        void SetBitcoinFavCryptoValue(IExchange exchange, List<CurrencySymbolPrice> aAllCurrencies, IEnumerable<GlobalBalance> aAllBalances);
        void SetBitcoinFavCryptoValue(IExchange exchange, List<CurrencySymbolPrice> aAllCurrencies, GlobalBalance oBalance);

        Task<List<GlobalBalance>> GetBalancesAsync(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies);

        List<GlobalBalance> LoadBalancefromCacheOnly(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies);
    }
}
