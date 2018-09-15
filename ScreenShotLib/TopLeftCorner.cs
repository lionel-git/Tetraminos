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
            PixelSamples = new List<RGB>();
        }

        public Position Position { get; set; }

        public List<RGB> PixelSamples { get; set; }

        public int N1(TopLeftCorner rhs) => Position.N1(rhs.Position);

        public int Height { get; set; }
        public int Width { get; set; }

        public List<Position> SquarePositions { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{Position} (H:{Height} W:{Width}) ");
            if (SquarePositions != null)
                foreach (var sp in SquarePositions)
                    sb.Append(sp);
            return sb.ToString();
        }

        public string SquaresString()
        {
            int minRow = int.MaxValue;
            int maxRow = int.MinValue;
            int minColumn = int.MaxValue;
            int maxColumn = int.MinValue;

            foreach (var sp in SquarePositions)
            {
                minRow = Math.Min(minRow, sp.Row);
                maxRow = Math.Max(maxRow, sp.Row);
                minColumn = Math.Min(minColumn, sp.Column);
                maxColumn = Math.Max(maxColumn, sp.Column);
            }

            var rows = maxRow - minRow + 1;
            var columns = maxColumn - minColumn + 1;
            if (rows > 1 || columns > 1)
            {
                var sb = new StringBuilder();
                sb.Append($"{rows},{columns},");
                if (rows > 1 && columns > 1)
                {
                    var datas = Helpers.InitArray<bool>(rows, columns);
                    foreach (var sp in SquarePositions)
                        datas[sp.Row - minRow, sp.Column - minColumn] = true;
                    for (int i = 0; i < datas.GetLength(0); i++)
                        for (int j = 0; j < datas.GetLength(1); j++)
                            sb.Append(Constants.ConvertBool(datas[i, j]));
                }
                return sb.ToString();
            }
            else
                return null;
        }
    }
}
