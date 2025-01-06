using Microsoft.Maui.Controls;

namespace Wordle
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Set the starting page of the app to MainPage
            MainPage = new MainPage();
        }
    }
}
