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
        private int[,] _datas;

        // Computed
        private List<Point> _points;

        public int Rows => _rows;
        public int Columns => _columns;

        public int Id => _id;

        public char Name => (char)_id;

        public int Area => _points.Count;

        public int this[int i, int j]
        {
            get
            {
                return _datas[i, j];
            }
        }

        public Piece(char id, int rows, int columns, string datas = null)
        {
            _id = id;
            _rows = rows;
            _columns = columns;
            _datas = new int[_rows, _columns];
            for (int i = 0; i < _rows; i++)
                for (int j = 0; j < _columns; j++)
                    _datas[i, j] = _id;
            if (!string.IsNullOrEmpty(datas))
            {
                if (datas.Length != Rows * Columns)
                    throw new Exception($"Invalid init string '{datas}' ({Rows}*{Columns}");
                int k = 0;
                for (int i = 0; i < _rows; i++)
                    for (int j = 0; j < _columns; j++)
                        if (datas[k++] == 'X')
                            _datas[i, j] = _id;
                        else
                            _datas[i, j] = Constants.Empty;
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
                left = left || this[i, 0] == _id;
                right = right || this[i, Columns - 1] == _id;
            }
            for (int j = 0; j < _columns; j++)
            {
                top = top || this[0, j] == _id;
                bottom = bottom || this[Rows - 1, j] == _id;
            }
            if (!left || !right || !top || !bottom)
                throw new Exception($"Piece '{Name}' is invalid");
            // Check if connex?

        }

        private void ComputeDatas()
        {
            _points = new List<Point>();
            for (int i = 0; i < _rows; i++)
                for (int j = 0; j < _columns; j++)
                    if (this[i, j] == _id)
                    {
                        var point = new Point();
                        point.Position = new Position(i, j);
                        
                        
                            
                        _points.Add(point);
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

    }
}
