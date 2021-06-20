using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using OpenWallet.Common;
using OpenWallet.ViewModels;

namespace OpenWallet.Views
{
    public partial class WalletPage : ContentPage
    {
        WalletViewModel _viewModel;


        public WalletPage()
        {
            var a = new GlobalBalance();
            InitializeComponent();

            BindingContext = _viewModel = new WalletViewModel();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}