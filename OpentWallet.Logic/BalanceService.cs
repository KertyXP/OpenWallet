using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using OpenWallet.Logic.Abstraction.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpentWallet.Logic
{
    public class BalanceService : IBalanceService
    {
        private IConfigService _configService { get; }
        private List<CurrencySymbolPrice> _currencies = new List<CurrencySymbolPrice>();

        public BalanceService(IConfigService configService)
        {
            _configService = configService;
        }

        public async Task LoadCurrencriesAsync(List<IExchange> aExchanges)
        {

            var tasks = aExchanges
                .Select(oExchange => oExchange.GetCurrenciesAsync());

            var result = await Task.WhenAll(tasks);

            _currencies = result
                .SelectMany(r => r)
                .ToList();
        }

        public List<CurrencySymbolPrice> GetCurrencies()
        {
            return _currencies;
        }
        public List<CurrencySymbolPrice> LoadFiatisation(List<CurrencySymbolPrice> aAllCurrencies)
        {

            List<CurrencySymbolPrice> aFiatisation = new List<CurrencySymbolPrice>();
            // fiatisation (needs at least 2 fiats Moneys (first is the fiatisation TO)
            if (_configService.oGlobalConfig.FiatMoneys.Count() > 1)
            {
                string to = _configService.oGlobalConfig.FiatMoneys.FirstOrDefault();
                aFiatisation = _configService.oGlobalConfig.FiatMoneys.Skip(1).Select(fiat =>
                {
                    return new CurrencySymbolPrice(fiat, to, aAllCurrencies.GetCutomPrice(fiat, to), string.Empty, string.Empty);
                }).ToList();
            }

            return aFiatisation;
        }

        public void SetBitcoinFavCryptoValue(IExchange exchange, List<CurrencySymbolPrice> aAllCurrencies, IEnumerable<GlobalBalance> aAllBalances)
        {

            foreach (var oBalance in aAllBalances)
            {
                SetBitcoinFavCryptoValue(exchange, aAllCurrencies, oBalance);
            }

            return;
        }
        public void SetBitcoinFavCryptoValue(IExchange exchange, List<CurrencySymbolPrice> aAllCurrencies, GlobalBalance oBalance)
        {

            if (exchange.oConfig.CurrencietoIgnore?.Any(c => c == oBalance.Crypto) == true)
                return;

            oBalance.FavCrypto = _configService.oGlobalConfig.FavoriteCurrency;
            if (oBalance.Exchange == "BSC")
            {
                //oBalance.BitCoinValue = aAllCurrencies.GetBtcValue(oBalance);
                oBalance.FavCryptoValue = aAllCurrencies.GetCutomValueFromBtc(oBalance, oBalance.FavCrypto);
            }
            else
            {
                oBalance.BitCoinValue = aAllCurrencies.GetBtcValue(oBalance);
                oBalance.FavCryptoValue = aAllCurrencies.GetCutomValue(oBalance, oBalance.FavCrypto);
            }

            return;
        }

        public async Task<List<GlobalBalance>> GetBalancesAsync(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies)
        {

            var tasks = aExchanges.Select(async exchange =>
            {
                var balance = (await exchange.GetBalanceAsync())
                .Where(b => exchange.oConfig.CurrencietoIgnore?.Any(c => c == b.Crypto) != true)
                .ForEach(balance =>
                {
                    SetBitcoinFavCryptoValue(exchange, aAllCurrencies, balance);
                })
                .ToList();

                _configService.SaveBalanceToCache(exchange, balance);

                return balance;
            });

            var result = await Task.WhenAll(tasks);

            return result
                .SelectMany(r => r)
                .ToList();
        }

        public List<GlobalBalance> LoadBalancefromCacheOnly(List<IExchange> aExchanges, List<CurrencySymbolPrice> aAllCurrencies)
        {

            var aBalance = aExchanges.Select(oExchange =>
            {
                var balance = _configService.LoadBalanceFromCache(oExchange);
                SetBitcoinFavCryptoValue(oExchange, aAllCurrencies, balance);

                return balance;
            })
                .SelectMany(b => b)
                .ToList();

            return aBalance;
        }
    }
}
