using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Wordle
{
    public static class WordAPI
    {
        private static readonly string fileName = Path.Combine(FileSystem.AppDataDirectory, "word.txt");
        private static readonly HttpClient client = new HttpClient();
        private static List<string> words = new List<string>();

        public static async Task InitializeAsync()
        {
            await SaveWords();
        }

        public static List<string> GetWords()
        {
            return words;
        }

        private static async Task SaveWords()
        {
            try
            {
                string url = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt";
                string txt = await client.GetStringAsync(url);

                string? directory = Path.GetDirectoryName(fileName);
                if (directory != null)
                {
                    Directory.CreateDirectory(directory);
                }
                else
                {
                    throw new InvalidOperationException("Invalid file path: Unable to determine directory.");
                }

                var lines = txt
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim().ToUpper())
                    .ToList();

                words = lines.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                words = new List<string>();
            }
        }
    }
}
