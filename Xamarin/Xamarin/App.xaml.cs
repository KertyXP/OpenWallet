using OpentWallet.Logic;
using OpenWallet.Logic.Abstraction.Interfaces;
using Unity;
using Xamarin.Forms;
using Xamarin.Services;

namespace Xamarin
{
    public partial class App : Application
    {
        public static UnityContainer UnityContainer;
        public App(UnityContainer unityContainer)
        {
            UnityContainer = unityContainer;

            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
