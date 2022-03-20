using Xamarin.Forms;
using OpenWallet.ViewModels;

namespace OpenWallet.Views
{
    public partial class TradesPage : ContentPage
    {
        TradesViewModel _viewModel;

        public TradesPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new TradesViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}