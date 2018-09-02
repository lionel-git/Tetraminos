using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
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

        public int N1(Position rhs)
        {
            return Math.Abs(Row - rhs.Row) + Math.Abs(Column - rhs.Column);
        }

        public override string ToString()
        {
            return $"({Row},{Column})";
        }
    }
}
