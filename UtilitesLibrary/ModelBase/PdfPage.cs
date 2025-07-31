using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitesLibrary.ModelBase
{
    public class PdfPage
    {
        public PdfPage()
        {
            SizeType = Service.PdfContentTypes.Enums.PdfSizeType.None;
        }

        public Service.PdfContentTypes.Enums.PdfSizeType SizeType { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
        public List<IPdfContent> Contents { get; set; }
    }
}
