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

        private string _name;

        private int _squareHeight;
        private int _squareWidth;

        private List<Tuple<double, double>> _coeffs;

        public ScreenShotParser()
        {
            _topLeftCorners = new List<TopLeftCorner>();
            _coeffs = new List<Tuple<double, double>>();
            _coeffs.Add(new Tuple<double, double>(0.5, 0.5));
            _coeffs.Add(new Tuple<double, double>(0.25, 0.25));
            _coeffs.Add(new Tuple<double, double>(0.25, 0.75));
            _coeffs.Add(new Tuple<double, double>(0.75, 0.25));
            _coeffs.Add(new Tuple<double, double>(0.75, 0.75));
        }

        public void LoadScreenShot(string fileName, string name)
        {
            _name = name;
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

        private double Eps(int n, int b)
        {
            double r = n / (double)b; // Should be close to integer
            return Math.Abs(r - (int)(r + 0.5));
        }

        private RGB GetPixel(TopLeftCorner corner, Tuple<double, double> coeffs, int shiftSquareRows = 0, int shiftSquareColumns = 0)
        {
            int row = (int)(corner.Position.Row + (shiftSquareRows + coeffs.Item1) * _squareHeight);
            int column = (int)(corner.Position.Column + (shiftSquareColumns + coeffs.Item2) * _squareWidth);
            if (row >= 0 && row <= _pixels.GetLength(0) && column >= 0 && column <= _pixels.GetLength(1))
                return _pixels[row, column];
            else
                return null;
        }

        private int DistanceSample(TopLeftCorner corner, int squareRow, int squareColumn)
        {
            int distance = 0;
            for (int i = 0; i < _coeffs.Count; i++)
            {
                var rgb = GetPixel(corner, _coeffs[i], squareRow, squareColumn);
                Logger.Info($"To compare: {rgb} {corner.PixelSamples[i]}");
                if (rgb != null)
                    distance += corner.PixelSamples[i].N1(rgb);
                else
                    distance += int.MaxValue / (_coeffs.Count+1);
            }
            Logger.Info($"Distance {squareRow} {squareColumn} = {distance}");
            return distance;
        }

        public void GetBaseDimensions()
        {
            _squareHeight = int.MaxValue;
            _squareWidth = int.MaxValue;
            foreach (var corner in _topLeftCorners)
            {
                _squareHeight = Math.Min(_squareHeight, corner.Height);
                _squareWidth = Math.Min(_squareWidth, corner.Width);
            }
            _squareHeight -= 1;
            _squareWidth -= 1;

            Logger.Info($"H:{_squareHeight},W:{_squareWidth}");

            // Check integers
            foreach (var corner in _topLeftCorners)
            {
                double epsW = Eps(corner.Width, _squareWidth);
                if (epsW > 0.05)
                    Logger.Info($"Bad width: {corner.Width} W:{epsW}");
                double epsH = Eps(corner.Height, _squareHeight);
                if (epsH > 0.05)
                    Logger.Info($"Bad height: {corner.Height} H:{epsH}");
            }

            foreach (var corner in _topLeftCorners)
            {
                foreach (var coeff in _coeffs)
                {
                    var rgb = GetPixel(corner, coeff);
                    corner.PixelSamples.Add(rgb);
                }
            }

            // A partir d'un top left corner, chercher sur haut/bas/gauche/droite
            foreach (var corner in _topLeftCorners)
            {
                Logger.Info($"\n==== {corner}");
                int distanceH = DistanceSample(corner, -1, 0);
                int distanceB = DistanceSample(corner, +1, 0);
                int distanceG = DistanceSample(corner, 0, -1);
                int distanceD = DistanceSample(corner, 0, +1);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{_name} H:{_pixels.GetLength(0)} x W:{_pixels.GetLength(1)}");
            sb.AppendLine($"Top Left: {_topLeftCorners.Count}");
            foreach (var tla in _topLeftCorners)
            {
                sb.AppendLine($"{tla}");
            }
            return sb.ToString();
        }
    }
}
