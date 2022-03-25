using OpentWallet.Logic;
using OpenWallet.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace OpenWallet.ViewModels
{
    public class WalletViewModel : BaseViewModel
    {
        public static List<Logic.Abstraction.IExchange> exchanges;

        public static List<CurrencySymbolPrice> allCurrencies;

        public static List<GlobalBalance> balances;

        public Command LoadItemsCommand { get; }
        public ObservableCollection<GlobalBalance> aBalances { get; }
        public string Test => "test";
        public WalletViewModel()
        {
            aBalances = new ObservableCollection<GlobalBalance>();
            Title = "About";
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

            //exchanges = await ConfigService.LoadExchangesAsync();

            //allCurrencies = await BalanceService.GetCurrencriesAsync(exchanges);

            //balances = BalanceService.LoadBalancesFromCacheOnly(exchanges, allCurrencies);

            //aBalances.Clear();
            //balances.OrderByDescending(b => b.FavCryptoValue).ForEach(aBalances.Add);
            //IsBusy = false;
        }
    }
}