using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitesLibrary.Implementations
{
    public class SaveMarkedCodesOptionFactory
    {
        private Enums.FileTypeEnum _fileType;

        public SaveMarkedCodesOptionFactory(Enums.FileTypeEnum fileType)
        {
            _fileType = fileType;
        }

        public Interfaces.ISaveMarkedCodes GetSaveMarkedCodesOption()
        {
            if (_fileType == Enums.FileTypeEnum.Pdf)
                return new PdfSaveMarkedCodes();
            else if (_fileType == Enums.FileTypeEnum.Svg)
                return new ImageSaveMarkedCodes();
            else if (_fileType == Enums.FileTypeEnum.Eps)
                return new EpsSaveMarkedCodes();//EpsInkscapeSaveMarkedCodes();
            else if (_fileType == Enums.FileTypeEnum.Csv)
                return new CsvSaveMarkedCodes();
            else if (_fileType == Enums.FileTypeEnum.Xlsx)
                return new XlsxSaveMarkedCodes();

            return null;
        }
    }
}
