using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmsQrCodesMakerApp.Models
{
    public class SavePrintDataMatrixModel : UtilitesLibrary.ModelBase.ViewModelBase
    {
        private string _changedFolderPath = null;
        public SavePrintDataMatrixModel(string orderId, string gtin, int quantity)
        {
            OrderId = orderId;
            Gtin = gtin;
            Quantity = quantity;

            SetFileTypes();
            SelectedFileType = null;
        }
        public string SavedFileName { get; set; }

        public string Gtin { get; set; }
        public int Quantity { get; set; }
        public string OrderId { get; set; }

        public List<KeyValuePair<UtilitesLibrary.Enums.FileTypeEnum, string>> FileTypes { get; set; }
        public UtilitesLibrary.Enums.FileTypeEnum? SelectedFileType { get; set; }

        public string FolderPath
        {
            get
            {
                if (_changedFolderPath == null)
                    return DefaultFolderPath;

                return _changedFolderPath;
            }
            set
            {
                _changedFolderPath = value;
                OnPropertyChanged("FolderPath");
            }
        }

        public string DefaultFolderPath
        {
            get
            {
                if(SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Svg ||
                    SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Eps)
                    return $"{UtilitesLibrary.Service.KnownFolders.GetPath(UtilitesLibrary.Enums.KnownFolder.Downloads)}\\order_{OrderId}_gtin_{Gtin}_quantity_{Quantity}";
                else
                    return UtilitesLibrary.Service.KnownFolders.GetPath(UtilitesLibrary.Enums.KnownFolder.Downloads);
            }
        }

        private void SetFileTypes()
        {
            FileTypes = new List<KeyValuePair<UtilitesLibrary.Enums.FileTypeEnum, string>>();

            FileTypes.Add(new KeyValuePair<UtilitesLibrary.Enums.FileTypeEnum, string>(UtilitesLibrary.Enums.FileTypeEnum.Pdf, "PDF (*.pdf)"));
            FileTypes.Add(new KeyValuePair<UtilitesLibrary.Enums.FileTypeEnum, string>(UtilitesLibrary.Enums.FileTypeEnum.Svg, "SVG (*.svg)"));
            FileTypes.Add(new KeyValuePair<UtilitesLibrary.Enums.FileTypeEnum, string>(UtilitesLibrary.Enums.FileTypeEnum.Eps, "EPS (*.eps)"));
            FileTypes.Add(new KeyValuePair<UtilitesLibrary.Enums.FileTypeEnum, string>(UtilitesLibrary.Enums.FileTypeEnum.Csv, "CSV (*.csv)"));
            FileTypes.Add(new KeyValuePair<UtilitesLibrary.Enums.FileTypeEnum, string>(UtilitesLibrary.Enums.FileTypeEnum.Xlsx, "XLSX (*.xlsx)"));
        }
    }
}
