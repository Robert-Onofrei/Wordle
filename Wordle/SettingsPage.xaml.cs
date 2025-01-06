using Microsoft.Maui.Controls;
using System;

namespace Wordle
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private async void OnBack_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
