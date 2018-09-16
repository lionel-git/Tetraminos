using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenShotLib
{
    public class RGB
    {
        private byte _r;
        private byte _g;
        private byte _b;

        public byte R => _r;
        public byte G => _g;
        public byte B => _b;

        public RGB(byte r, byte g, byte b)
        {
            _r = r;
            _g = g;
            _b = b;
        }

        public RGB(byte[] datas, int offset)
        {
            _b = datas[offset + 0];
            _g = datas[offset + 1];
            _r = datas[offset + 2];
        }

        public int N1(RGB rhs)
        {
            return Math.Abs(_r - rhs._r) + Math.Abs(_g - rhs._g) + Math.Abs(_b - rhs._b);
        }

        public RGB Normalize()
        {
            double n = _r + _g + _b;
            if (n > 0.5)
                return new RGB((byte)(255 * _r / n), (byte)(255 * _g / n), (byte)(255 * _b / n));
            else
                return null;
        }

        public override string ToString()
        {
            return $"({_r},{_g},{_b})";
        }
    }
}
