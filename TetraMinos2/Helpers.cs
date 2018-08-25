using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraMinos2
{
    public static class Helpers
    {
        public static string ToString2(this List<Position> list)
        {
            var sb = new StringBuilder();
            foreach (var item in list)
                sb.Append(item).Append(" ");
            return sb.ToString();
        }
    }
}
