using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    /// <summary>
    /// Класс для генерации головоломки судоку и её решения.
    /// </summary>
    public class SudokuGenerator
    {
        private Random random = new Random();
        private int[,] _solutionArray = new int[9, 9];
        private int[,] _puzzleArray = new int[9, 9];

        public int[,] SolutionArray => _solutionArray;

        public int[,] PuzzleArray => _puzzleArray;

        public int[,] GenerateNewPuzzle(int targetClues)
        {
            // 1. Очистка и генерация полного решения
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    _solutionArray[r, c] = 0;
                }
            }

            // Заполнение поля
            Solve(0, 0, _solutionArray);

            // 2. Копирование решения в массив головоломки
            Array.Copy(_solutionArray, _puzzleArray, _solutionArray.Length);

            // 3. Удаление чисел для создания головоломки
            RemoveNumbersToCreatePuzzle(targetClues);

            return _solutionArray;
        }

        private void RemoveNumbersToCreatePuzzle(int targetClues)
        {
            List<(int r, int c)> allCells = new List<(int r, int c)>();
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    allCells.Add((r, c));
                }
            }
            Shuffle(allCells); // Случайный порядок удаления

            int cellsToRemove = allCells.Count - targetClues;
            int removedCount = 0;

            foreach ((int r, int c) in allCells)
            {
                if (removedCount >= cellsToRemove)
                    break;

                _puzzleArray[r, c] = 0; // Удаляем число
                removedCount++;
            }
        }

        private bool Solve(int r, int c, int[,] grid)
        {
            if (r == 9)
                return true; // Вся сетка заполнена

            int nextR = (c == 8) ? r + 1 : r;
            int nextC = (c == 8) ? 0 : c + 1;

            if (grid[r, c] != 0)
            {
                // Ячейка уже заполнена, переходим к следующей
                return Solve(nextR, nextC, grid);
            }

            // Случайный порядок чисел от 1 до 9
            List<int> numbers = Enumerable.Range(1, 9).ToList();
            Shuffle(numbers);

            foreach (int num in numbers)
            {
                if (IsSafe(r, c, num, grid))
                {
                    grid[r, c] = num;

                    if (Solve(nextR, nextC, grid))
                    {
                        return true;
                    }

                    grid[r, c] = 0; 
                }
            }

            return false; 
        }

        private bool IsSafe(int r, int c, int num, int[,] grid)
        {
            // Проверка строки и столбца
            for (int i = 0; i < 9; i++)
            {
                if (grid[r, i] == num || grid[i, c] == num)
                    return false;
            }

            // Проверка блока 3x3
            int startRow = r - r % 3;
            int startCol = c - c % 3;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (grid[startRow + i, startCol + j] == num)
                        return false;
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