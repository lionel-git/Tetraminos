using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraMinos2
{
    public static class TabHashing
    {
        private static int[,] _xorTable;
        const int MaxDatas = 65536;

        public static int Hash(bool[,] datas)
        {
            if (datas.GetLength(0) * datas.GetLength(1) > MaxDatas)
                throw new Exception($"Two many datas to hash: {datas.GetLength(0) * datas.GetLength(1)}");

            if (_xorTable == null)
            {
                _xorTable = new int[2, MaxDatas];
                Random rnd = new Random();
                for (int i = 0; i < MaxDatas; i++)
                {
                    _xorTable[0, i] = rnd.Next(int.MinValue, int.MaxValue);
                    _xorTable[1, i] = rnd.Next(int.MinValue, int.MaxValue);
                }
            }

            int hashCode = 0;
            int counter = 0;
            for (int i = 0; i < datas.GetLength(0); i++)
                for (int j = 0; j < datas.GetLength(1); j++)
                    hashCode ^= _xorTable[datas[i, j] ? 1 : 0, counter++];
            return hashCode;
        }
    }
}
