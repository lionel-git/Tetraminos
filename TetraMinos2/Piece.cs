using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraMinos2
{
    public class Piece
    {
        // Inputs
        private int _id;
        private int _rows;
        private int _columns;
        private bool[,] _datas;

        private int _occurences;
        private int _current;

        // Computed
        private double _complexity;

        public int Rows => _rows;
        public int Columns => _columns;
        private int _hashCode;

        public int CurrentId => _id + _current;

        public int Area => _points.Count;

        public double Complexity => _complexity;

        public int Occurences => _occurences;

        // List of points from (0,0) in the piece
        private List<Point> _points;

        // Points sorted by N4
        private List<Point>[] _pointsN4;
        // Points sorted by N8
        private List<Point>[] _pointsN8;

        public List<Point> Points => _points;

        public List<Point>[] PointsN4 => _pointsN4;

        private char CurrentName
        {
            get
            {
                return (IsAvailable ? (char)(CurrentId) : '#');
            }
        }

        public const char FlagOn = 'X';

        public string Names
        {
            get
            {
                var available = (IsAvailable ? "" : "(N/A)");
                if (_occurences == 1)
                    return $"{available}{(char)_id}";
                else
                    return $"({available}{_occurences})[{(char)_id}-{(char)(_id + _occurences - 1)}] ";
            }
        }

        public bool IsAvailable => (_current < _occurences);

        public bool this[int i, int j]
        {
            get
            {
                return _datas[i, j];
            }
        }

        public int CheckPos(int i, int j)
        {
            if (i >= 0 && i < Rows && j >= 0 && j < Columns && _datas[i, j])
                return 1;
            else
                return 0;
        }

        public override bool Equals(object piece)
        {
            var rhs = piece as Piece;
            if (piece == null)
                throw new Exception($"Invalid comparison of piece with type: '{rhs.GetType()}'");

            if (GetHashCode() != rhs.GetHashCode())
                return false;

            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    if (CheckPos(i, j) != rhs.CheckPos(i, j))
                        return false;

            return true;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }


        public Piece(int id, int occurences, int rows, int columns, string datas = null)
        {
            _id = id;
            _rows = rows;
            _columns = columns;
            _occurences = occurences;
            _current = 0;
            _datas = Helpers.InitArray(_rows, _columns, true);
            if (!string.IsNullOrEmpty(datas))
            {
                if (datas.Length != Rows * Columns)
                    throw new TetraMinoException($"Invalid init string '{datas}' ({Rows}*{Columns}");
                int k = 0;
                for (int i = 0; i < _rows; i++)
                    for (int j = 0; j < _columns; j++)
                        if (datas[k++] == FlagOn)
                            _datas[i, j] = true;
                        else
                            _datas[i, j] = false;
            }
            CheckConsistency();
            ComputeDatas();
        }

        public void UpdateCurrentId(int inc)
        {
            _current += inc;
            // Valid range [ 0 - _occurences], _current=_occurences means the piece is no more available
            if (_current < 0 || _current > _occurences)
                throw new Exception($"Piece {this} invalid currentId: {_current}/{_occurences}");
        }

        private void CheckConsistency()
        {
            // At least one point should be set on each side
            bool left = false, right = false, top = false, bottom = false;
            for (int i = 0; i < _rows; i++)
            {
                left = left || this[i, 0];
                right = right || this[i, Columns - 1];
            }
            for (int j = 0; j < _columns; j++)
            {
                top = top || this[0, j];
                bottom = bottom || this[Rows - 1, j];
            }
            if (!left || !right || !top || !bottom)
                throw new TetraMinoException($"Piece '{Names}' is invalid");
            // Check if connex?

        }

        private int ComputeHashCode()
        {
            return (Rows << 0) ^ (Columns << 8) ^ (Area << 16) ^ TabHashing.Hash(_datas);
        }

        private double CalculateComplexity()
        {
            double diffHeightWidth = Math.Abs(Rows - Columns);
            double maxArea = Rows * Columns;
            double density = Math.Abs(Area / maxArea - 0.5);
            double n3 = PointsN4[3].Count;

            return  10.0 / (0.5 + density) + 2.0 * diffHeightWidth + 1.0 * maxArea;

        }

        private void ComputeDatas()
        {
            _points = new List<Point>();
            _pointsN4 = Helpers.InitAList(Point.N4Size);
            _pointsN8 = Helpers.InitAList(Point.N8Size);
            for (int i = 0; i < _rows; i++)
                for (int j = 0; j < _columns; j++)
                    if (this[i, j])
                    {
                        var point = new Point();
                        point.Position = new Position(i, j);
                        point.Neighboor4 = CheckPos(i, j - 1) + CheckPos(i, j + 1) + CheckPos(i - 1, j) + CheckPos(i + 1, j);
                        point.Neighboor8 = point.Neighboor4 
                            + CheckPos(i - 1, j - 1) + CheckPos(i + 1, j + 1) + CheckPos(i + 1, j - 1) + CheckPos(i - 1, j + 1);
                        _pointsN4[point.Neighboor4].Add(point);
                        _pointsN8[point.Neighboor8].Add(point);
                        _points.Add(point);
                    }
            _hashCode = ComputeHashCode();
            _complexity = CalculateComplexity();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Piece {Names} ({Rows},{Columns}) C={Complexity}:");
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    sb.Append(" ");
                    if (this[i, j])
                        sb.Append(CurrentName);
                    else
                        sb.Append(Constants.Off);
                }
                if (i < Rows - 1)
                    sb.AppendLine();
            }
            return sb.ToString();
        }

        public string ToStringDebug()
        {
            var sb = new StringBuilder();
            sb.AppendLine(ToString());
            sb.AppendLine($"area:  {Area}");
            for (int i = 0; i < _pointsN4.Length; i++)
            {
                sb.AppendLine($"With neighboor4 = {i}");
                foreach (var point in _pointsN4[i])
                    sb.AppendLine(point.ToString());
                sb.AppendLine("====");
            }
            return sb.ToString();
        }


    }
}
