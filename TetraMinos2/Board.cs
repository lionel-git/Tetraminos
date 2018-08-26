using log4net;
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
        static readonly ILog Logger = LogManager.GetLogger(nameof(Board));

        private int _rows;
        private int _columns;

        private int[,] _datas;

        // Neighboors up, down, left, right
        private int[,] _n4C;
        // Neighboors diags
        private int[,] _n4D;

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

        public int Check(int i, int j)
        {
            return (this[i, j] == Constants.Empty ? 0 : 1);
        }

        public Board(int rows, int columns)
        {
            _rows = rows;
            _columns = columns;

            _datas = Helpers.InitArray(3 * rows, 3 * columns, Constants.Border);
            for (int i = 0; i < _rows; i++)
                for (int j = 0; j < _columns; j++)
                    this[i, j] = Constants.Empty;

            _n4C = Helpers.InitArray<int>(_rows + 2, _columns + 2);
            _n4D = Helpers.InitArray<int>(_rows + 2, _columns + 2);
            for (int i = 0; i < _rows; i++)
                for (int j = 0; j < _columns; j++)
                {
                    _n4C[1 + i, 1 + j] = Check(i + 1, j) + Check(i - 1, j) + Check(i, j + 1) + Check(i, j - 1);
                    _n4D[1 + i, 1 + j] = Check(i + 1, j + 1) + Check(i - 1, j - 1) + Check(i - 1, j + 1) + Check(i + 1, j - 1);
                }
        }

        private void UpdateNeighBoors(int row, int column, int increment)
        {
            _n4C[1 + row - 1, 1 + column + 0] += increment;
            _n4C[1 + row + 1, 1 + column + 0] += increment;
            _n4C[1 + row + 0, 1 + column - 1] += increment;
            _n4C[1 + row + 0, 1 + column + 1] += increment;

            _n4D[1 + row - 1, 1 + column - 1] += increment;
            _n4D[1 + row + 1, 1 + column + 1] += increment;
            _n4D[1 + row + 1, 1 + column - 1] += increment;
            _n4D[1 + row - 1, 1 + column + 1] += increment;
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
                        UpdateNeighBoors(row, column, incNeighboor);
                    }
        }

        private bool IsCollision(int row, int column, Piece piece)
        {
            for (int k = 0; k < piece.Rows; k++)
                for (int l = 0; l < piece.Columns; l++)
                    if (piece[k, l] && this[row + k, column + l] != Constants.Empty)
                        return true;
            return false;
        }

        [Obsolete]
        public List<Position> SearchPositions(Piece piece)
        {
            var positions = new List<Position>();
            for (int i = 0; i <= Rows - piece.Rows; i++)
                for (int j = 0; j <= Columns - piece.Columns; j++)
                    if (!IsCollision(i, j, piece))
                        positions.Add(new Position(i, j));
            return positions;
        }


        // Find free position on board with high n4C/n4D
        private Position SearchMostDifficult()
        {



            return new Position(0, 0);
        }


        // Recursive search, Algo:
        // Sort free positions by decreasing complexity (n4C + n4D high)
        // for a position, sort pieces by complexity (presorted, sorted dictionnary?)
        // for a piece, find a possible point : point.neighboor + position.neighboor <= MaxNeighboors
        // Check Collision
        // if ok => Place piece + recurse
        // Recurse succeed => return true
        // Recurse fails => next point/piece
        public bool Solve(Dictionary<char, Piece> pieces)
        {
            Logger.Debug($"{pieces.Count} pieces, solve for {ToStringDebug()}");
            if (pieces.Count == 0)
            {
                Logger.Info("Solution found !!");
                Logger.Info(ToString());
                return true;
            }
            else
            {
                var position = SearchMostDifficult();

                // Iterer sur les pieces + points
                // attention on ne peut pas iterer sur collection modifiee



                var piece = pieces.First();
                {
                    // Check available points


                    // Point de la piece
                    int rowPoint = 0;
                    int columnPoint = 0;

                    int row = position.Row + rowPoint;
                    int column = position.Column + columnPoint;
                    if (!IsCollision(row, column, piece.Value))
                    {
                        pieces.Remove(piece.Key);
                        UpdatePiece(piece.Value, new Position(row, column), Operation.Put, true);
                        if (Solve(pieces))
                            return true;
                        else
                        {
                            UpdatePiece(piece.Value, new Position(row, column), Operation.Remove, true);
                            pieces.Add(piece.Key, piece.Value);
                        }
                    }
                }
                return false;
            }
        }

        public void TrySolve(Dictionary<char, Piece> pieces)
        {
            Logger.Info("Starting solve...");

            // Check some conditions
            int area = 0;
            int maxRows = 0;
            int maxColumns = 0;
            foreach (var piece in pieces.Values)
            {
                area += piece.Area;
                maxRows = Math.Max(maxRows, piece.Rows);
                maxColumns = Math.Max(maxColumns, piece.Columns);
            }

            if (area != Rows * Columns)
                throw new TetraMinoException($"Area mismatch: pieces area = {area} != {Rows}*{Columns}");

            if (maxRows > Rows)
                throw new TetraMinoException($"A piece is too tall: {maxRows} > {Rows}");

            if (maxColumns > Columns)
                throw new TetraMinoException($"A piece is too large: {maxColumns} > {Columns}");

            Solve(pieces);

            Logger.Info("End solve.");
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
            sb.Append($"n4C:\n{_n4C.ToString2(1,1)}");
            sb.Append($"n4D:\n{_n4D.ToString2(1,1)}");
            return sb.ToString();
        }
    }
}
