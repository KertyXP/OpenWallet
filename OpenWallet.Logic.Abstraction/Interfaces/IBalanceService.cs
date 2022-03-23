using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenWallet.Logic.Abstraction.Interfaces
{
    public interface IBalanceService
    {

        Task<List<CurrencySymbolPrice>> GetCurrencriesAsync(List<IExchange> aExchanges);

        List<CurrencySymbolPrice> LoadFiatisation(List<CurrencySymbolPrice> aAllCurrencies);

        void SetBitcoinFavCryptoValue(IExchange exchange, List<CurrencySymbolPrice> aAllCurrencies, IEnumerable<GlobalBalance> aAllBalances);
        void SetBitcoinFavCryptoValue(IExchange exchange, List<CurrencySymbolPrice> aAllCurrencies, GlobalBalance oBalance);

        Task<List<GlobalBalance>> GetBalancesAsync(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies);

        List<GlobalBalance> LoadBalancesFromCacheOnly(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies);
    }
}
