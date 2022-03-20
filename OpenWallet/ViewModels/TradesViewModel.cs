using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using OpenWallet.Models;
using OpenWallet.Views;
using OpentWallet.Logic;
using OpenWallet.Common;
using System.Linq;
using System.Collections.Generic;

namespace OpenWallet.ViewModels
{
    public class GlobalTradeViewModel
    {
        public GlobalTrade Trade { get; set; }
        public double Delta { get; set; }
        public bool IsProfitable { get; set; }
        public System.Drawing.Color SellStateBackColor { get; set; }
        public System.Drawing.Color SellStateBackColorSelected { get; set; }
        public System.Drawing.Color ArchiveStateForeColor { get; set; }
        public System.Drawing.Color DeltaColor { get; set; }
        public System.Drawing.Color DeltaColorSelected { get; set; }
        public GlobalTradeViewModel(GlobalTrade trade, bool isArchived)
        {
            Trade = trade;

            Delta = TradeService.GetDelta(trade, WalletViewModel.allCurrencies);
            IsProfitable = TradeService.IsProfitable(trade, Delta);

            SellStateBackColor = TradeService.GetSellStateBackColor(trade);
            SellStateBackColorSelected = TradeService.GetSellStateBackColorSelected(trade);
            ArchiveStateForeColor = TradeService.GetArchiveStateForeColor(isArchived);

            DeltaColor = TradeService.GetDeltaColor(IsProfitable);
            DeltaColorSelected = TradeService.GetDeltaColorSelected(IsProfitable);
        }
    }

    public class TradesViewModel : BaseViewModel
    {
        private Item _selectedItem;

        public ObservableCollection<GlobalTradeViewModel> trades { get; }
        public Command LoadItemsCommand { get; }
        public Command RefreshTrades { get; }
        public Command<Item> ItemTapped { get; }

        public TradesViewModel()
        {
            Title = "Browse";
            trades = new ObservableCollection<GlobalTradeViewModel>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            ItemTapped = new Command<Item>(OnItemSelected);

            RefreshTrades = new Command(OnRefreshTrades);



        }
        Dictionary<string, List<GlobalTrade>> archiveTrades;

        private async void RefreshTradesUI(Func<Task<List<GlobalTrade>>> actionInvokeTradeLoad)
        {

            IsBusy = true;

            try
            {
                trades.Clear();

                List<GlobalTrade> tradesTemp = await actionInvokeTradeLoad.Invoke();
                archiveTrades = ConfigService.LoadArchiveTradeFromCache();

                foreach (GlobalTrade trade in tradesTemp)
                {

                    bool tradeIsArchived = archiveTrades.GetOrDefault(trade.CustomCouple)?.Any(g => g.InternalExchangeId == trade.InternalExchangeId) == true;

                    if (tradeIsArchived && true)
                        continue;

                    if (trade.From == trade.To)
                        continue;

                    trades.Add(new GlobalTradeViewModel(trade, tradeIsArchived));

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
            RefreshTradesUI(() => Task.FromResult(TradeService.LoadTradesFromCacheOnly(WalletViewModel.exchanges, WalletViewModel.allCurrencies)));
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

            RefreshTradesUI(() => TradeService.LoadTrades(WalletViewModel.exchanges, WalletViewModel.balances, WalletViewModel.allCurrencies));
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