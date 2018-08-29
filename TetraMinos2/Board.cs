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

        private bool _solveForOne;
        private int _solutions;

        private int _rows;
        private int _columns;

        private int[,] _datas;

        // Neighboors up, down, left, right
        private int[,] _n4C;
        // Neighboors diags
        private int[,] _n4D;

        public int Rows => _rows;
        public int Columns => _columns;

        // Stats
        private UInt64 _callsSolve;
        private UInt64 _callsUpdateBoard;
        private UInt64 _callsUpdateNeighBoor;

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
            _callsUpdateNeighBoor++;
            _n4C[1 + row - 1, 1 + column + 0] += increment;
            _n4C[1 + row + 1, 1 + column + 0] += increment;
            _n4C[1 + row + 0, 1 + column - 1] += increment;
            _n4C[1 + row + 0, 1 + column + 1] += increment;

            _n4D[1 + row - 1, 1 + column - 1] += increment;
            _n4D[1 + row + 1, 1 + column + 1] += increment;
            _n4D[1 + row + 1, 1 + column - 1] += increment;
            _n4D[1 + row - 1, 1 + column + 1] += increment;
        }

        public void UpdateBoard(Piece piece, int boardRow, int boardColumn, Operation operation, bool check)
        {
            _callsUpdateBoard++;
            int newValue, oldValue, increment;
            switch (operation)
            {
                case Operation.Put:
                    newValue = piece.CurrentId;
                    oldValue = Constants.Empty;
                    increment = +1;
                    break;

                case Operation.Remove:
                    newValue = Constants.Empty;
                    oldValue = piece.CurrentId - 1; // The previous current Id
                    increment = -1;
                    break;

                default:
                    throw new Exception($"Invalid operation: {operation}");
            }

            foreach (var p in  piece.Points)
            {
                int row = boardRow + p.Position.Row;
                int column = boardColumn + p.Position.Column;
                if (check && this[row, column] != oldValue)
                    throw new TetraMinoException($"Invalid operation '{operation}' with piece '{piece.Names}' on position ({boardRow},{boardColumn})");
                this[row, column] = newValue;
                UpdateNeighBoors(row, column, increment);
            }

            piece.UpdateCurrentId(increment);
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
            var position = new Position(int.MinValue, int.MinValue);
            int maxDifficulty = int.MinValue;
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    if (this[i, j] == Constants.Empty)
                    {
                        int difficulty = 2 * _n4C[1 + i, 1 + j] + _n4D[1 + i, 1 + j];
                        if (difficulty > maxDifficulty)
                        {
                            position.Row = i;
                            position.Column = j;
                            maxDifficulty = difficulty;
                        }
                    }
            return position;
        }

        // That pieces on the board match original list
        private string CheckResult(List<Piece> pieces)
        {
            var piecesD = LoadPiecesFromBoard();
            foreach (var piece in pieces)
            {
                int occurences;
                if (piecesD.TryGetValue(piece, out occurences))
                {
                    if (occurences == piece.Occurences)
                        piecesD.Remove(piece);
                    else
                        return $"Mismatch occurences on piece {piece}/{occurences}";                        
                }
                else
                    return $"Piece missing on board : {piece}";
            }
            if (piecesD.Count != 0)
                return $"{pieces.Count} pieces are in suplus on board";
            return null;
        }

        // Recursive search, Algo:
        // Sort free positions by decreasing complexity (n4C + n4D high)
        // for a position, sort pieces by complexity (presorted, sorted dictionnary?)
        // for a piece, find a possible point : point.neighboor + position.neighboor <= MaxNeighboors
        // Check Collision
        // if ok => Place piece + recurse
        // Recurse succeed => return true
        // Recurse fails => next point/piece
        public bool Solve(List<Piece> pieces)
        {
            _callsSolve++;
            if (Logger.IsDebugEnabled)
                Logger.Debug($"{pieces.Where(x => x.IsAvailable).Count()} pieces, solve for {ToStringDebug()}");
            if (pieces.Count == 0)
            {
                Logger.Info($"Solution found:\n{ToString()}");
                var message = CheckResult(pieces);
                if (string.IsNullOrEmpty(message))
                    Logger.Info("Solution is verified....");
                else
                    Logger.Info($"Solution is incorrect: {message}");
                _solutions++;
                return true;
            }
            else
            {
                var position = SearchMostDifficult();
                int maxN4C = 4 - _n4C[1 + position.Row, 1 + position.Column];
                int maxN4D = 4 - _n4D[1 + position.Row, 1 + position.Column];
                if (Logger.IsDebugEnabled)
                    Logger.Debug($"Position = {position} maxN4C={maxN4C} maxN4D={maxN4D}");

                // Iterer sur les pieces + points
                // attention on ne peut pas iterer sur collection modifiee
                foreach (var piece in pieces)
                {
                    if (piece.IsAvailable)
                    {
                        // Check available points de la piece avec maxN4D/maxN4C
                        // tester par ordre decroissant maxN4D jusqu'a 0                        
                        int rowPoint = 0;
                        int columnPoint = 0;

                        int row = position.Row + rowPoint;
                        int column = position.Column + columnPoint;
                        if (!IsCollision(row, column, piece))
                        {
                            UpdateBoard(piece, row, column, Operation.Put, true);
                            if (Solve(pieces) && _solveForOne)
                                return true;
                            UpdateBoard(piece, row, column, Operation.Remove, true);
                        }
                    }
                }
                return false;
            }
        }

        private void ResetStats()
        {
            _callsSolve = 0;
            _callsUpdateBoard = 0;
            _callsUpdateNeighBoor = 0;
        }

        public void TrySolve(Dictionary<char, Piece> pieces, bool solveForOne = true)
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

            // Check all pieces are differents
            var pieceList = pieces.Values.ToList();
            for (int i = 0; i < pieceList.Count; i++)
                for (int j = i + 1; j < pieceList.Count; j++)
                    if (pieceList[i].Equals(pieceList[j]))
                        throw new TetraMinoException($"Two pieces are identical: {pieceList[i]} and {pieceList[j]}");

            _solveForOne = solveForOne;
            ResetStats();
            Solve(pieces.Values.OrderByDescending(x => x.Complexity).ToList());

            Logger.Info($"End solve, solutions: {_solutions} , solForOne={_solveForOne}");
            Logger.Info($"Stats: #Solve={_callsSolve} #UpdateBoard={_callsUpdateBoard} #UpdateNeighBoor={_callsUpdateNeighBoor}");
        }

        private static Piece GeneratePiece(int id, List<Position> positions)
        {
            int minRow = int.MaxValue;
            int maxRow = int.MinValue;
            int minColumn = int.MaxValue;
            int maxColumn = int.MinValue;
            foreach (var position in positions)
            {
                minRow = Math.Min(minRow, position.Row);
                maxRow = Math.Max(maxRow, position.Row);
                minColumn = Math.Min(minColumn, position.Column);
                maxColumn = Math.Max(maxColumn, position.Column);
            }

            int rows = maxRow - minRow + 1;
            int columns = maxColumn - minColumn + 1;
            byte[] buffer = new byte[rows * columns];
            foreach (var position in positions)
                buffer[(position.Row - minRow) * columns + (position.Column - minColumn)] = (byte)Piece.FlagOn;

            return new Piece(id, 1, maxRow - minRow + 1, maxColumn - minColumn + 1, Encoding.ASCII.GetString(buffer));
        }

        // For checking: reload list of pieces from board
        public Dictionary<Piece, int> LoadPiecesFromBoard()
        {
            // First gather id and positions
            var piecesPos = new Dictionary<int, List<Position>>();
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                {
                    int id = this[i, j];
                    if (id != Constants.Empty)
                    {
                        List<Position> positions;
                        if (!piecesPos.TryGetValue(id, out positions))
                        {
                            positions = new List<Position>();
                            piecesPos.Add(id, positions);
                        }
                        positions.Add(new Position(i, j));
                    }
                }

            var pieces = new Dictionary<Piece, int>();
            foreach (var piecePos in piecesPos)                
            {
                var piece = GeneratePiece(piecePos.Key, piecePos.Value);
                if (pieces.ContainsKey(piece))
                    pieces[piece]++;
                else
                    pieces.Add(piece, 1);
            }

            return pieces;
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

        public string ToStringDebug(bool completeBoard = false)
        {
            var sb = new StringBuilder();
            if (completeBoard)
            {
                sb.AppendLine($"Board Debug ({_datas.GetLength(0)},{_datas.GetLength(1)}):");
                for (int i = 0; i < _datas.GetLength(0); i++)
                {
                    for (int j = 0; j < _datas.GetLength(1); j++)
                        sb.Append(" ").Append(Constants.ConvertCell(_datas[i, j]));
                    sb.AppendLine();
                }
            }
            else
                sb.Append(ToString());
            sb.Append($"n4C:\n{_n4C.ToString2(1,1)}");
            sb.Append($"n4D:\n{_n4D.ToString2(1,1)}");
            return sb.ToString();
        }
    }
}
