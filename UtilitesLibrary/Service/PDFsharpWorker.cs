using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.ModelBase;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;

namespace UtilitesLibrary.Service
{
    public class PDFsharpWorker : Interfaces.IPdfFileWorker
    {
        public override byte[] GetPdfFileFromContents(List<ModelBase.PdfPage> pdfPages, string filePath)
        {
            byte[] pdfBytes = null;
            using (var document = new PdfDocument())
            {
                foreach (var pdfPage in pdfPages)
                {
                    var page = document.AddPage();
                    //page.Orientation = PdfSharp.PageOrientation.Portrait;
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    if (pdfPage.Height != null && pdfPage.Width != null)
                    {
                        XGraphicsUnit sizeType;

                        switch (pdfPage.SizeType)
                        {
                            case PdfContentTypes.Enums.PdfSizeType.Inch:
                                sizeType = XGraphicsUnit.Inch;
                                break;
                            case PdfContentTypes.Enums.PdfSizeType.Pixel:
                                sizeType = XGraphicsUnit.Point;
                                break;
                            case PdfContentTypes.Enums.PdfSizeType.Millimeter:
                                sizeType = XGraphicsUnit.Millimeter;
                                break;
                            case PdfContentTypes.Enums.PdfSizeType.Centimeter:
                                sizeType = XGraphicsUnit.Centimeter;
                                break;
                            default:
                                sizeType = XGraphicsUnit.Point;
                                break;
                        }

                        page.Height = new XUnit(pdfPage.Height.Value, sizeType);
                        page.Width = new XUnit(pdfPage.Width.Value, sizeType);
                    }

                    foreach (var content in pdfPage.Contents)
                    {
                        if (content as PdfContentTypes.PdfImage != null)
                        {
                            var imageContent = content as PdfContentTypes.PdfImage;
                            var img = XImage.FromStream(imageContent.ImageStream);

                            double lowerLeftX, lowerLeftY, upperRightX, upperRightY;

                            switch (imageContent.SizeType)
                            {
                                case PdfContentTypes.Enums.PdfSizeType.Millimeter:
                                    lowerLeftX = imageContent.LowerLeftX / 0.353;
                                    lowerLeftY = imageContent.LowerLeftY / 0.353;
                                    upperRightX = imageContent.UpperRightX / 0.353;
                                    upperRightY = imageContent.UpperRightY / 0.353;
                                    break;
                                case PdfContentTypes.Enums.PdfSizeType.Centimeter:
                                    lowerLeftX = imageContent.LowerLeftX / 0.0353;
                                    lowerLeftY = imageContent.LowerLeftY / 0.0353;
                                    upperRightX = imageContent.UpperRightX / 0.0353;
                                    upperRightY = imageContent.UpperRightY / 0.0353;
                                    break;
                                case PdfContentTypes.Enums.PdfSizeType.Inch:
                                    lowerLeftX = imageContent.LowerLeftX / 0.0139;
                                    lowerLeftY = imageContent.LowerLeftY / 0.0139;
                                    upperRightX = imageContent.UpperRightX / 0.0139;
                                    upperRightY = imageContent.UpperRightY / 0.0139;
                                    break;
                                default:
                                    lowerLeftX = imageContent.LowerLeftX;
                                    lowerLeftY = imageContent.LowerLeftY;
                                    upperRightX = imageContent.UpperRightX;
                                    upperRightY = imageContent.UpperRightY;
                                    break;
                            }

                            var rect = new XRect(lowerLeftX, lowerLeftY, upperRightX - lowerLeftX, upperRightY - lowerLeftY);

                            gfx.DrawImage(img, rect);
                        }

                        if(content as PdfContentTypes.PdfText != null)
                        {
                            var textContent = content as PdfContentTypes.PdfText;

                            XFont font;

                            if (textContent.Bold)
                                font = new XFont(textContent.FontFamily, textContent.FontSize, XFontStyle.Bold);
                            else if(textContent.Italic)
                                font = new XFont(textContent.FontFamily, textContent.FontSize, XFontStyle.Italic);
                            else if (textContent.UnderLine)
                                font = new XFont(textContent.FontFamily, textContent.FontSize, XFontStyle.Underline);
                            else if (textContent.Strikeout)
                                font = new XFont(textContent.FontFamily, textContent.FontSize, XFontStyle.Strikeout);
                            else
                                font = new XFont(textContent.FontFamily, textContent.FontSize);

                            double lowerLeftX, lowerLeftY, upperRightX, upperRightY;

                            switch (textContent.SizeType)
                            {
                                case PdfContentTypes.Enums.PdfSizeType.Millimeter:
                                    lowerLeftX = textContent.LowerLeftX / 0.353;
                                    lowerLeftY = textContent.LowerLeftY / 0.353;
                                    upperRightX = textContent.UpperRightX / 0.353;
                                    upperRightY = textContent.UpperRightY / 0.353;
                                    break;
                                case PdfContentTypes.Enums.PdfSizeType.Centimeter:
                                    lowerLeftX = textContent.LowerLeftX / 0.0353;
                                    lowerLeftY = textContent.LowerLeftY / 0.0353;
                                    upperRightX = textContent.UpperRightX / 0.0353;
                                    upperRightY = textContent.UpperRightY / 0.0353;
                                    break;
                                case PdfContentTypes.Enums.PdfSizeType.Inch:
                                    lowerLeftX = textContent.LowerLeftX / 0.0139;
                                    lowerLeftY = textContent.LowerLeftY / 0.0139;
                                    upperRightX = textContent.UpperRightX / 0.0139;
                                    upperRightY = textContent.UpperRightY / 0.0139;
                                    break;
                                default:
                                    lowerLeftX = textContent.LowerLeftX;
                                    lowerLeftY = textContent.LowerLeftY;
                                    upperRightX = textContent.UpperRightX;
                                    upperRightY = textContent.UpperRightY;
                                    break;
                            }

                            var stringFormat = XStringFormats.TopLeft;

                            PdfSharp.Drawing.Layout.XTextFormatter tf = new PdfSharp.Drawing.Layout.XTextFormatter(gfx);
                            tf.DrawString(textContent.Text, font, XBrushes.Black, new XRect(lowerLeftX, lowerLeftY, upperRightX - lowerLeftX, upperRightY - lowerLeftY), stringFormat);
                        }
                    }
                }

                document.Save(filePath);
                pdfBytes = System.IO.File.ReadAllBytes(filePath);
            }
            return pdfBytes;
        }
    }
}
