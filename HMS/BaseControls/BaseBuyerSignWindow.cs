using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HonestMarkSystem.BaseControls
{
    public abstract class BaseBuyerSignWindow : Window
    {
        protected string _filePath;
        protected byte[] _docSellerContent;

        public virtual byte[] DocSellerContent
        {
            get {
                if (_docSellerContent == null)
                    _docSellerContent = System.IO.File.ReadAllBytes(_filePath);

                return _docSellerContent;
            }
        }

        public virtual byte[] SellerSignature { get; set; }

        protected string GetSignatureStringForReport()
        {
            var signatureAsBase64 = Convert.ToBase64String(SellerSignature);
            return signatureAsBase64;
        }

        public abstract void SetDefaultParameters(Models.ConsignorOrganization organization, string subject, DataContextManagementUnit.DataAccess.Contexts.Abt.DocEdoPurchasing dataBaseObject, string edoProgramVersion);

        public abstract void OnAllPropertyChanged();

        public abstract Reporter.IReport Report { get; }

        public abstract string FileName { get; }

        public abstract bool IsMarked { get; }
    }
}
