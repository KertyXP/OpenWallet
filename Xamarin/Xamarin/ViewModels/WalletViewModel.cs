using Autofac;
using OpenWallet.Common;
using OpenWallet.Logic.Abstraction;
using OpenWallet.Logic.Abstraction.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Xamarin.ViewModels
{
    public class WalletViewModel : BaseViewModel
    {
        public static List<IExchange> exchanges;

        public static List<CurrencySymbolPrice> allCurrencies;

        public static List<GlobalBalance> balances;

        public Command LoadItemsCommand { get; }
        public ObservableCollection<GlobalBalance> aBalances { get; }
        private IConfigService _configService { get; }
        private IBalanceService _balanceService { get; }
        public string Test => "test";
        public WalletViewModel(IConfigService configService, IBalanceService balanceService)
        {

                _configService = configService;
                _balanceService = balanceService;

            aBalances = new ObservableCollection<GlobalBalance>();
            Title = "Wallet";
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;
            var status = await Permissions.RequestAsync<Permissions.StorageRead>();
            status = await Permissions.RequestAsync<Permissions.StorageWrite>();

            while (true)
            {

                status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                if (status == PermissionStatus.Granted)
                {

                    status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                    if (status == PermissionStatus.Granted)
                    {
                        break;
                    }
                }
                await Task.Delay(250);
            }

            exchanges = await _configService.LoadExchangesAsync();

            allCurrencies = await _balanceService.GetCurrencriesAsync(exchanges);

            balances = _balanceService.LoadBalancesFromCacheOnly(exchanges, allCurrencies);

            aBalances.Clear();
            balances.OrderByDescending(b => b.FavCryptoValue).ForEach(aBalances.Add);
            IsBusy = false;
        }
    }
}