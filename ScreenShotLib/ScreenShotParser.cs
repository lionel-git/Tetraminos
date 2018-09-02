using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using log4net;
using System.Drawing;
using System.Drawing.Imaging;

namespace ScreenShotLib
{
    public class ScreenShotParser
    {
        static readonly ILog Logger = LogManager.GetLogger(nameof(ScreenShotParser));

        // Pixels int=(r,g,b) Height(rows) x Width (columns)
        int[,] _rawDatas;

        public ScreenShotParser()
        {
        }

        public void LoadScreenShot(string fileName)
        {
            var image = Image.FromFile(fileName);
            var rawBitMap = new RawBitmap(image);
            _rawDatas = rawBitMap.ConvertDatas();
        }

        public override string ToString()
        {
            return $"H:{_rawDatas.GetLength(0)} x W:{_rawDatas.GetLength(1)}";
        }
    }
}
