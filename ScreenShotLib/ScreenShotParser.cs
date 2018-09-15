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

        private string _name;

        private int _squareHeight;
        private int _squareWidth;

        private List<Tuple<double, double>> _coeffs;

        private List<Position> _neighBoors;

        public ScreenShotParser()
        {
            _topLeftCorners = new List<TopLeftCorner>();
            _coeffs = new List<Tuple<double, double>>();
            for (double r = 0.1; r < 0.91; r += 0.1)
                for (double c = 0.1; c < 0.91; c += 0.1)
                    _coeffs.Add(new Tuple<double, double>(r, c));
            _neighBoors = new List<Position>();
            _neighBoors.Add(new Position(-1, 0));
            _neighBoors.Add(new Position(+1, 0));
            _neighBoors.Add(new Position(0, -1));
            _neighBoors.Add(new Position(0, +1));
        }

        public void LoadScreenShot(string fileName, string name)
        {
            _name = name;
            var image = Image.FromFile(fileName);
            var rawBitMap = new RawBitmap(image);
            _pixels = rawBitMap.ConvertDatas();
        }

        public void SaveScreenShot(string fileName)
        {
            Logger.Info($"Saving to bmp file: {fileName}");
            var rawBitMap = new RawBitmap(_pixels);
            rawBitMap.SaveToBmpFile(fileName);
        }

        private bool IsNearGrey(int i, int j, bool relative = false)
        {
            var p = _pixels[i, j];
            int d = Math.Abs(p.R - p.G) + Math.Abs(p.G - p.B) + Math.Abs(p.B - p.R);
            if (relative)
            {
                double sum = (double)p.R + (double)p.G + (double)p.B;
                if (sum < 0.5)
                    return true;
                else
                    return d / sum <= 15.0 / (3.0 * 128.0);
            }
            else
                return d <= 8;
        }

        private int GetBlackRows(int i, int j)
        {
            int k = 0;
            while (i + k < _pixels.GetLength(0) && IsNearGrey(i + k, j))
                k++;
            return k;
        }

        private int GetBlackColumns(int i, int j)
        {
            int k = 0;
            while (j + k < _pixels.GetLength(1) && IsNearGrey(i, j + k))
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
                    if (IsNearGrey(i, j))
                    {
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

        private void SetPixels(TopLeftCorner corner, RGB rgb, Position squarePosition)
        {
            int row = (int)(corner.Position.Row + squarePosition.Row * _squareHeight);
            int column = (int)(corner.Position.Column + squarePosition.Column * _squareWidth);

            for (int i = 0; i < _squareHeight; i++)
            {
                if (row + i >= 0 && row + i <= _pixels.GetLength(0) && column + i >= 0 && column + i <= _pixels.GetLength(1))
                {
                    _pixels[row + i, column + i] = rgb;
                    _pixels[row + i, column + _squareWidth - i] = rgb;
                }
            }
        }

        private int DistanceSample(TopLeftCorner corner, Position squarePos)
        {
            int distance = 0;
            for (int i = 0; i < _coeffs.Count; i++)
            {
                var rgb = GetPixel(corner, _coeffs[i], squarePos.Row, squarePos.Column);
//                Logger.Info($"To compare: {rgb} {corner.PixelSamples[i]}");
                if (rgb != null)
                    distance += corner.PixelSamples[i].N1(rgb);
                else
                    distance += int.MaxValue / (_coeffs.Count+1);
            }
            Logger.Info($"Distance {squarePos.Row} {squarePos.Column} = {distance}");
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

                var positions = new Dictionary<Position, bool>();
                positions.Add(new Position(0, 0), true);

                bool newPositionFound = false;
                do
                {
                    var newPositions = new Dictionary<Position, bool>();
                    foreach (var position in positions)
                    {
                        foreach (var neighBoor in _neighBoors)
                        {
                            var testPosition = position.Key + neighBoor;
                            if (DistanceSample(corner, testPosition) < 25 * _coeffs.Count)
                                if (!newPositions.ContainsKey(testPosition))
                                    newPositions.Add(testPosition, true);
                        }
                    }

                    //Report new positions
                    newPositionFound = false;
                    foreach (var newPosition in newPositions)
                    {
                        if (!positions.ContainsKey(newPosition.Key))
                        {
                            positions.Add(newPosition.Key, true);
                            newPositionFound = true;
                        }
                    }
                }
                while (newPositionFound);
                corner.SquarePositions = positions.Keys.ToList();
            }

            // Debug
            foreach (var corner in _topLeftCorners)
            {
                Logger.Info($"Corner: {corner}");
                SetPixels(corner, new RGB(255, 0, 0), new Position(0,0));
                foreach (var squarePosition in corner.SquarePositions)
                {
                    SetPixels(corner, new RGB(0, 0, 255), squarePosition);
                }
            }
            SaveScreenShot($@"c:\tmp\test_{_name}.bmp");
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
