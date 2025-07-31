using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitesLibrary.Service.PdfContentTypes
{
    public class PdfImage : ModelBase.IPdfContent
    {
        public int UpperRightX { get; set; }
        public int UpperRightY { get; set; }
        public int LowerLeftX { get; set; }
        public int LowerLeftY { get; set; }
        public System.IO.Stream ImageStream { get; set; }
    }
}
