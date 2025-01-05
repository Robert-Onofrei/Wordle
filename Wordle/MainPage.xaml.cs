using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;

namespace Wordle
{
    public partial class MainPage : ContentPage
    {
        private Label[,] letterSpace = new Label[6, 5];

        private int currentRow = 0;
        private int currentCol = 0; 
        private bool isGameOver = false;

        //Pracitce word for now
        private string secretWord = "MAUIX";

        public MainPage()
        {
            InitializeComponent();

            //This seems very messy, Try to refine
            letterSpace[0, 0] = R0C0; letterSpace[0, 1] = R0C1; letterSpace[0, 2] = R0C2; letterSpace[0, 3] = R0C3; letterSpace[0, 4] = R0C4;
            letterSpace[1, 0] = R1C0; letterSpace[1, 1] = R1C1; letterSpace[1, 2] = R1C2; letterSpace[1, 3] = R1C3; letterSpace[1, 4] = R1C4;
            letterSpace[2, 0] = R2C0; letterSpace[2, 1] = R2C1; letterSpace[2, 2] = R2C2; letterSpace[2, 3] = R2C3; letterSpace[2, 4] = R2C4;
            letterSpace[3, 0] = R3C0; letterSpace[3, 1] = R3C1; letterSpace[3, 2] = R3C2; letterSpace[3, 3] = R3C3; letterSpace[3, 4] = R3C4;
            letterSpace[4, 0] = R4C0; letterSpace[4, 1] = R4C1; letterSpace[4, 2] = R4C2; letterSpace[4, 3] = R4C3; letterSpace[4, 4] = R4C4;
            letterSpace[5, 0] = R5C0; letterSpace[5, 1] = R5C1; letterSpace[5, 2] = R5C2; letterSpace[5, 3] = R5C3; letterSpace[5, 4] = R5C4;
        }

        private void OnLetterClicked(object sender, EventArgs e)
        {
            if (isGameOver) return;

            //Checks if button is doing a button job
            var button = sender as Button;
            if (button == null) return;

            //Only adds letter if space exists in the current row
            if (currentCol < 5)
            {
                letterSpace[currentRow, currentCol].Text = button.Text;
                currentCol++;
            }
        }

        private void OnBackspaceClicked(object sender, EventArgs e)
        {
            if (isGameOver) return;

            //If there's at least one letter typed in the current row
            if (currentCol > 0)
            {
                currentCol--;
                letterSpace[currentRow, currentCol].Text = "";
            }
        }

        private void OnEnterClicked(object sender, EventArgs e)
        {
            if (isGameOver) return;

            //Proceeds if there is at least 5 letters in the current row
            if (currentCol < 5)
            {
                MessageLabel.Text = "Not enough letters!";
                return;
            }

            //Builds the guess string
            string guess = "";
            for (int c = 0; c < 5; c++)
            {
                guess += letterSpace[currentRow, c].Text;
            }

            //Implement a check if the guess is in a valid word list.

            //Compare guess to secret word and color the boxes
            ColorBoxes(guess.ToUpperInvariant());

            //Check for win
            if (guess.ToUpperInvariant() == secretWord)
            {
                MessageLabel.Text = $"You guessed it! The word was {secretWord}.";
                isGameOver = true;
                return;
            }

            //Move to next row
            currentRow++;
            currentCol = 0;

            //If guessed 6 times game over
            if (currentRow == 6)
            {
                MessageLabel.Text = $"No more tries! The word was {secretWord}.";
                isGameOver = true;
            }
        }

        private void ColorBoxes(string guess)
        { 
            //Builds dictionary of letter counts for the guess word
            Dictionary<char, int> letterCounts = new Dictionary<char, int>();
            foreach (char ch in secretWord)
            {
                if (!letterCounts.ContainsKey(ch))
                    letterCounts[ch] = 0;
                letterCounts[ch]++;
            }

            //If a guess char == secret char in same position, it's green.
            Color[] colors = new Color[5];
            for (int i = 0; i < 5; i++)
            {
                char guessChar = guess[i];
                char secretChar = secretWord[i];
                if (guessChar == secretChar)
                {
                    colors[i] = Colors.Green;
                    letterCounts[guessChar]--;
                }
            }

            //For positions not marked green:
            //If guessChar is in letterCounts > 0, mark yellow & decrement
            //Other marks gray
            for (int i = 0; i < 5; i++)
            {
                //If green skip
                if (colors[i] == Colors.Green)
                    continue;

                char guessChar = guess[i];
                if (letterCounts.ContainsKey(guessChar) && letterCounts[guessChar] > 0)
                {
                    colors[i] = Color.FromArgb("#C9B458");
                    letterCounts[guessChar]--;
                }
                else
                {
                    colors[i] = Color.FromArgb("#3b3b3b");
                }
            }

            //Applies the colors to the row
            for (int c = 0; c < 5; c++)
            {
                letterSpace[currentRow, c].BackgroundColor = colors[c];
            }
        }
    }
}
