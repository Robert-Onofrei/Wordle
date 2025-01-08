using Microsoft.Maui.Controls;

namespace Wordle
{
    public partial class TutorialPage : ContentPage
    {
        public TutorialPage()
        {
            InitializeComponent();
        }

        private async void CloseTutorialClicked(object sender, EventArgs e)
        {
            //Closes the tutorial page
            await Navigation.PopModalAsync();
        }
    }
}
