using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace Sudoku
{
    public static class Database
    {
        private static string DbPath = "leaderboard.sqlite";
        private static string ConnectionString = $"Data Source={DbPath};Version=3;";

        public static void InitializeDatabase()
        {
            if (!File.Exists(DbPath))
            {
                SQLiteConnection.CreateFile(DbPath);
            }

            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    string sqlScores = @"
                        CREATE TABLE IF NOT EXISTS Scores (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name varchar(100) NOT NULL,
                            Difficulty INTEGER NOT NULL,
                            TimeInTicks INTEGER NOT NULL,
                            Date varchar(10) NOT NULL
                        );";
                    using (var command = new SQLiteCommand(sqlScores, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    string sqlUsers = @"
                        CREATE TABLE IF NOT EXISTS Users (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name varchar(100) NOT NULL UNIQUE, 
                            PasswordHash varchar(256) NOT NULL
                        );";
                    using (var command = new SQLiteCommand(sqlUsers, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации БД: {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public static bool RegisterUser(string name, string password, out string errorMessage, bool delete)
        {
            errorMessage = string.Empty;
            InitializeDatabase();

            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    long newUserId = -1; // Переменная для хранения ID нового пользователя

                    // 1. Вставка пользователя
                    string insertSql = "INSERT INTO Users (Name, PasswordHash) VALUES (@name, @hash)";
                    using (var command = new SQLiteCommand(insertSql, connection))
                    {
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@hash", HashPassword(password));
                        command.ExecuteNonQuery();
                    }

                    // 2. Получение ID только что вставленной строки
                    using (var idCommand = new SQLiteCommand("SELECT last_insert_rowid()", connection))
                    {
                        newUserId = (long)idCommand.ExecuteScalar();
                    }

                    // 3. Условное удаление, если delete == true
                    if (delete)
                    {
                        string deleteSql = "DELETE FROM Users WHERE Id = @id";
                        using (var deleteCommand = new SQLiteCommand(deleteSql, connection))
                        {
                            deleteCommand.Parameters.AddWithValue("@id", newUserId);
                            deleteCommand.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (SQLiteException ex)
            {
                if (ex.ResultCode == SQLiteErrorCode.Constraint)
                {
                    errorMessage = "Пользователь с таким именем уже существует.";
                }
                else
                {
                    errorMessage = $"Ошибка БД: {ex.Message}";
                }
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Ошибка: {ex.Message}";
                return false;
            }
        }

        public static bool AuthenticateUser(string name, string password)
        {
            InitializeDatabase();
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT PasswordHash FROM Users WHERE Name = @name";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@name", name);
                        var result = command.ExecuteScalar();

                        if (result != null)
                        {
                            string dbHash = result.ToString();
                            string inputHash = HashPassword(password);
                            return dbHash == inputHash;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка входа: {ex.Message}");
            }
            return false;
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool InsertScore(string name, int difficulty, TimeSpan time)
        {
            InitializeDatabase();
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    // 1. Пытаемся обновить существующую запись для этого игрока и сложности
                    string updateSql = "UPDATE Scores SET TimeInTicks = @time, Date = @date WHERE Name = @name AND Difficulty = @difficulty";

                    using (var updateCommand = new SQLiteCommand(updateSql, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@name", name);
                        updateCommand.Parameters.AddWithValue("@difficulty", difficulty);
                        updateCommand.Parameters.AddWithValue("@time", time.Ticks);
                        updateCommand.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        // 2. Если записей обновлено не было, значит записи нет, делаем INSERT
                        if (rowsAffected == 0)
                        {
                            string insertSql = "INSERT INTO Scores (Name, Difficulty, TimeInTicks, Date) VALUES (@name, @difficulty, @time, @date)";
                            using (var insertCommand = new SQLiteCommand(insertSql, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@name", name);
                                insertCommand.Parameters.AddWithValue("@difficulty", difficulty);
                                insertCommand.Parameters.AddWithValue("@time", time.Ticks);
                                insertCommand.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                insertCommand.ExecuteNonQuery();
                            }

                            return true;
                        }
                        else return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения счета: {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static List<ScoreEntry> GetTopScores(int difficulty, int limit = 10)
        {
            InitializeDatabase();
            var scores = new List<ScoreEntry>();
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = $"SELECT Name, Difficulty, TimeInTicks, Date FROM Scores WHERE Difficulty = @difficulty ORDER BY TimeInTicks ASC LIMIT @limit";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@difficulty", difficulty);
                        command.Parameters.AddWithValue("@limit", limit);

                        using (var reader = command.ExecuteReader())
                        {
                            int rank = 1;
                            while (reader.Read())
                            {
                                long ticks = reader.GetInt64(2);
                                scores.Add(new ScoreEntry
                                {
                                    Id = rank++,
                                    Name = reader.GetString(0),
                                    Difficulty = reader.GetInt32(1),
                                    TimeInTicks = ticks,
                                    Time = TimeSpan.FromTicks(ticks),
                                    Date = DateTime.Parse(reader.GetString(3))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки лидеров: {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return scores;
        }

        public static bool IsNewRecord(string name, int difficulty, TimeSpan newTime)
        {
            InitializeDatabase();
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    string sql = "SELECT MIN(TimeInTicks) FROM Scores WHERE Name = @name AND Difficulty = @difficulty";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@difficulty", difficulty);

                        object result = command.ExecuteScalar();

                        if (result == DBNull.Value || result == null)
                        {
                            return true; // Записи нет - значит это рекорд
                        }

                        long bestTimeTicks = (long)result;
                        return newTime.Ticks < bestTimeTicks;
                    }
                }
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}