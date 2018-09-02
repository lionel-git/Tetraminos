using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScreenShotLib;

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

            var pA = new Piece('A', 1, 1, 3);
            Logger.Info(pA);

            var pB = new Piece('B', 1, 2, 2);
            Logger.Info(pB);

            var pC = new Piece('C', 1, 3, 2, "X.XX.X");
            Logger.Info(pC);

            board.UpdateBoard(pA, 1, 0, Operation.Put, true);
            board.UpdateBoard(pB, 2, 0, Operation.Put, true);
            Logger.Info(board);

            Logger.Info(board.SearchPositions(pA).ToString2());

            board.UpdateBoard(pA, 1, 0, Operation.Remove, true);
            Logger.Info(board);

            var positions = board.SearchPositions(pC);
            board.UpdateBoard(pC, positions[0].Row, positions[0].Column, Operation.Put, true);
            Logger.Info(board);
        }

        static void Test1()
        {
            Board board;
            var pieces = TestLoader.LoadTest("Test1.txt", out board);

            board.UpdateBoard(pieces['A'], 1, 0, Operation.Put, true);
            Logger.Info(board);
            board.UpdateBoard(pieces['B'], 2, 0, Operation.Put, true);
            Logger.Info(board);

            Logger.Info(board.SearchPositions(pieces['A']).ToString2());

            board.UpdateBoard(pieces['A'], 1, 0, Operation.Remove, true);
            Logger.Info(board);

            var positions = board.SearchPositions(pieces['C']);
            board.UpdateBoard(pieces['C'], positions[0].Row, positions[0].Column, Operation.Put, true);
            Logger.Info(board);

        //    board.TrySolve(pieces);

            var piecesD = board.LoadPiecesFromBoard();
            foreach (var pieceD in piecesD)
                Logger.Info($"{pieceD.Value} occurence of {pieceD.Key}");

        }

        static void Test1b()
        {
            Board board;
            var piecesD = TestLoader.LoadTest("Test2.txt", out board);

            foreach (var pieceD in piecesD)
                Logger.Info($"{pieceD.Value}");

            board.UpdateBoard(piecesD['A'], 1, 0, Operation.Put, true);
            Logger.Info(board);
            board.UpdateBoard(piecesD['A'], 2, 0, Operation.Put, true);
            Logger.Info(board);

            board.UpdateBoard(piecesD['A'], 2, 0, Operation.Remove, true);
            Logger.Info(board);

            board.UpdateBoard(piecesD['A'], 1, 0, Operation.Remove, true);
            Logger.Info(board);

            /*foreach (var piece in pieces)
                Logger.Info(piece.Value.ToStringDebug());
                */


            //            board.TrySolve(pieces);
        }

        static void Test2()
        {
            Board board;
            var pieces = TestLoader.LoadTest("Moyen205.txt", out board);

            Logger.Info(board);
            foreach (var piece in pieces)
                Logger.Info(piece.Value.ToStringDebug());

            var orderPieces = pieces.Values.OrderByDescending(x => x.Complexity);
            foreach (var piece in orderPieces)
                Logger.Info($"{piece.Names} => {piece.Complexity}");

            //board.UpdateBoard(pieces['A'], new Position(1, 0), Operation.Put, true);

            Logger.Info(board.ToStringDebug());
            board.TrySolve(pieces, true);
        }

        static void Test3()
        {
            foreach (var name in new List<string>() { "Moyen205", "Difficile35" })
            {
                var screenShotParser = new ScreenShotParser();
                var fileName = $@"ScreenShots\{name}.jpg";
                screenShotParser.LoadScreenShot(fileName, name);
                screenShotParser.SearchTopLeftAngle(70, 70);
                Logger.Info(screenShotParser);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                //Test1b(); return;
                //Test2();
                Test3();
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
