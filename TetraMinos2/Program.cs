using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraMinos2
{
    class Program
    {
        static readonly ILog Logger = LogManager.GetLogger("TetraMinos");

        static void Test0()
        {
            var board = new Board(5, 3);
            Logger.Info(board);
            Logger.Info(board.ToStringDebug());

            var pA = new Piece('A', 1, 3);
            Logger.Info(pA);

            var pB = new Piece('B', 2, 2);
            Logger.Info(pB);

            var pC = new Piece('C', 3, 2, "X.XX.X");
            Logger.Info(pC);

            board.UpdatePiece(pA, new Position(1, 0), Operation.Put, true);
            board.UpdatePiece(pB, new Position(2, 0), Operation.Put, true);
            Logger.Info(board);

            Logger.Info(board.SearchPositions(pA).ToString2());

            board.UpdatePiece(pA, new Position(1, 0), Operation.Remove, true);
            Logger.Info(board);

            var positions = board.SearchPositions(pC);
            board.UpdatePiece(pC, positions[0], Operation.Put, true);
            Logger.Info(board);
        }

        static void Test1()
        {
            Board board;
            var pieces = TestLoader.LoadTest("Test1.txt", out board);

            board.UpdatePiece(pieces['A'], new Position(1, 0), Operation.Put, true);
            Logger.Info(board);
            board.UpdatePiece(pieces['B'], new Position(2, 0), Operation.Put, true);
            Logger.Info(board);

            Logger.Info(board.SearchPositions(pieces['A']).ToString2());

            board.UpdatePiece(pieces['A'], new Position(1, 0), Operation.Remove, true);
            Logger.Info(board);

            var positions = board.SearchPositions(pieces['C']);
            board.UpdatePiece(pieces['C'], positions[0], Operation.Put, true);
            Logger.Info(board);

            board.TrySolve(pieces);
        }

        static void Test2()
        {
            Board board;
            var pieces = TestLoader.LoadTest("Moyen24.txt", out board);

            Logger.Info(board);
            foreach (var piece in pieces)
                Logger.Info(piece.Value.ToStringDebug());

            //board.UpdatePiece(pieces['A'], new Position(1, 0), Operation.Put, true);

            Logger.Info(board.ToStringDebug());
            board.TrySolve(pieces);

           
        
        }

        static void Main(string[] args)
        {
            try
            {
                Test2();
            }
            catch (TetraMinoException e)
            {
                Logger.Warn(e);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }
}
