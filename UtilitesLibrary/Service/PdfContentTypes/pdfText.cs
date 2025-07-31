using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitesLibrary.Service.PdfContentTypes
{
    public class PdfText : ModelBase.IPdfContent
    {
        public int UpperRightX { get; set; }
        public int UpperRightY { get; set; }
        public int LowerLeftX { get; set; }
        public int LowerLeftY { get; set; }
        public string Text { get; set; }
        public string FontFamily { get; set; }
        public int FontSize { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool UnderLine { get; set; }
        public bool Strikeout { get; set; }
    }
}
