using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    /// <summary>
    /// Класс для генерации головоломки судоку 16x16.
    /// </summary>
    public class SudokuGenerator16
    {
        private Random random = new Random();
        private int[,] _solutionArray = new int[16, 16];
        private int[,] _puzzleArray = new int[16, 16];

        public int[,] SolutionArray => _solutionArray;
        public int[,] PuzzleArray => _puzzleArray;

        public int[,] GenerateNewPuzzle(int targetClues)
        {
            // Очистка
            for (int r = 0; r < 16; r++)
                for (int c = 0; c < 16; c++)
                    _solutionArray[r, c] = 0;

            // Генерация полного решения
            Solve(0, 0, _solutionArray);

            // Копирование в пазл
            Array.Copy(_solutionArray, _puzzleArray, _solutionArray.Length);

            // Удаление чисел
            RemoveNumbersToCreatePuzzle(targetClues);

            return _solutionArray;
        }

        private void RemoveNumbersToCreatePuzzle(int targetClues)
        {
            List<(int r, int c)> allCells = new List<(int r, int c)>();
            for (int r = 0; r < 16; r++)
                for (int c = 0; c < 16; c++)
                    allCells.Add((r, c));

            Shuffle(allCells);

            int cellsToRemove = allCells.Count - targetClues;
            int removedCount = 0;

            foreach ((int r, int c) in allCells)
            {
                if (removedCount >= cellsToRemove)
                    break;

                _puzzleArray[r, c] = 0;
                removedCount++;
            }
        }

        private bool Solve(int r, int c, int[,] grid)
        {
            if (r == 16)
                return true;

            int nextR = (c == 15) ? r + 1 : r;
            int nextC = (c == 15) ? 0 : c + 1;

            if (grid[r, c] != 0)
                return Solve(nextR, nextC, grid);

            List<int> numbers = Enumerable.Range(1, 16).ToList();
            Shuffle(numbers);

            foreach (int num in numbers)
            {
                if (IsSafe(r, c, num, grid))
                {
                    grid[r, c] = num;
                    if (Solve(nextR, nextC, grid)) return true;
                    grid[r, c] = 0;
                }
            }

            return false;
        }

        private bool IsSafe(int r, int c, int num, int[,] grid)
        {
            // Строка и столбец
            for (int i = 0; i < 16; i++)
            {
                if (grid[r, i] == num || grid[i, c] == num) return false;
            }

            // Блок 4x4
            int startRow = r - r % 4;
            int startCol = c - c % 4;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (grid[startRow + i, startCol + j] == num) return false;
                }
            }

            return true;
        }

        private void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}