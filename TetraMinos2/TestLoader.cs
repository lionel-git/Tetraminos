using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraMinos2
{
    public class TestLoader
    {
        private string _fileName;
        public TestLoader(string fileName)
        {
            _fileName = fileName;
        }

        public static Dictionary<char, Piece> LoadTest(string fileName, out Board board)
        {
            board = null;
            var pieces = new Dictionary<char, Piece>();
            using (var file = new StreamReader(fileName))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (!line.StartsWith("#"))
                    {
                        var tokens = line.Split(',');
                        switch (tokens[0].ToLower())
                        {
                            case "board":
                                board = new Board(int.Parse(tokens[1]), int.Parse(tokens[2]));
                                break;
                            case "piece":
                                var id = char.Parse(tokens[1]);
                                pieces.Add(id, new Piece(id, int.Parse(tokens[2]), int.Parse(tokens[3]), tokens[4]));
                                break;
                        }
                    }
                }
            }
            return pieces;
        }
    }
}