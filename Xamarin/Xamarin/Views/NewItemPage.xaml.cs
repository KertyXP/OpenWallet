using Xamarin.Forms;

using Xamarin.Models;
using Xamarin.ViewModels;

namespace Xamarin.Views
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