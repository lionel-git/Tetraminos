﻿using Common;
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
            int rows, columns;
            var datas = Helpers.GenerateDatas(SquarePositions, out rows, out columns);
            if (rows > 1 || columns > 1)
            {
                var sb = new StringBuilder();
                sb.Append($"{rows},{columns},");
                if (rows > 1 && columns > 1)
                    sb.Append(datas);
                return sb.ToString();
            }
            else
                return null;
        }
    }
}
