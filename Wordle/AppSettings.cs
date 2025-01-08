using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Wordle
{
    public class AppSettings : INotifyPropertyChanged
    {
        //Singleton useful for sharing data between different pages
        private static AppSettings instance;

        //Constructor
        public static AppSettings Instance => instance ??= new AppSettings();

        //Variables
        private string colourPalette = "Default";
        private bool screenKeyboard = true;
        private bool isHardModeOn = false;
        private double fontSize = 19;

        //Event Handler
        public event PropertyChangedEventHandler PropertyChanged;

        //Getters and Setters
        public string ColourPalette
        {
            get => colourPalette;
            set
            {
                if (colourPalette != value)
                {
                    colourPalette = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ScreenKeyboard
        {
            get => screenKeyboard;
            set
            {
                if (screenKeyboard != value)
                {
                    screenKeyboard = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsHardModeOn
        {
            get => isHardModeOn;
            set
            {
                if (isHardModeOn != value)
                {
                    isHardModeOn = value;
                    OnPropertyChanged();
                }
            }
        }

        public double FontSize
        {
            get => fontSize;
            set
            {
                if (fontSize != value)
                {
                    fontSize = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
