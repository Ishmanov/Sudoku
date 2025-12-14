using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sudoku
{
    /// <summary>
    /// Логика взаимодействия для PageMainMenu.xaml
    /// </summary>
    public partial class PageMainMenu : Page
    {
        public PageMainMenu()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is Window window)
            {
                window.Title = "Main Menu";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PageDifficultySelection difficultySelectionPage = new PageDifficultySelection();
            this.NavigationService.Navigate(difficultySelectionPage);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PageLeaderboard leaderboardPage = new PageLeaderboard();
            this.NavigationService.Navigate(leaderboardPage);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            PageDifficultySelection16 difficultySelection16Page = new PageDifficultySelection16();
            this.NavigationService.Navigate(difficultySelection16Page);
        }
    }
}
