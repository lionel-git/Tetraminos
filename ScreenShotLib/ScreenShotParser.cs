using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using log4net;

namespace ScreenShotLib
{
    public class ScreenShotParser
    {
        static readonly ILog Logger = LogManager.GetLogger(nameof(ScreenShotParser));

        public ScreenShotParser()
        {
        }

        public void ParseFile(string fileName)
        {
            var data = File.ReadAllBytes(fileName);
            Logger.Info($"Size = {data.Length}");
        }
    }
}
