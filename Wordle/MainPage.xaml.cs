using Microsoft.Maui.Controls;

namespace Wordle
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

        }

        private void OnLetterClicked(object sender, EventArgs e)
        {
            //This I will handle the click event of the letter buttons

        }

        private void OnEnterClicked(object sender, EventArgs e)
        {
            //Checks Guesses, and updates the boxes with colour
        }
    }
}
