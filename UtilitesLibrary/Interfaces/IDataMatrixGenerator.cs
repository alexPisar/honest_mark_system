using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitesLibrary.Interfaces
{
    public abstract class IDataMatrixGenerator
    {
        public abstract byte[] GenerateAndSaveDataMatrix(string text, string fileName, string fileFolder, int width = 200, int height = 200);
        public abstract System.Drawing.Image GetImageByDataMatrix(string text, int width = 200, int height = 200);

        public virtual System.IO.Stream GetImageStreamByDataMatrix(string text, int width = 200, int height = 200)
        {
            var img = GetImageByDataMatrix(text, width, height);
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            img.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

            return memoryStream;
        }

        public virtual bool[,] GetRawDataMatrixCode(string text)
        {
            return null;
        }
    }
}
