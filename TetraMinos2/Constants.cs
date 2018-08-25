using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraMinos2
{
    public static class Constants
    {
        public const int Empty = -1;
        public const int Border = -2;

        const string On = "X";
        const string Off = ".";

        public static string ConvertCell(int v)
        {
            switch (v)
            {
                case Border:
                    return On;
                case Empty:
                    return Off;
                default:
                    return char.ConvertFromUtf32(v);
            }
        }

        public static string ConvertCell(bool b)
        {
            return (b ? On : Off);
        }
    }
}
