using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using Sudoku;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace UnitTestSudoku
{

    [TestClass]
    public class UnitTest1
    {

        public static bool IsValidSudoku(int[,] board)
        {
            int size = 9;

            for (int i = 0; i < size; i++)
            {
                HashSet<int> rowSet = new HashSet<int>();
                HashSet<int> colSet = new HashSet<int>();

                for (int j = 0; j < size; j++)
                {
                    int rowValue = board[i, j];
                    int colValue = board[j, i];

                    if (rowValue < 1 || rowValue > 9) return false;
                    if (rowSet.Contains(rowValue))
                    {
                        return false;
                    }
                    rowSet.Add(rowValue);

                    if (colValue < 1 || colValue > 9) return false;
                    if (colSet.Contains(colValue))
                    {
                        return false;
                    }
                    colSet.Add(colValue);
                }
            }

            int subgridSize = 3;
            for (int blockRow = 0; blockRow < size; blockRow += subgridSize)
            {
                for (int blockCol = 0; blockCol < size; blockCol += subgridSize)
                {
                    HashSet<int> blockSet = new HashSet<int>();

                    for (int i = 0; i < subgridSize; i++)
                    {
                        for (int j = 0; j < subgridSize; j++)
                        {
                            int value = board[blockRow + i, blockCol + j];

                            if (value < 1 || value > 9) return false;
                            if (blockSet.Contains(value))
                            {
                                return false;
                            }
                            blockSet.Add(value);
                        }
                    }
                }
            }

            return true;
        }

        public static bool IsValidSudoku16(int[,] board)
        {
            int size = 16;
            int subgridSize = 4; 

            // 1. Проверка строк и столбцов
            for (int i = 0; i < size; i++)
            {
                HashSet<int> rowSet = new HashSet<int>();
                HashSet<int> colSet = new HashSet<int>();

                for (int j = 0; j < size; j++)
                {
                    int rowValue = board[i, j];
                    int colValue = board[j, i];

                    // Проверка диапазона значений 
                    if (rowValue < 1 || rowValue > size || colValue < 1 || colValue > size)
                    {
                        return false;
                    }

                    // Проверка повторяющихся значений в строке
                    if (rowSet.Contains(rowValue))
                    {
                        return false;
                    }
                    rowSet.Add(rowValue);

                    // Проверка повторяющихся значений в столбце
                    if (colSet.Contains(colValue))
                    {
                        return false;
                    }
                    colSet.Add(colValue);
                }
            }

            // 2. Проверка подсеток 4x4
            for (int blockRowStart = 0; blockRowStart < size; blockRowStart += subgridSize)
            {
                for (int blockColStart = 0; blockColStart < size; blockColStart += subgridSize)
                {
                    HashSet<int> blockSet = new HashSet<int>();

                    for (int i = 0; i < subgridSize; i++)
                    {
                        for (int j = 0; j < subgridSize; j++)
                        {
                            int value = board[blockRowStart + i, blockColStart + j];

                            if (blockSet.Contains(value))
                            {
                                return false;
                            }
                            blockSet.Add(value);
                        }
                    }
                }
            }

            return true;
        }

        public static bool SearchName(string name)
        {
            Database.InitializeDatabase();
            var usernames = new List<string>();

            string localDbPath = "leaderboard.sqlite";
            string connectionString = $"Data Source={localDbPath};Version=3;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT Name FROM Users ORDER BY Name ASC";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usernames.Add(reader.GetString(0));
                        }
                    }
                }
            }

            string[] names = usernames.ToArray();

            return !names.Contains(name);
        }

        public static bool SearchRecord(string name, int diff, int sec)
        {
            Database.InitializeDatabase();
            string localDbPath = "leaderboard.sqlite";
            string connectionString = $"Data Source={localDbPath};Version=3;";

            long? timeInTicks = null;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT TimeInTicks FROM Scores WHERE Name = @name AND Difficulty = @difficulty";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@difficulty", diff);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            timeInTicks = reader.GetInt64(0);
                        }
                    }
                }
            }

            if (timeInTicks == null)
            {
                return false;
            }

            TimeSpan dbTime = new TimeSpan(timeInTicks.Value);

            return sec == dbTime.TotalSeconds;
        }
    

        [TestMethod]
        public void TestGenerate9x9()
        {
            SudokuGenerator sudoku = new SudokuGenerator();

            int[,] a = sudoku.GenerateNewPuzzle(30);

            Assert.AreEqual(true, IsValidSudoku(a));
        }

        [TestMethod]
        public void TestGenerate16x16()
        {
            SudokuGenerator16 sudoku16 = new SudokuGenerator16();

            int[,] a = sudoku16.GenerateNewPuzzle(30);

            Assert.AreEqual(true, IsValidSudoku16(a));
        }

        [TestMethod]
        public void TestRegistration()
        {
            string name = "UnitTest";
            string password = "password";

            bool search = SearchName(name);
            bool register = Database.RegisterUser(name, password, out string dbError, true);

            Assert.AreEqual(search, register);
        }

        [TestMethod]
        public void TestInsetScore()
        {
            string name = "UnitTest";
            int diff = 3;
            int sec = 200;
            TimeSpan time = TimeSpan.FromSeconds(sec);

            bool insert = Database.InsertScore(name, diff, time);
            bool search = SearchRecord(name, diff, sec);

            Assert.AreEqual(search, insert);
        }
    }
}
