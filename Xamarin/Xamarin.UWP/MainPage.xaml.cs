using OpentWallet.Logic;
using OpenWallet.Logic.Abstraction.Interfaces;
using Unity;
using Windows.Storage;

namespace Xamarin.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            Windows.Storage.StorageFolder installedLocation = ApplicationData.Current.LocalFolder;


            var unityContainer = new UnityContainer();
            unityContainer.RegisterSingleton<IConfigService, ConfigService>();

            unityContainer.RegisterInstance(unityContainer);
            unityContainer.RegisterSingleton<IIocService, IocService>();
            unityContainer.RegisterSingleton<ITradeService, TradeService>();
            unityContainer.RegisterSingleton<IBalanceService, BalanceService>();

            unityContainer.Resolve<IConfigService>().Init(installedLocation.Path);

            LoadApplication(new Xamarin.App(unityContainer));
        }
    }
}
