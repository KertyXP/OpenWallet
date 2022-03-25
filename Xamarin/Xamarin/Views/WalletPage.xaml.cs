using OpenWallet.Common;
using Unity;
using Xamarin.Forms;
using Xamarin.ViewModels;

namespace Xamarin.Views
{
    public partial class WalletPage : ContentPage
    {
        WalletViewModel _viewModel;


        public WalletPage()
        {
            var a = new GlobalBalance();
            InitializeComponent();

            BindingContext = _viewModel = App.UnityContainer.Resolve<WalletViewModel>();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}