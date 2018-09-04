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
    // Raw bmp format:
    // https://en.wikipedia.org/wiki/BMP_file_format
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

        public RawBitmap(RGB[,] pixels)
        {
            int fileSize = 54 + 3 * pixels.GetLength(0) * pixels.GetLength(1);
            _datas = new byte[fileSize];

            SetString(0, "BM");
            SetInt(0x02, _datas.Length);
            SetInt(0x0A, 54);

            SetInt(0x0E, 40); // 54 = 0x0E + 40
            SetInt(0x12, pixels.GetLength(1));
            SetInt(0x16, pixels.GetLength(0));

            SetShort(0x1A, 1);
            SetShort(0x1C, 24);

            for (int i = 0; i < pixels.GetLength(0); i++) // Height
                for (int j = 0; j < pixels.GetLength(1); j++) // Width
                {
                    int offset = 54 + 3 * (i * pixels.GetLength(1) + j);
                    _datas[offset + 0] = pixels[pixels.GetLength(0) - 1 - i, j].B;
                    _datas[offset + 1] = pixels[pixels.GetLength(0) - 1 - i, j].G;
                    _datas[offset + 2] = pixels[pixels.GetLength(0) - 1 - i, j].R;
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

        private void SetInt(int offset, int value)
        {
            for (int i = 0; i < 4; i++)
                _datas[offset + i] = (byte)((value >> (8 * i)) & 0xFF);
        }

        private void SetShort(int offset, short value)
        {
            for (int i = 0; i < 2; i++)
                _datas[offset + i] = (byte)((value >> (8 * i)) & 0xFF);
        }

        private void SetString(int offset, string value)
        {
            for (int i = 0; i < value.Length; i++)
                _datas[offset + i] = (byte)value[i];
        }

        public void SaveToBmpFile(string fileName)
        {
            File.WriteAllBytes(fileName, _datas);
        }

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
