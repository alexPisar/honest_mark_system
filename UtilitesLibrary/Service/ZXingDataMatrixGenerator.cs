using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.Datamatrix;

namespace UtilitesLibrary.Service
{
    public class ZXingDataMatrixGenerator : Interfaces.IDataMatrixGenerator
    {
        public override System.Drawing.Image GenerateDataMatrix(string text, int width = 200, int height = 200)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.DATA_MATRIX,
                Options = new EncodingOptions
                {
                    Height = height,
                    Width = width,
                    Margin = 0,
                }
            };

            // Кодирование данных в формате GS1 DataMatrix.  
            // Важно правильно формировать строку для GS1. 
            // Обычно она начинается с AI (Application Identifier)
            // Например, для GTIN (01) и серийного номера (21): (01)01234567890128(21)1234567890
            var bitMap = writer.Write(text);
            return bitMap;
            //var encoded = writer.Encode(text);

            //if (encoded == null)
            //{
            //    return null; // Или обработайте ошибку
            //}

            //using (var stream = new System.IO.MemoryStream())
            //{
            //    writer.
            //    encoded.ToBitmap().Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            //    return stream.ToArray();
            //}
        }
    }
}
