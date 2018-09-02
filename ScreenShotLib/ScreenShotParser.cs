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

        private List<Position> _topLeftAngels;

        private static readonly RGB Black = new RGB(0, 0, 0);

        int BlackThreshold = 55;

        public ScreenShotParser()
        {
            _topLeftAngels = new List<Position>();
        }

        public void LoadScreenShot(string fileName)
        {
            var image = Image.FromFile(fileName);
            var rawBitMap = new RawBitmap(image);
            _pixels = rawBitMap.ConvertDatas();
        }

        private bool CheckBlackLine(int i, int j, int rows)
        {
            for (int k = 0; k < rows; k++)
                if (Black.N1(_pixels[i + k, j]) > BlackThreshold)
                    return false;
            return true;
        }

        private bool CheckBlackColumn(int i, int j, int columns)
        {
            for (int k = 0; k < columns; k++)
                if (Black.N1(_pixels[i, j + k]) > BlackThreshold)
                    return false;
            return true;
        }

        private void AddLefAngel(Position newPosition)
        {
            foreach (var position in _topLeftAngels)
                if (newPosition.N1(position) < 20)
                    return;
            _topLeftAngels.Add(newPosition);
        }

        public void SearchTopLeftAngle(int rows, int columns)
        {
            _topLeftAngels.Clear();
            for (int i = 0; i < _pixels.GetLength(0) - rows; i++)
                for (int j = 0; j < _pixels.GetLength(1) - columns; j++)
                {
                    if (CheckBlackLine(i, j, rows) && CheckBlackColumn(i, j, columns))
                        AddLefAngel(new Position(i, j));
                }
        }

        public void SearchBlackLines()
        {
            int startColumn = -1;
            int count = 0;
            for (int i = 0; i < _pixels.GetLength(0); i++)
                for (int j = 0; j < _pixels.GetLength(1); j++)
                    if (Black.N1(_pixels[i, j]) < BlackThreshold)
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
            sb.AppendLine($"Top Left: {_topLeftAngels.Count}");
            foreach (var tla in _topLeftAngels)
            {
                sb.AppendLine($"{tla}");
            }
            return sb.ToString();
        }
    }
}
