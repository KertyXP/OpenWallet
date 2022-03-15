using OpentWallet.Logic;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {


#if WINDOWS
            string path = Microsoft.Maui.Essentials.FileSystem.AppDataDirectory + "\\config.json";
            string sjson = File.ReadAllText(path);
            ConfigService.Init(path);
#else
            var stream = await Microsoft.Maui.Essentials.FileSystem.OpenAppPackageFileAsync(filePath);
#endif


            count++;
            CounterLabel.Text = $"Current count: {count}";

            SemanticScreenReader.Announce(CounterLabel.Text);
        }
    }
}