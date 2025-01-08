using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Wordle
{
    public partial class HistoryPage : ContentPage
    {
        //Name of the JSON file for saving attempts + player name
        private static readonly string historyFileName = Path.Combine(FileSystem.AppDataDirectory, "playerHistory.json");

        private PlayerHistory history;

        public HistoryPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //Loads existing or new history from disk
            history = await LoadHistoryAsync();

            //Pops up for player name if blank or "Unknown"
            if (string.IsNullOrWhiteSpace(history.PlayerName) || history.PlayerName == "Unknown")
            {
                string name = await DisplayPromptAsync("Player Name", "Please enter your name:",
                    accept: "OK",
                    cancel: "Cancel",
                    placeholder: "Your Name Here");

                if (!string.IsNullOrWhiteSpace(name))
                {
                    history.PlayerName = name;
                    await SaveHistoryAsync(history);
                }
            }

            //Displays the player's name
            PlayerNameLabel.Text = $"Player: {history.PlayerName}";

            //Binds the Attempts to the CollectionView
            AttemptsView.ItemsSource = history.Attempts;
        }

        //Closes the page
        private async void CloseHistoryPage(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        //Records an attempt
        public static async Task RecordAttemptAsync(string correctWord, int guesses, double timeTaken)
        {
            //Loads existing or new
            PlayerHistory hist = await LoadHistoryAsync();

            //Creates a new attempt
            var attempt = new Attempt
            {
                TimeTaken = timeTaken,
                NumberOfGuesses = guesses,
                CorrectWord = correctWord
            };

            //Adds attempt to the list
            hist.Attempts.Add(attempt);

            //Saves updated history
            await SaveHistoryAsync(hist);
        }

        //Loads existing history
        public static async Task<PlayerHistory> LoadHistoryAsync()
        {
            try
            {
                if (File.Exists(historyFileName))
                {
                    string json = await File.ReadAllTextAsync(historyFileName);
                    var hist = JsonSerializer.Deserialize<PlayerHistory>(json);
                    if (hist != null) return hist;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading history: " + ex.Message);
            }
            return new PlayerHistory();
        }

        //Saves the updated history
        public static async Task SaveHistoryAsync(PlayerHistory hist)
        {
            try
            {
                string json = JsonSerializer.Serialize(hist, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(historyFileName, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving history: " + ex.Message);
            }
        }
    }

    //Represents one puzzle attempt
    public class Attempt
    {
        //How long it took
        public double TimeTaken { get; set; }

        //Number of guesses
        public int NumberOfGuesses { get; set; }

        //Correct guess
        public string CorrectWord { get; set; }
    }

    //Holds the player's name + list of attempts
    public class PlayerHistory
    {
        public string PlayerName { get; set; } = "Unknown";
        public List<Attempt> Attempts { get; set; } = new List<Attempt>();
    }
}
