﻿using Xamarin.Forms;
using OpenWallet.Services;
using OpentWallet.Logic;
using Xamarin.Essentials;
using PCLStorage;

namespace OpenWallet
{
    public partial class App : Application
    {

        public App()
        {
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
