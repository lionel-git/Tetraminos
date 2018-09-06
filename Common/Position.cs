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

        public static Position operator +(Position p1, Position p2)
        {
            return new Position(p1.Row + p2.Row, p1.Column + p2.Column);
        }

        public override int GetHashCode()
        {
            UInt64 data = (UInt64)Row + (((UInt64)Column) << 32);
            return data.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var position = obj as Position;
            if (position == null)
                throw new Exception($"Invalid comparison of type '{GetType()}' with type: '{obj.GetType()}'");
            return Row == position.Row && Column == position.Column;
        }

        public override string ToString()
        {
            return $"({Row},{Column})";
        }
    }
}
