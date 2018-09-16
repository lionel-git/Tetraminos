using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
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

        public static List<T>[] InitArrayList<T>(int n)
        {
            var arrayList = new List<T>[n];
            for (int i = 0; i < arrayList.Length; i++)
                arrayList[i] = new List<T>();
            return arrayList;
        }

        public static T[,] InitArrayD2<T>(int rows, int columns, T value = default(T))
        {
            var datas = new T[rows, columns];
            for (int i = 0; i < datas.GetLength(0); i++)
                for (int j = 0; j < datas.GetLength(1); j++)
                    datas[i, j] = value;
            return datas;
        }

        public static T[] InitArrayD1<T>(int length, T value = default(T))
        {
            var datas = new T[length];
            for (int i = 0; i < datas.GetLength(0); i++)
                    datas[i] = value;
            return datas;
        }

        public static string ToString2<T>(this T[,] datas, int rowBorder = 0, int columnBorder = 0)
        {
            var sb = new StringBuilder();
            for (int i = rowBorder; i < datas.GetLength(0) - rowBorder; i++)
            {
                for (int j = columnBorder; j < datas.GetLength(1) - columnBorder; j++)
                    sb.Append(" ").Append(datas[i, j]);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static string GenerateDatas(List<Position> positions, out int rows, out int columns)
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

            rows = maxRow - minRow + 1;
            columns = maxColumn - minColumn + 1;
            var buffer = InitArrayD1<byte>(rows * columns, (byte)Constants.FlagOff);
            foreach (var position in positions)
                buffer[(position.Row - minRow) * columns + (position.Column - minColumn)] = (byte)Constants.FlagOn;

            return Encoding.ASCII.GetString(buffer);
        }
    }
}
