using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Storage;

namespace Wordle
{
    public partial class MainPage : ContentPage
    {
        //Variables
        private Label[,] letterBoxes = new Label[6, 5];
        private int curRow = 0;
        private int curCol = 0;
        private bool gameOver = false;
        private bool isHardModeOn = false;
        private string secretWord;
        private readonly Random _random = new Random();

        //Defines colour palette pairs
        private Dictionary<string, PaletteColours> palettePairs = new Dictionary<string, PaletteColours>
        {
            {
                //Defualt colour
                "Default", new PaletteColours
                {
                    BackgroundColor = "#121212",
                    KeyBackgroundColor = "#787C7E",
                    KeyCorrectColor = "#6AAA64",
                    KeyAlmostColor = "#C9B458",
                    KeyWrongColor = "#3b3b3b"
                }
            },
            {
                //Pastel colour
                "Pastel", new PaletteColours
                {
                    BackgroundColor = "#F7E1D7",
                    KeyBackgroundColor = "#C9CBA3",
                    KeyCorrectColor = "#6AAA64",
                    KeyAlmostColor = "#C9B458",
                    KeyWrongColor = "#A3A3A3"
                }
            },
            {
                //Synth-wave colour
                "Synth-wave", new PaletteColours
                {
                    BackgroundColor = "#1B1B3A",
                    KeyBackgroundColor = "#6D9DC5",
                    KeyCorrectColor = "#6AAA64",
                    KeyAlmostColor = "#C9B458",
                    KeyWrongColor = "#3b3b3b"
                }
            }
        };

        //Tracks letter that must be in a specific place, eg: Green letters
        private Dictionary<int, char> letterByPos = new Dictionary<int, char>();

        //Tracks letters that must be in the guess, eg: Yellow letters
        private HashSet<char> reqLetters = new HashSet<char>();

        public MainPage()
        {
            InitializeComponent();

            //Creates letter box array 
            for (int row = 0; row < 6; row++)
                for (int col = 0; col < 5; col++)
                    letterBoxes[row, col] = (Label)FindByName($"R{row}C{col}");

            //Gets notified when the settings change
            AppSettings.Instance.PropertyChanged += AppSettingsChanged;

            //Default settings
            ApplySettings();
        }

        private void GetWord(List<string> wordList)
        {
            if (wordList == null || wordList.Count == 0)
                throw new InvalidOperationException("Word list is empty or not initialized.");

            //Generates a random index within the bounds of the word list
            int randomIndex = _random.Next(wordList.Count);

            //Assigns the randomly selected word to secretWord
            secretWord = wordList[randomIndex].ToUpper();
        }

        //Indicates if WordAPI has already been initialized
        private bool _hasInitialized = false;

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //Only initialize WordAPI once
            if (!_hasInitialized)
            {
                await WordAPI.InitializeAsync();
                _hasInitialized = true;
            }

            //Commenting out the second initialization to avoid re-initializing on each return
            //await WordAPI.InitializeAsync(); //Removed to prevent multiple inits

            if (!Preferences.Get("HasOpenedTutorial", false))
            {
                Preferences.Set("HasOpenedTutorial", true);
                await Navigation.PushModalAsync(new TutorialPage());
            }

            //Retrieves the saved secret word if it exists
            string savedSecretWord = Preferences.Get("SavedSecretWord", string.Empty);

            if (!string.IsNullOrEmpty(savedSecretWord))
            {
                //Gets the saved secret word
                secretWord = savedSecretWord;

                //gets the saved current row and column
                curRow = Preferences.Get("SavedCurRow", 0);
                curCol = Preferences.Get("SavedCurCol", 0);

                //gets the saved game state
                gameOver = Preferences.Get("SavedGameOver", false);
                isHardModeOn = Preferences.Get("SavedHardMode", false);

                //Recreates the letter boxes
                for (int row = 0; row < 6; row++)
                {
                    string rowString = Preferences.Get($"SavedRow{row}", "_____");

                    for (int col = 0; col < 5; col++)
                    {
                        char letterChar = rowString[col];
                        if (letterChar == '_')
                            letterBoxes[row, col].Text = "";
                        else
                            letterBoxes[row, col].Text = letterChar.ToString();
                    }
                }

                //Re-applies colors only on fully guessed rows
                ReApplyColorsUpToCurrentRow();
            }
            else
            {
                //If there's NO saved puzzle, ensure we have a secretWord
                List<string> wordList = WordAPI.GetWords();
                if (wordList != null && wordList.Count > 0)
                {
                    GetWord(wordList);
                }
                else
                {
                    //Word list failed to load
                    await DisplayAlert("Error", "Failed to load words. Please try again later.", "OK");
                }
            }
        }

        private void ReApplyColorsUpToCurrentRow()
        {
            for (int row = 0; row < curRow; row++)
            {
                //Only color if the row is fully typed
                bool rowIsFull = true;
                string rowGuess = "";

                for (int col = 0; col < 5; col++)
                {
                    string letter = letterBoxes[row, col].Text;
                    if (string.IsNullOrEmpty(letter))
                    {
                        //If we find an empty letter, skip re-color
                        rowIsFull = false;
                        break;
                    }
                    rowGuess += letter;
                }

                if (rowIsFull)
                {
                    //Temporarily override curRow to re-color that row
                    int originalCurRow = curRow;
                    curRow = row;
                    ColorBoxes(rowGuess.ToUpper());
                    curRow = originalCurRow;
                }
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            //Saves the secret word
            Preferences.Set("SavedSecretWord", secretWord);

            //Saves current row/col
            Preferences.Set("SavedCurRow", curRow);
            Preferences.Set("SavedCurCol", curCol);

            //Saves gameOver + isHardModeOn
            Preferences.Set("SavedGameOver", gameOver);
            Preferences.Set("SavedHardMode", isHardModeOn);

            //Save the letters on the board
            //For each row we build a 5-letter string.
            for (int row = 0; row < 6; row++)
            {
                string rowString = "";
                for (int col = 0; col < 5; col++)
                {
                    string letter = letterBoxes[row, col].Text;
                    if (string.IsNullOrEmpty(letter))
                    {
                        letter = "_";
                    }

                    rowString += letter;
                }
                Preferences.Set($"SavedRow{row}", rowString);
            }
        }

        private void AppSettingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplySettings();
        }

        private void ApplySettings()
        {
            ApplyColorPalette(AppSettings.Instance.ColourPalette);
            ChangeKeyboardVisiblity(AppSettings.Instance.ScreenKeyboard);
            SetHardMode(AppSettings.Instance.IsHardModeOn);
            SetFontSize(AppSettings.Instance.FontSize);
        }

        private void ApplyColorPalette(string palette)
        {
            if (palettePairs.ContainsKey(palette))
            {
                var colors = palettePairs[palette];
                //Update Background Color
                this.BackgroundColor = Color.FromHex(colors.BackgroundColor);

                //Update BoxBackgroundColour Dynamic Resource
                Application.Current.Resources["BoxBackgroundColour"] = Color.FromHex(colors.KeyBackgroundColor);

                //Reset letter boxes that haven't been guessed yet
                for (int row = 0; row < 6; row++)
                {
                    for (int col = 0; col < 5; col++)
                    {
                        if (string.IsNullOrEmpty(letterBoxes[row, col].Text))
                        {
                            //Set to default letter box color
                            letterBoxes[row, col].BackgroundColor = Color.FromHex(colors.KeyBackgroundColor);
                        }
                    }
                }
            }
        }

        private void OnNewGameClicked(object sender, EventArgs e)
        {
            //When new game is clicked
            StartNewGame();
        }


        //Turns the keyboard on or off 
        public void ChangeKeyboardVisiblity(bool isVisible)
        {
            KeyboardLayout.IsVisible = isVisible;
        }

        //Sets the hard mode
        public void SetHardMode(bool hardMode)
        {
            isHardModeOn = hardMode;
        }

        //Sets the font size
        public void SetFontSize(double fontSize)
        {
            foreach (Label label in letterBoxes)
            {
                label.FontSize = fontSize;
            }

            foreach (var child in KeyboardLayout.Children)
            {
                if (child is Button button)
                {
                    button.FontSize = fontSize;
                }
            }
        }

        //When letter is clicked
        private void OnLetterClicked(object sender, EventArgs e)
        {
            if (gameOver) return;

            var button = sender as Button;
            if (button == null) return;

            //Only adds letter if empty
            if (curCol < 5)
            {
                var label = letterBoxes[curRow, curCol];

                label.Opacity = 0;
                label.Text = button.Text.ToUpper();

                label.FadeTo(1, 250);

                curCol++;
            }
        }

        //When backspace is clicked
        private void OnBackspaceClicked(object sender, EventArgs e)
        {
            if (gameOver) return;

            //If there's at least one letter typed in the current row
            if (curCol > 0)
            {
                curCol--;
                letterBoxes[curRow, curCol].Text = "";

                //Resets the background color if not green or yellow
                string palette = AppSettings.Instance.ColourPalette;
                var colors = palettePairs[palette];
                letterBoxes[curRow, curCol].BackgroundColor = Color.FromHex(colors.KeyBackgroundColor);
            }
        }

        //When enter is clicked
        private void OnEnterClicked(object sender, EventArgs e)
        {
            if (gameOver) return;

            //Proceeds if row is full
            if (curCol != 5)
            {
                Message.Text = "Not enough letters!";
                return;
            }

            //Builds the guess string
            string guess = "";
            for (int c = 0; c < 5; c++)
            {
                guess += letterBoxes[curRow, c].Text;
            }

            //Ensures cosistency
            guess = guess.ToUpper();

            //Checks if the guess is in the word list
            if (!WordAPI.GetWords().Contains(guess))
            {
                Message.Text = "That's not in the word list!";
                return;
            }

            //Checks constraints if hard mode is on
            if (isHardModeOn)
            {
                //Checks required letters by position eg: green
                foreach (var kvp in letterByPos)
                {
                    int position = kvp.Key;
                    char requiredChar = kvp.Value;
                    if (guess[position] != requiredChar)
                    {
                        Message.Text = $"Letter '{requiredChar}' must be in position {position + 1}.";
                        return;
                    }
                }

                //Checks required letters eg: yellow
                foreach (char requiredChar in reqLetters)
                {
                    if (!guess.Contains(requiredChar.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        Message.Text = $"Guess must contain the letter '{requiredChar}'.";
                        return;
                    }
                }
            }

            //Compares the guess to the secret word and then colours the boxes
            ColorBoxes(guess.ToUpper());

            //Checks for win
            if (guess.ToUpper() == secretWord)
            {
                Message.Text = $"You guessed it! The word was {secretWord}.";
                gameOver = true;

                //Prompts to start a new game
                PromptNewGame();
                return;
            }

            //Moves to next row
            curRow++;
            curCol = 0;

            //If guessed 6 times, game over
            if (curRow == 6)
            {
                Message.Text = $"No more tries! The word was {secretWord}.";
                gameOver = true;

                //Prompts to start a new game
                PromptNewGame();
            }
        }

        //Colours the boxes based on the guess
        private void ColorBoxes(string guess)
        {
            string palette = AppSettings.Instance.ColourPalette;
            var colors = palettePairs[palette];

            //Builds dictionary of letter counts for the guess word
            Dictionary<char, int> letterCounts = new Dictionary<char, int>();
            foreach (char ch in secretWord)
            {
                if (!letterCounts.ContainsKey(ch))
                    letterCounts[ch] = 0;
                letterCounts[ch]++;
            }

            //First checks correct letters
            for (int i = 0; i < 5; i++)
            {
                char guessChar = guess[i];
                char secretChar = secretWord[i];
                if (guessChar == secretChar)
                {
                    letterBoxes[curRow, i].BackgroundColor = Color.FromHex(colors.KeyCorrectColor);
                    letterCounts[guessChar]--;

                    letterBoxes[curRow, i].Opacity = 0;
                    letterBoxes[curRow, i].FadeTo(1, 300);

                    //Updates constraints
                    if (isHardModeOn)
                    {
                        if (!letterByPos.ContainsKey(i))
                            letterByPos.Add(i, guessChar);
                    }
                }
            }

            //Second checks almost correct letters
            for (int i = 0; i < 5; i++)
            {
                //If already coloured correctly skips
                if (letterBoxes[curRow, i].BackgroundColor.Equals(Color.FromHex(colors.KeyCorrectColor)))
                {
                    continue;
                }

                char guessChar = guess[i];
                if (letterCounts.ContainsKey(guessChar) && letterCounts[guessChar] > 0)
                {
                    letterBoxes[curRow, i].BackgroundColor = Color.FromHex(colors.KeyAlmostColor);
                    letterCounts[guessChar]--;

                    //Updates constraints
                    if (isHardModeOn)
                    {
                        reqLetters.Add(guessChar);
                    }

                    letterBoxes[curRow, i].Opacity = 0;
                    letterBoxes[curRow, i].FadeTo(1, 300);
                }
                else
                {
                    letterBoxes[curRow, i].BackgroundColor = Color.FromHex(colors.KeyWrongColor);

                    letterBoxes[curRow, i].Opacity = 0;
                    letterBoxes[curRow, i].FadeTo(1, 300);
                }
            }
        }

        //When settings is clicked opens settings page
        private async void OpenSettings(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new SettingsPage());
        }

        //Resets constraints
        private void ResetConstraints()
        {
            letterByPos.Clear();
            reqLetters.Clear();
        }

        //Starts a new game
        private void StartNewGame()
        {
            //Resets game state
            gameOver = false;
            curRow = 0;
            curCol = 0;
            Message.Text = "";

            //Resets constraints
            ResetConstraints();

            //Clears letter boxes
            for (int row = 0; row < 6; row++)
                for (int col = 0; col < 5; col++)
                {
                    letterBoxes[row, col].Text = "";
                    string palette = AppSettings.Instance.ColourPalette;
                    var colors = palettePairs[palette];
                    letterBoxes[row, col].BackgroundColor = Color.FromHex(colors.KeyBackgroundColor);
                }

            //Select a new secret word for the new game
            List<string> wordList = WordAPI.GetWords();
            if (wordList != null && wordList.Count > 0)
            {
                GetWord(wordList);
            }
            else
            {
                //Handle the scenario where the word list is empty or failed to load
                DisplayAlert("Error", "Word list is empty. Cannot start a new game.", "OK");
                gameOver = true;
            }
        }

        //Prompts to start a new game
        private async void PromptNewGame()
        {
            bool restart = await DisplayAlert("Game Over", "Do you want to play again?", "Yes", "No");
            if (restart)
            {
                StartNewGame();
            }
            else
            {
                gameOver = true;
            }
        }

        private async void OpenTutorial(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new TutorialPage());
        }
    }

    //Helper class that stores palette colours
    public class PaletteColours
    {
        public string BackgroundColor { get; set; }
        public string KeyBackgroundColor { get; set; }
        public string KeyCorrectColor { get; set; }
        public string KeyAlmostColor { get; set; }
        public string KeyWrongColor { get; set; }
    }
}
