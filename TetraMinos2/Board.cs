using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraMinos2
{
    public enum Operation { Put, Remove };

    public class Board
    {
        private int _rows;
        private int _columns;

        private int[,] _datas;

        private int[,] _n4;
        private int[,] _n8;

        public int Rows => _rows;
        public int Columns => _columns;

        public int this[int i, int j]
        {
            get
            {
                return _datas[_rows + i, _columns + j];
            }
            set
            {
                _datas[_rows + i, _columns + j] = value;
            }
        }

        public Board(int rows, int columns)
        {
            _rows = rows;
            _columns = columns;

            _datas = Helpers.InitArray(3 * rows, 3 * columns, Constants.Border);
            for (int i = 0; i < _rows; i++)
                for (int j = 0; j < _columns; j++)
                    this[i, j] = Constants.Empty;

            _n4 = Helpers.InitArray<int>(_rows + 2, _columns + 2);
            _n8 = Helpers.InitArray<int>(_rows + 2, _columns + 2);
            // A updater avec bordure!

        }

        private void UpdateNeighBoors(int row, int column, int increment)
        {
            _n4[row - 1, column] += increment;
            _n4[row + 1, column] += increment;
            _n4[row, column - 1] += increment;
            _n4[row, column + 1] += increment;

            _n8[row - 1, column - 1] += increment;
            _n8[row + 1, column + 1] += increment;
            _n8[row + 1, column - 1] += increment;
            _n8[row - 1, column + 1] += increment;
        }

        public void UpdatePiece(Piece piece, Position position, Operation operation, bool check)
        {
            int newValue = (operation == Operation.Put ? piece.Id : Constants.Empty);
            int oldValue = (operation == Operation.Put ? Constants.Empty : piece.Id);
            int incNeighboor = (operation == Operation.Put ? +1 : -1);

            for (int i = 0; i < piece.Rows; i++)
                for (int j = 0; j < piece.Columns; j++)
                    if (piece[i, j])
                    {
                        int row = position.Row + i;
                        int column = position.Column + j;
                        if (check && this[row, column] != oldValue)
                            throw new TetraMinoException($"Invalid operation '{operation}' with piece '{piece.Name}' on position {position}");
                        this[row, column] = newValue;
                        UpdateNeighBoors(1+row, 1+column, incNeighboor);
                    }
        }

        private bool IsCollision(int i, int j, Piece piece)
        {
            for (int k = 0; k < piece.Rows; k++)
                for (int l = 0; l < piece.Columns; l++)
                    if (this[i + k, j + l] != Constants.Empty && piece[k, l])
                        return true;
            return false;
        }

        public List<Position> SearchPositions(Piece piece)
        {
            var positions = new List<Position>();
            for (int i = 0; i <= Rows - piece.Rows; i++)
                for (int j = 0; j <= Columns - piece.Columns; j++)
                    if (!IsCollision(i, j, piece))
                        positions.Add(new Position(i, j));
            return positions;
        }

        public void TrySolve(List<Piece> pieces)
        {
            Console.WriteLine("Starting solve...");

            // Check some conditions
            int area = 0;
            int maxRows = 0;
            int maxColumns = 0;
            foreach (var piece in pieces)
            {
                area += piece.Area;
                maxRows = Math.Max(maxRows, piece.Rows);
                maxColumns = Math.Max(maxColumns, piece.Columns);
            }

            if (area != Rows * Columns)
                throw new TetraMinoException($"Area mismatch: pieces area = {area} != {Rows}*{Columns}");

            if (maxRows>Rows)
                throw new TetraMinoException($"A piece is too tall: {maxRows} > {Rows}");

            if (maxColumns > Columns)
                throw new TetraMinoException($"A piece is too large: {maxColumns} > {Columns}");

            Console.WriteLine("End solve.");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Board ({Rows},{Columns}):");
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                    sb.Append(" ").Append(Constants.ConvertCell(this[i, j]));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public string ToStringDebug()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Board Debug ({_datas.GetLength(0)},{_datas.GetLength(1)}):");
            for (int i = 0; i < _datas.GetLength(0); i++)
            {
                for (int j = 0; j < _datas.GetLength(1); j++)
                    sb.Append(" ").Append(Constants.ConvertCell(_datas[i, j]));
                sb.AppendLine();
            }
            sb.Append($"n4:\n{_n4.ToString2()}");
            sb.Append($"n8:\n{_n8.ToString2()}");
            return sb.ToString();
        }
    }
}
