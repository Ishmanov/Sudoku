using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Sudoku
{
    public partial class PageGameDiff16 : Page
    {
        private SudokuGenerator16 generator = new SudokuGenerator16();
        private DispatcherTimer timer;
        private TimeSpan timeElapsed;
        private int[,] mainArray = new int[16, 16];
        private int[,] solutionArray => generator.SolutionArray;
        private bool[,] isClue = new bool[16, 16];

        private int difficalty = 4; 
        private int TargetClues = 150; 
        private TextBox[,] allGridBox = new TextBox[16, 16];

        public PageGameDiff16()
        {
            InitializeComponent();
            ReadSettings();
            GenerateGridUI();
            generator.GenerateNewPuzzle(TargetClues);
            LoadPuzzle(generator.PuzzleArray);
            FillingInTheGrid();

            SavePuzzleToJson(generator.PuzzleArray, generator.SolutionArray);

            InitializeTimer();
            timer.Start();
        }

        private void ReadSettings()
        {
            try
            {
                if (File.Exists("settings.json"))
                {
                    string jsonContent = File.ReadAllText("settings.json");
                    using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                    {
                        JsonElement root = doc.RootElement;
                        if (root.TryGetProperty("difficalty", out JsonElement element))
                        {
                            difficalty = element.GetInt32();
                        }
                    }
                }
            }
            catch { difficalty = 4; }

            if (difficalty == 4) TargetClues = 160;
            else if (difficalty == 5) TargetClues = 130;
            else TargetClues = 100;
        }

        /// <summary>
        /// Динамическое создание сетки 16x16 с блоками 4x4
        /// </summary>
        private void GenerateGridUI()
        {
            for (int i = 0; i < 4; i++)
            {
                MainSudokuGrid.RowDefinitions.Add(new RowDefinition());
                MainSudokuGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int blockRow = 0; blockRow < 4; blockRow++)
            {
                for (int blockCol = 0; blockCol < 4; blockCol++)
                {
                    Grid regionGrid = new Grid();
                    regionGrid.Margin = new Thickness(1); 
                    regionGrid.Background = Brushes.White;

                    for (int k = 0; k < 4; k++)
                    {
                        regionGrid.RowDefinitions.Add(new RowDefinition());
                        regionGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    }

                    Grid.SetRow(regionGrid, blockRow);
                    Grid.SetColumn(regionGrid, blockCol);
                    MainSudokuGrid.Children.Add(regionGrid);

                    for (int r = 0; r < 4; r++)
                    {
                        for (int c = 0; c < 4; c++)
                        {
                            TextBox tb = new TextBox();
                            int globalRow = blockRow * 4 + r;
                            int globalCol = blockCol * 4 + c;

                            tb.Name = $"TextBlock_{globalRow}_{globalCol}";
                            tb.Style = (Style)FindResource(typeof(TextBox)); 
                            tb.MaxLength = 2; 

                            // События
                            tb.TextChanged += TextBox_TextChanged;
                            tb.PreviewTextInput += TextBox_PreviewTextInput;

                            Grid.SetRow(tb, r);
                            Grid.SetColumn(tb, c);
                            regionGrid.Children.Add(tb);

                            allGridBox[globalRow, globalCol] = tb;
                        }
                    }
                }
            }
        }

        private void LoadPuzzle(int[,] puzzle)
        {
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    mainArray[i, j] = puzzle[i, j];
                    isClue[i, j] = puzzle[i, j] != 0;
                }
            }
        }

        private void SavePuzzleToJson(int[,] puzzle, int[,] solution)
        {
            try
            {
                var puzzleList = new List<int[]>();
                var solutionList = new List<int[]>();
                for (int i = 0; i < 16; i++)
                {
                    int[] rowP = new int[16];
                    int[] rowS = new int[16];
                    for (int j = 0; j < 16; j++)
                    {
                        rowP[j] = puzzle[i, j];
                        rowS[j] = solution[i, j];
                    }
                    puzzleList.Add(rowP);
                    solutionList.Add(rowS);
                }

                var data = new SudokuData { Puzzle = puzzleList.ToArray(), Solution = solutionList.ToArray() };
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText("sudoku_answer.json", JsonSerializer.Serialize(data, options));
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string name = textBox.Name;
            string[] parts = name.Split('_');
            if (parts.Length == 3 && int.TryParse(parts[1], out int i) && int.TryParse(parts[2], out int j))
            {
                if (isClue[i, j])
                {
                    e.Handled = true;
                    return;
                }
            }

            if (!Regex.IsMatch(e.Text, "^[0-9]+$"))
            {
                e.Handled = true;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (int.TryParse(textBox.Text, out int val))
            {
                if (val < 1 || val > 16)
                {
                    textBox.Foreground = Brushes.Red; 
                }
                else
                {
                    textBox.Foreground = Brushes.Blue;
                }
            }
            else if (!string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = ""; 
            }
        }

        private bool ReadInputAndValidate()
        {
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    string text = allGridBox[i, j].Text.Trim();
                    int number;

                    if (string.IsNullOrEmpty(text))
                        mainArray[i, j] = 0;
                    else if (int.TryParse(text, out number) && number >= 1 && number <= 16)
                        mainArray[i, j] = number;
                    else
                    {
                        MessageBox.Show($"Неверное число в ячейке ({i + 1}, {j + 1}). Допустимо 1-16.");
                        return false;
                    }
                }
            }
            return true;
        }

        private string CheckSudokuRules()
        {
            for (int i = 0; i < 16; i++)
                for (int j = 0; j < 16; j++)
                    if (mainArray[i, j] == 0) return "Не все ячейки заполнены.";

            // Проверка строк
            for (int i = 0; i < 16; i++)
            {
                HashSet<int> rowSet = new HashSet<int>();
                for (int j = 0; j < 16; j++)
                    if (!rowSet.Add(mainArray[i, j])) return $"Повтор в строке {j + 1}.";
            }

            // Проверка столбцов
            for (int j = 0; j < 16; j++)
            {
                HashSet<int> colSet = new HashSet<int>();
                for (int i = 0; i < 16; i++)
                    if (!colSet.Add(mainArray[i, j])) return $"Повтор в столбце {j + 1}.";
            }

            // Проверка блоков 4x4
            for (int blockRow = 0; blockRow < 4; blockRow++)
            {
                for (int blockCol = 0; blockCol < 4; blockCol++)
                {
                    HashSet<int> boxSet = new HashSet<int>();
                    for (int i = blockRow * 4; i < blockRow * 4 + 4; i++)
                    {
                        for (int j = blockCol * 4; j < blockCol * 4 + 4; j++)
                        {
                            if (!boxSet.Add(mainArray[i, j])) return $"Повтор в блоке.";
                        }
                    }
                }
            }
            return string.Empty;
        }

        private void FillingInTheGrid()
        {
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    TextBox block = allGridBox[i, j];
                    if (block == null) continue;

                    if (isClue[i, j])
                    {
                        block.Text = $"{mainArray[i, j]}";
                        block.IsReadOnly = true;
                        block.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));
                        block.Foreground = Brushes.Black;
                        block.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        block.Text = (mainArray[i, j] != 0) ? $"{mainArray[i, j]}" : "";
                        block.IsReadOnly = false;
                        block.Background = Brushes.White;
                        block.Foreground = Brushes.Blue;
                        block.FontWeight = FontWeights.Normal;
                    }
                }
            }
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timeElapsed = TimeSpan.Zero;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timeElapsed = timeElapsed.Add(TimeSpan.FromSeconds(1));
            LabelTime.Content = timeElapsed.ToString(@"mm\:ss");
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            PageMainMenu mainMenuPage = new PageMainMenu();
            this.NavigationService.Navigate(mainMenuPage);
        }

        private void ButtonEnd_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();

            if (!ReadInputAndValidate())
            {
                timer.Start();
                return;
            }

            string validationError = CheckSudokuRules();
            bool isSolvedCorrectly = true;

            if (string.IsNullOrEmpty(validationError))
            {
                for (int i = 0; i < 16; i++)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        if (mainArray[i, j] != solutionArray[i, j])
                        {
                            isSolvedCorrectly = false;
                            break;
                        }
                    }
                    if (!isSolvedCorrectly) break;
                }
            }
            else
            {
                isSolvedCorrectly = false;
            }

            if (isSolvedCorrectly)
            {
                MessageBox.Show("Поздравляем! Вы решили судоку 16x16 верно!", "Победа", MessageBoxButton.OK, MessageBoxImage.Information);

                string userName = GetUserName();

                if (Database.IsNewRecord(userName, difficalty, timeElapsed))
                {
                    Database.InsertScore(userName, difficalty, timeElapsed);
                    MessageBox.Show($"Новый рекорд для {userName}!", "Рекорд", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Результат не является рекордом.", "Победа", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show(string.IsNullOrEmpty(validationError) ? "Неверное решение!" : validationError, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                timer.Start();
            }
        }

        private string GetUserName()
        {
            if (!string.IsNullOrEmpty(UserSession.CurrentUserName))
            {
                return UserSession.CurrentUserName;
            }
            return "Guest";
        }
    }
}