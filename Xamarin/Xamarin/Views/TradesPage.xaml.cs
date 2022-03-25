using Unity;
using Xamarin.Forms;
using Xamarin.ViewModels;

namespace Xamarin.Views
{
    public partial class TradesPage : ContentPage
    {
        TradesViewModel _viewModel;

        public TradesPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = App.UnityContainer.Resolve<TradesViewModel>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}