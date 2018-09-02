using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenShotLib
{
    public class TopLeftCorner
    {
        public TopLeftCorner(int row, int column, int height = 0, int width = 0)
        {
            Position = new Position(row, column);
            Height = height;
            Width = width;
        }

        public Position Position { get; set; }

        public int N1(TopLeftCorner rhs) => Position.N1(rhs.Position);

        public int Height { get; set; }
        public int Width { get; set; }

        public override string ToString()
        {
            return $"{Position} (H:{Height} W:{Width})";
        }
    }
}
