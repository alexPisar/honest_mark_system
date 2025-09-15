using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.ModelBase;

namespace UtilitesLibrary.Implementations
{
    public class PdfSaveMarkedCodes : Interfaces.ISaveMarkedCodes
    {
        Interfaces.IDataMatrixGenerator _dataMatrixGenerator;
        Interfaces.IPdfFileWorker _pdfWorker;

        public PdfSaveMarkedCodes()
        {
            _dataMatrixGenerator = new Service.DataMatrixNetGenerator();
            _pdfWorker = new Service.PDFsharpWorker();
        }

        public PdfSaveMarkedCodes(Interfaces.IDataMatrixGenerator dataMatrixGenerator)
        {
            _dataMatrixGenerator = dataMatrixGenerator;
            _pdfWorker = new Service.PDFsharpWorker();
        }

        public async Task SaveMarkedCodes(string pathFolder, string savedFileName, List<string> fullMarkedCodes)
        {
            if (fullMarkedCodes == null)
                return;

            var pdfPages = new List<PdfPage>();

            foreach (var markedCode in fullMarkedCodes)
            {
                var markedCodeText = markedCode.Substring(0, markedCode.IndexOf((char)29));
                var imageStream = _dataMatrixGenerator.GetImageStreamByDataMatrix(markedCode, 300, 300);

                var pdfPage = new PdfPage()
                {
                    SizeType = Service.PdfContentTypes.Enums.PdfSizeType.Millimeter,
                    Height = 25,
                    Width = 43,
                    Contents = new List<IPdfContent>(new IPdfContent[] {
                                new Service.PdfContentTypes.PdfImage
                                {
                                    SizeType = Service.PdfContentTypes.Enums.PdfSizeType.Millimeter,
                                    LowerLeftX = 1,
                                    LowerLeftY = 2,
                                    UpperRightX = 22,
                                    UpperRightY = 23,
                                    ImageStream = imageStream
                                },
                                new Service.PdfContentTypes.PdfText
                                {
                                    SizeType = Service.PdfContentTypes.Enums.PdfSizeType.Millimeter,
                                    LowerLeftX = 23,
                                    LowerLeftY = 4,
                                    UpperRightX = 41,
                                    UpperRightY = 7,
                                    Text = markedCodeText.Substring(0, 12),
                                    FontFamily = "Arial",
                                    FontSize = 7,
                                    Bold = true
                                },
                                new Service.PdfContentTypes.PdfText
                                {
                                    SizeType = Service.PdfContentTypes.Enums.PdfSizeType.Millimeter,
                                    LowerLeftX = 23,
                                    LowerLeftY = 7,
                                    UpperRightX = 41,
                                    UpperRightY = 10,
                                    Text = markedCodeText.Substring(12, 12),
                                    FontFamily = "Arial",
                                    FontSize = 7,
                                    Bold = true
                                },
                                new Service.PdfContentTypes.PdfText
                                {
                                    SizeType = Service.PdfContentTypes.Enums.PdfSizeType.Millimeter,
                                    LowerLeftX = 23,
                                    LowerLeftY = 10,
                                    UpperRightX = 41,
                                    UpperRightY = 13,
                                    Text = markedCodeText.Substring(24),
                                    FontFamily = "Arial",
                                    FontSize = 7,
                                    Bold = true
                                }
                            })
                };

                pdfPages.Add(pdfPage);
            }

            var pdfBytes = await _pdfWorker.GetPdfFileFromContents(pdfPages);
            System.IO.File.WriteAllBytes($"{pathFolder}\\{savedFileName}.pdf", pdfBytes);
        }
    }
}
