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

        public static T[,] InitArray<T>(int rows, int columns, T value = default(T))
        {
            var datas = new T[rows, columns];
            for (int i = 0; i < datas.GetLength(0); i++)
                for (int j = 0; j < datas.GetLength(1); j++)
                    datas[i, j] = value;
            return datas;
        }

        public static string ToString2<T>(this T[,] datas)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < datas.GetLength(0); i++)
            {
                for (int j = 0; j < datas.GetLength(1); j++)
                    sb.Append(" ").Append(datas[i, j]);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
