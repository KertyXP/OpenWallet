using Xamarin.Forms;

using OpenWallet.Models;
using OpenWallet.ViewModels;

namespace OpenWallet.Views
{
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = new NewItemViewModel();
        }
    }
}