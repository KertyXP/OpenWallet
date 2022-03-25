using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using OpenWallet.Common;
using System.Linq;
using System.Collections.Generic;
using OpenWallet.Logic.Abstraction.Interfaces;
using Autofac;
using Xamarin.Views;
using Xamarin.Models;

namespace Xamarin.ViewModels
{

    public class TradesViewModel : BaseViewModel
    {
        private Item _selectedItem;

        public ObservableCollection<GlobalTradeViewModel> trades { get; }
        public Command LoadItemsCommand { get; }
        public Command RefreshTrades { get; }
        public Command<Item> ItemTapped { get; }

        private IConfigService _configService { get; }

        public TradesViewModel(IConfigService configService)
        {
            Title = "Browse";
            trades = new ObservableCollection<GlobalTradeViewModel>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            ItemTapped = new Command<Item>(OnItemSelected);

            RefreshTrades = new Command(OnRefreshTrades);
            {
                _configService = configService;
            }


        }
        Dictionary<string, List<GlobalTrade>> archiveTrades;

        private async void RefreshTradesUI(Func<Task<List<GlobalTrade>>> actionInvokeTradeLoad)
        {

            IsBusy = true;

            try
            {
                trades.Clear();

                List<GlobalTrade> tradesTemp = await actionInvokeTradeLoad.Invoke();
                archiveTrades = _configService.LoadArchiveTradeFromCache();

                foreach (GlobalTrade trade in tradesTemp)
                {

                    bool tradeIsArchived = archiveTrades.GetOrDefault(trade.CustomCouple)?.Any(g => g.InternalExchangeId == trade.InternalExchangeId) == true;

                    if (tradeIsArchived && true)
                        continue;

                    if (trade.From == trade.To)
                        continue;

                    //trades.Add(new GlobalTradeViewModel(trade, tradeIsArchived));

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
        async Task ExecuteLoadItemsCommand()
        {
            //RefreshTradesUI(() => Task.FromResult(TradeService.LoadTradesFromCacheOnly(WalletViewModel.exchanges, WalletViewModel.allCurrencies)));
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        private async void OnRefreshTrades(object obj)
        {

            //RefreshTradesUI(() => TradeService.LoadTrades(WalletViewModel.exchanges, WalletViewModel.balances, WalletViewModel.allCurrencies));
        }

        async void OnItemSelected(Item item)
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
        }
    }
}