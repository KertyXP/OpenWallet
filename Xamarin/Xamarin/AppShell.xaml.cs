using Xamarin.Forms;
using Xamarin.Views;

namespace Xamarin
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(WalletPage), typeof(WalletPage));
            Routing.RegisterRoute(nameof(TradesPage), typeof(TradesPage));
        }

    }
}
