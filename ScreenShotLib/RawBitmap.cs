using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace ScreenShotLib
{
    public class RawBitmap
    {
        private byte[] _datas;

        public RawBitmap(Image image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Bmp);
                _datas = stream.ToArray();
            }
        }

        private int GetShort(int offset)
        {
            long r = (_datas[offset + 0] << 0) + (_datas[offset + 1] << 8);
            return (int)r;
        }

        private int GetInt(int offset)
        {
            long r = (_datas[offset + 0] << 0) + (_datas[offset + 1] << 8) + (_datas[offset + 2] << 16) + (_datas[offset + 3] << 24);
            return (int)r;
        }

        //cf https://en.wikipedia.org/wiki/BMP_file_format
        public RGB[,] ConvertDatas()
        {
            string type = $"{(char)_datas[0]}{(char)_datas[1]}";
            int fileSize = GetInt(0x02);
            int offsetDatas = GetInt(0x0A);

            var sizeHeader = GetInt(0x0E); // Should be 40
            var width = GetInt(0x12);
            var height = GetInt(0x16);

            var colorPlanes = GetShort(0x1A); // 1
            var bpp = GetShort(0x1C); // 24 bits per pixel (R,G,B)
            var compMethod = GetInt(0x1E); // 0 = None
            var imgSize = GetInt(0x22); // 0
            var colors = GetInt(0x2E); // 0

            if (bpp != 24)
                throw new Exception($"Only bpp=24 is handled (bpp={bpp})");

            if (compMethod != 0)
                throw new Exception($"Only uncompress file is handled (compMethod={compMethod})");

            if (width % 4 != 0)
                throw new Exception("Width is not a multiple of 4");

            if (fileSize - offsetDatas != width * height * 3)
                throw new Exception("Format pb ?");

            // Bmp store image from bottom
            var rawDatas = new RGB[height, width];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    rawDatas[height - 1 - i, j] = new RGB(_datas, offsetDatas + 3 * (i * width + j));
            return rawDatas;
        }
    }
}
