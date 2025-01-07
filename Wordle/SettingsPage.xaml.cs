using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;

namespace Wordle
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            BindingContext = AppSettings.Instance;
        }

        //Returns to the main page when back is clicked
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
