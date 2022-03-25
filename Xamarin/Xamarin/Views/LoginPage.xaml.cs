using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.ViewModels;

namespace Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            this.BindingContext = new LoginViewModel();
        }
    }
}