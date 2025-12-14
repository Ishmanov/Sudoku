using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Sudoku
{
    public partial class PageDifficultySelection16 : Page
    {
        public PageDifficultySelection16()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is Window window)
            {
                window.Title = "Difficulty Selection 16x16";
            }
        }

        private void SaveDifficultyAndNavigate(int difficulty)
        {
            try
            {
                string json = "{}";
                if (File.Exists("settings.json"))
                {
                    json = File.ReadAllText("settings.json");
                }

                JsonNode jsonNode = JsonNode.Parse(json) ?? new JsonObject();
                jsonNode["difficalty"] = difficulty;

                string updatedJson = jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("settings.json", updatedJson);

                // Переход на страницу игры 16x16
                PageGameDiff16 gamePage = new PageGameDiff16();
                this.NavigationService.Navigate(gamePage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}");
            }
        }

        private void Button_Easy_Click(object sender, RoutedEventArgs e)
        {
            SaveDifficultyAndNavigate(4); 
        }

        private void Button_Medium_Click(object sender, RoutedEventArgs e)
        {
            SaveDifficultyAndNavigate(5);
        }

        private void Button_Hard_Click(object sender, RoutedEventArgs e)
        {
            SaveDifficultyAndNavigate(6); 
        }

        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            PageDifficultySelection page9x9 = new PageDifficultySelection();
            this.NavigationService.Navigate(page9x9);
        }
    }
}