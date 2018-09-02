using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using log4net;
using System.Drawing;
using System.Drawing.Imaging;
using Common;

namespace ScreenShotLib
{
    public class ScreenShotParser
    {
        static readonly ILog Logger = LogManager.GetLogger(nameof(ScreenShotParser));

        // Pixels int=(r,g,b) Height(rows) x Width (columns)
        private RGB[,] _pixels;

        private List<TopLeftCorner> _topLeftCorners;

        private static readonly RGB Black = new RGB(0, 0, 0);

        public ScreenShotParser()
        {
            _topLeftCorners = new List<TopLeftCorner>();
        }

        public void LoadScreenShot(string fileName)
        {
            var image = Image.FromFile(fileName);
            var rawBitMap = new RawBitmap(image);
            _pixels = rawBitMap.ConvertDatas();
        }

        private bool IsNearBlack(int i, int j)
        {
            return Black.N1(_pixels[i, j]) <= 3 * 18;
        }

        private int GetBlackRows(int i, int j)
        {
            int k = 0;
            while (i + k < _pixels.GetLength(0) && IsNearBlack(i + k, j))
                k++;
            return k;
        }

        private int GetBlackColumns(int i, int j)
        {
            int k = 0;
            while (j + k < _pixels.GetLength(1) && IsNearBlack(i, j + k))
                k++;
            return k;
        }

        private void AddTopLeftCorner(TopLeftCorner newCorner)
        {
            foreach (var corner in _topLeftCorners)
                if (newCorner.N1(corner) < 2 * 10)
                    return;
            _topLeftCorners.Add(newCorner);
        }

        public void SearchTopLeftAngle(int minRows, int minColumns)
        {
            _topLeftCorners.Clear();
            for (int i = 0; i < _pixels.GetLength(0) - minRows; i++)
                for (int j = 0; j < _pixels.GetLength(1) - minColumns; j++)
                {
                    int rows = GetBlackRows(i, j);
                    if (rows > minRows)
                    {
                        int columns = GetBlackColumns(i, j);
                        if (columns > minColumns)
                            AddTopLeftCorner(new TopLeftCorner(i, j, rows, columns));
                    }
                }
        }

        public void SearchBlackLines()
        {
            int startColumn = -1;
            int count = 0;
            for (int i = 0; i < _pixels.GetLength(0); i++)
                for (int j = 0; j < _pixels.GetLength(1); j++)
                    if (IsNearBlack(i, j))
                    {
                       // Logger.Info($"({i}, {j}) => {_pixels[i, j]}");
                        count++;
                        if (startColumn == -1)
                            startColumn = j;
                    }
                    else
                    {
                        if (startColumn >= 0)
                        {
                            Logger.Info($"Line {i} => C:{startColumn}, L:{j - startColumn}");
                            startColumn = -1;
                        }
                    }
            Logger.Info($"Found: {count}");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"H:{_pixels.GetLength(0)} x W:{_pixels.GetLength(1)}");
            sb.AppendLine($"Top Left: {_topLeftCorners.Count}");
            foreach (var tla in _topLeftCorners)
            {
                sb.AppendLine($"{tla}");
            }
            return sb.ToString();
        }
    }
}
