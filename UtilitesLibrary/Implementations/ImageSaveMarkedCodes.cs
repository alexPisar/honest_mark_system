using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitesLibrary.Implementations
{
    public class ImageSaveMarkedCodes : Interfaces.ISaveMarkedCodes
    {
        protected Interfaces.IDataMatrixGenerator _dataMatrixGenerator;
        public ImageSaveMarkedCodes()
        {
            _dataMatrixGenerator = new Service.DataMatrixNetGenerator();
        }

        public int? Index { get; set; }

        public virtual async Task SaveMarkedCodes(string pathFolder, string savedFileName, List<string> fullMarkedCodes)
        {
            if (fullMarkedCodes == null)
                return;

            if (Index == null)
                throw new Exception("Не задан индекс для сохранения.");

            int indx = Index.Value;
            foreach (var markedCode in fullMarkedCodes)
            {
                var markedCodeText = markedCode.Substring(0, markedCode.IndexOf((char)29));

                string indxStr = indx.ToString();

                if (indxStr.Length < 5)
                    indxStr = indxStr.PadLeft(5, '0');

                string fileName = $"{savedFileName}_{indxStr}";
                string svgFilePath = $"{pathFolder}\\{savedFileName}_{indxStr}.svg";

                await Task.Run(() => { _dataMatrixGenerator.GenerateAndSaveDataMatrix(markedCode, fileName, pathFolder, 300, 300); });

                ImageProcessSaveInNeedFormat(svgFilePath, pathFolder, savedFileName, indxStr);

                indx++;
            }
        }

        protected virtual void ImageProcessSaveInNeedFormat(string savedFilePath, string savingFilePathFolder, string savingFileName, string indxStr)
        {

        }
    }
}
