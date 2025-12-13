using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Sudoku
{
    public partial class PageLeaderboard : Page
    {
        public PageLeaderboard()
        {
            InitializeComponent();
            Database.InitializeDatabase();
            LoadLeaderboard(1); 
        }

        private void LoadLeaderboard(int difficulty)
        {
            List<ScoreEntry> topScores = Database.GetTopScores(difficulty);
            LeaderboardDataGrid.ItemsSource = topScores;

            BtnDiff1.FontWeight = FontWeights.Normal;
            BtnDiff2.FontWeight = FontWeights.Normal;
            BtnDiff3.FontWeight = FontWeights.Normal;
            BtnDiff4.FontWeight = FontWeights.Normal;
            BtnDiff5.FontWeight = FontWeights.Normal;
            BtnDiff6.FontWeight = FontWeights.Normal;

            switch (difficulty)
            {
                case 1: BtnDiff1.FontWeight = FontWeights.Bold; break;
                case 2: BtnDiff2.FontWeight = FontWeights.Bold; break;
                case 3: BtnDiff3.FontWeight = FontWeights.Bold; break;
                case 4: BtnDiff4.FontWeight = FontWeights.Bold; break;
                case 5: BtnDiff5.FontWeight = FontWeights.Bold; break;
                case 6: BtnDiff6.FontWeight = FontWeights.Bold; break;
            }
        }

        private void DifficultyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag && int.TryParse(tag, out int difficulty))
            {
                LoadLeaderboard(difficulty);
            }
        }

        private void BackToMainMenu_Click(object sender, RoutedEventArgs e)
        {
            PageMainMenu mainMenuPage = new PageMainMenu();
            this.NavigationService.Navigate(mainMenuPage);
        }
    }
}