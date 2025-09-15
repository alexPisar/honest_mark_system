using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmsQrCodesMakerApp.Models
{
    public class SavePrintDataMatrixModel : UtilitesLibrary.ModelBase.ViewModelBase
    {
        public SavePrintDataMatrixModel(string defaultSavedFileName)
        {
            SavedFileName = defaultSavedFileName;
            SetFileTypes();
            SelectedFileType = null;
        }
        public string SavedFileName { get; set; }

        public List<KeyValuePair<UtilitesLibrary.Enums.FileTypeEnum, string>> FileTypes { get; set; }
        public UtilitesLibrary.Enums.FileTypeEnum? SelectedFileType { get; set; }

        public string FolderPath { get; set; }

        private void SetFileTypes()
        {
            FileTypes = new List<KeyValuePair<UtilitesLibrary.Enums.FileTypeEnum, string>>();

            FileTypes.Add(new KeyValuePair<UtilitesLibrary.Enums.FileTypeEnum, string>(UtilitesLibrary.Enums.FileTypeEnum.Pdf, "PDF (*.pdf)"));
            FileTypes.Add(new KeyValuePair<UtilitesLibrary.Enums.FileTypeEnum, string>(UtilitesLibrary.Enums.FileTypeEnum.Svg, "SVG (*.svg)"));
            FileTypes.Add(new KeyValuePair<UtilitesLibrary.Enums.FileTypeEnum, string>(UtilitesLibrary.Enums.FileTypeEnum.Eps, "Encapsulared PostScript(*.eps)"));
            FileTypes.Add(new KeyValuePair<UtilitesLibrary.Enums.FileTypeEnum, string>(UtilitesLibrary.Enums.FileTypeEnum.Csv, "CSV File(*.csv)"));
        }
    }
}
