using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraMinos2
{
    public class Position
    {
        public Position(int row, int col)
        {
            Row = row;
            Column = col;
        }

        public int Row { get; set; }
        public int Column { get; set; }

        public override string ToString()
        {
            return $"({Row},{Column})";
        }
    }
}
