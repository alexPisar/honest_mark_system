using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataMatrix.net;

namespace UtilitesLibrary.Service
{
    public class DataMatrixNetGenerator : Interfaces.IDataMatrixGenerator
    {
        public override byte[] GenerateAndSaveDataMatrix(string text, string fileName, string fileFolder, int width = 200, int height = 200)
        {
            DmtxImageEncoder encoder = new DmtxImageEncoder();

            var svgStr = encoder.EncodeSvgImage(text);
            System.IO.File.WriteAllText($"{fileFolder}\\{fileName}.svg", svgStr);
            return Encoding.UTF8.GetBytes(svgStr);
        }

        public override System.Drawing.Image GetImageByDataMatrix(string text, int width = 200, int height = 200)
        {
            DmtxImageEncoder encoder = new DmtxImageEncoder();
            return encoder.EncodeImage(text);
        }
    }
}
