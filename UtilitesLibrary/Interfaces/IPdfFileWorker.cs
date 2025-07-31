using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitesLibrary.Interfaces
{
    public abstract class IPdfFileWorker
    {
        public abstract byte[] GetPdfFileFromContents(List<ModelBase.PdfPage> pdfPages, string filePath);
    }
}
