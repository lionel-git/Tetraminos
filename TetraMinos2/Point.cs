using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraMinos2
{
    // Contains some informations about a point in a piece
    public class Point
    {
        public const int N4Size = 4 + 1; // 0 to 4
        public const int N8Size = 8 + 1; // 0 to 8

        // Position of point inside piece (from top left = 0,0)
        public Position Position { get; set; }
        
        // neighboor up, down left, right
        public int Neighboor4 { get; set; }

        // neighboor4 + 4 on diags
        public int Neighboor8 { get; set; }

        public override string ToString()
        {
            return $"{Position} => {Neighboor4} {Neighboor8}";
        }
    }
}
