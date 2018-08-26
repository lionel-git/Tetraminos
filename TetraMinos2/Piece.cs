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

        // Computed
        private int _area;
        public int Rows => _rows;
        public int Columns => _columns;

        public int Id => _id;

        public char Name => (char)_id;

        public int Area => _area;

        // Points sorted by N4
        private List<Point>[] _pointsN4;
        // Points sorted by N8
        private List<Point>[] _pointsN8;

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

        public Piece(char id, int rows, int columns, string datas = null)
        {
            _id = id;
            _rows = rows;
            _columns = columns;
            _datas = Helpers.InitArray(_rows, _columns, true);
            if (!string.IsNullOrEmpty(datas))
            {
                if (datas.Length != Rows * Columns)
                    throw new TetraMinoException($"Invalid init string '{datas}' ({Rows}*{Columns}");
                int k = 0;
                for (int i = 0; i < _rows; i++)
                    for (int j = 0; j < _columns; j++)
                        if (datas[k++] == 'X')
                            _datas[i, j] = true;
                        else
                            _datas[i, j] = false;
            }
            CheckConsistency();
            ComputeDatas();
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
                throw new TetraMinoException($"Piece '{Name}' is invalid");
            // Check if connex?

        }

        private List<Point>[] InitAList(int n)
        {
            var arrayList = new List<Point>[n];
            for (int i = 0; i < arrayList.Length; i++)
                arrayList[i] = new List<Point>();
            return arrayList;
        }

        private void ComputeDatas()
        {
            _pointsN4 = InitAList(Point.N4Size);
            _pointsN8 = InitAList(Point.N8Size);
            _area = 0;
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
                        _area++;
                    }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Piece {Name} ({Rows},{Columns}):");
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                    sb.Append(" ").Append(Constants.ConvertCell(this[i, j]));
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
