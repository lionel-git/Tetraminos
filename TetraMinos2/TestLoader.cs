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
        const string BaseDir = "Tests";

        public static Dictionary<char, Piece> LoadTest(string fileName, out Board board)
        {
            board = null;
            var pieces = new Dictionary<char, Piece>();
            char? id = null;
            using (var file = new StreamReader(Path.Combine(BaseDir, fileName)))
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
                            case "name":
                                if (id != null)
                                    throw new TetraMinoException($"Name already declared in file {fileName}");
                                id = char.Parse(tokens[1]);
                                break;
                            case "piece":
                                int occurences = int.Parse(tokens[1]);
                                pieces.Add(id.Value, new Piece(id.Value, int.Parse(tokens[1]), int.Parse(tokens[2]), int.Parse(tokens[3]), tokens[4]));
                                id = (char)((byte)id + occurences);
                                break;
                        }
                    }
                }
            }
            return pieces;
        }
    }
}