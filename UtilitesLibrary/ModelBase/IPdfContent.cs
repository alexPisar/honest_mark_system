using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitesLibrary.ModelBase
{
    public abstract class IPdfContent
    {
        public Service.PdfContentTypes.Enums.PdfSizeType SizeType { get; set; }
    }
}
