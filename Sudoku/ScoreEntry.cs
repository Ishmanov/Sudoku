using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    public class ScoreEntry
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public int Difficulty { get; set; } 
        public TimeSpan Time { get; set; } 
        public long TimeInTicks { get; set; }
        public DateTime Date { get; set; }
    }
}
