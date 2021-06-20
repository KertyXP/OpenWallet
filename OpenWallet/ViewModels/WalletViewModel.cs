using OpentWallet.Logic;
using OpenWallet.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace OpenWallet.ViewModels
{
    public class WalletViewModel : BaseViewModel
    {
        public Command LoadItemsCommand { get; }
        public ObservableCollection<GlobalBalance> aBalances { get; }
        public string Test => "test";
        public WalletViewModel()
        {
            aBalances = new ObservableCollection<GlobalBalance>();
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamain-quickstart"));
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            Task.Run(async () =>
            {


            });
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

            var aExchanges = Config.LoadExchanges();

            List<CurrencySymbolPrice> aAllCurrencies = Config.GetCurrencries(aExchanges);

            List<GlobalBalance> aAll = await Config.GetBalances(aExchanges, aAllCurrencies);

            aBalances.Clear();
            aAll.ForEach(aBalances.Add);
            IsBusy = false;
        }
        public ICommand OpenWebCommand { get; }
    }
}