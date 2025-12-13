using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
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
using static Sudoku.PageAccount;

namespace Sudoku
{
    public partial class PageDifficultySelection : Page
    {
        public PageDifficultySelection()
        {
            InitializeComponent();
        }

        public class SettingsData
        {
            public int difficalty { get; set; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string json = File.ReadAllText("settings.json");
            JsonNode jsonNode = JsonNode.Parse(json);
            if (jsonNode is JsonObject jsonObject && jsonObject.ContainsKey("difficalty"))
            {
                jsonObject["difficalty"] = 1;
            }
            string updatedJson = jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("settings.json", updatedJson);


            PageGameDiff1 GameDiff1Page = new PageGameDiff1();
            this.NavigationService.Navigate(GameDiff1Page);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string json = File.ReadAllText("settings.json");
            JsonNode jsonNode = JsonNode.Parse(json);
            if (jsonNode is JsonObject jsonObject && jsonObject.ContainsKey("difficalty"))
            {
                jsonObject["difficalty"] = 2;
            }
            string updatedJson = jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("settings.json", updatedJson);

            PageGameDiff1 GameDiff1Page = new PageGameDiff1();
            this.NavigationService.Navigate(GameDiff1Page);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string json = File.ReadAllText("settings.json");
            JsonNode jsonNode = JsonNode.Parse(json);
            if (jsonNode is JsonObject jsonObject && jsonObject.ContainsKey("difficalty"))
            {
                jsonObject["difficalty"] = 3;
            }
            string updatedJson = jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("settings.json", updatedJson);

            PageGameDiff1 GameDiff1Page = new PageGameDiff1();
            this.NavigationService.Navigate(GameDiff1Page);
        }
    }
}
