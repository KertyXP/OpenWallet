using OpentWallet.Logic;
using OpenWallet.Logic.Abstraction.Interfaces;
using System;
using System.Windows.Forms;
using Unity;

namespace OpenWallet.WinForm
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            UnityContainer unityContainer = new UnityContainer();
            unityContainer.RegisterInstance(unityContainer);
            unityContainer.RegisterSingleton<IIocService, IocService>();
            unityContainer.RegisterSingleton<ITradeService, TradeService>();
            unityContainer.RegisterSingleton<IConfigService, ConfigService>();
            unityContainer.RegisterSingleton<IBalanceService, BalanceService>();
            unityContainer.RegisterSingleton<Form1>();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(unityContainer.Resolve<Form1>());
        }
    }
}
