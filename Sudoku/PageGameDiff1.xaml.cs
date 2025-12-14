using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Sudoku
{
    public class SudokuData
    {
        public int[][] Puzzle { get; set; }
        public int[][] Solution { get; set; }
    }

    public partial class PageGameDiff1 : Page
    {
        private SudokuGenerator generator = new SudokuGenerator();
        private DispatcherTimer timer;
        private TimeSpan timeElapsed;
        private int[,] mainArray = new int[9, 9];
        private int[,] solutionArray => generator.SolutionArray;
        private bool[,] isClue = new bool[9, 9];

        private int difficalty = 1;
        private int TargetClues = 35;
        private TextBox[,] allGridBox = new TextBox[9, 9];

        public PageGameDiff1()
        {
            InitializeComponent();
            ReadSettings();
            InitializeGridBlocks();
            generator.GenerateNewPuzzle(TargetClues);
            LoadPuzzle(generator.PuzzleArray);
            FillingInTheGrid();
            SavePuzzleToJson(generator.PuzzleArray, generator.SolutionArray);
            InitializeTimer();
            timer.Start();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is Window window)
            {
                window.Title = "Sudoku Game 9x9";
            }
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
            catch { difficalty = 1; }

            if (difficalty == 1) TargetClues = 35;
            else if (difficalty == 2) TargetClues = 28;
            else TargetClues = 20;
        }

        private void LoadPuzzle(int[,] puzzle)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    mainArray[i, j] = puzzle[i, j];
                    isClue[i, j] = puzzle[i, j] != 0;
                }
            }
        }

        private void InitializeGridBlocks()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    string name = $"TextBlock_{i}_{j}";
                    TextBox block = (TextBox)this.FindName(name);
                    if (block != null)
                    {
                        allGridBox[i, j] = block;
                        block.MaxLength = 1;
                        block.TextChanged += TextBox_TextChanged;
                        block.PreviewTextInput += TextBox_PreviewTextInput;
                    }
                }
            }
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
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[1-9]$"))
            {
                e.Handled = true;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text.Length > 1)
            {
                textBox.Text = textBox.Text.Substring(0, 1);
                textBox.CaretIndex = 1;
            }
            if (textBox.Text.Length > 0 && !char.IsDigit(textBox.Text[0]))
            {
                textBox.Text = "";
            }
        }

        private bool ReadInputAndValidate()
        {
            if (allGridBox[0, 0] == null) return false;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    string text = allGridBox[i, j].Text.Trim();
                    int number;

                    if (string.IsNullOrEmpty(text))
                        mainArray[i, j] = 0;
                    else if (int.TryParse(text, out number) && number >= 1 && number <= 9)
                        mainArray[i, j] = number;
                    else
                    {
                        MessageBox.Show($"Ошибка ввода в ячейке ({i + 1}, {j + 1})");
                        return false;
                    }
                }
            }
            return true;
        }

        private string CheckSudokuRules()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (mainArray[i, j] == 0) return "Не все ячейки заполнены.";

            for (int i = 0; i < 9; i++)
            {
                HashSet<int> rowSet = new HashSet<int>();
                for (int j = 0; j < 9; j++)
                    if (!rowSet.Add(mainArray[i, j])) return $"Повтор в строке {j + 1}.";
            }

            for (int j = 0; j < 9; j++)
            {
                HashSet<int> colSet = new HashSet<int>();
                for (int i = 0; i < 9; i++)
                    if (!colSet.Add(mainArray[i, j])) return $"Повтор в столбце {j + 1}.";
            }

            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    HashSet<int> boxSet = new HashSet<int>();
                    for (int i = blockRow * 3; i < blockRow * 3 + 3; i++)
                    {
                        for (int j = blockCol * 3; j < blockCol * 3 + 3; j++)
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
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
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
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
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
                MessageBox.Show("Поздравляем! Вы решили судоку верно!", "Победа", MessageBoxButton.OK, MessageBoxImage.Information);

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

        private void SavePuzzleToJson(int[,] puzzle, int[,] solution)
        {
            try
            {
                var puzzleList = new List<int[]>();
                var solutionList = new List<int[]>();
                for (int i = 0; i < 9; i++)
                {
                    int[] rowP = new int[9];
                    int[] rowS = new int[9];
                    for (int j = 0; j < 9; j++)
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