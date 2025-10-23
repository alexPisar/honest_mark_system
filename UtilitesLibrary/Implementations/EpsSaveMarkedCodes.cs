using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitesLibrary.Implementations
{
    public class EpsSaveMarkedCodes : ImageSaveMarkedCodes
    {
        public EpsSaveMarkedCodes() : base()
        {
            
        }

        public override async Task SaveMarkedCodes(string pathFolder, string savedFileName, List<string> fullMarkedCodes)
        {
            if (fullMarkedCodes == null)
                return;

            if (Index == null)
                throw new Exception("Не задан индекс для сохранения.");

            int indx = Index.Value;
            foreach (var markedCode in fullMarkedCodes)
            {
                string indxStr = indx.ToString();

                if (indxStr.Length < 5)
                    indxStr = indxStr.PadLeft(5, '0');

                var dataMatrixRawCode = _dataMatrixGenerator.GetRawDataMatrixCode(markedCode);

                var rawWidth = dataMatrixRawCode.GetLength(0);
                var rawHeight = dataMatrixRawCode.GetLength(1);

                decimal recHeight = 1.0001M;
                decimal recWidth = 1.0001M;

                using (var fileStream = new System.IO.FileStream($"{pathFolder}\\{savedFileName}_{indxStr}.eps", System.IO.FileMode.Create))
                {
                    using (var streamWriter = new System.IO.StreamWriter(fileStream))
                    {
                        await streamWriter.WriteLineAsync("%!PS-Adobe-3.0 EPSF-3.0\n" +
                            $"%%BoundingBox: 0 0 {rawWidth + 3} {rawHeight + 3}\n" +
                            $"%%HiResBoundingBox: 0 0 {(rawWidth + 2) * recWidth} {(rawHeight + 2) * recHeight}\n" +
                            "%%Creator: Barcode4J (http://barcode4j.krysalis.org)\n" +
                            $"%%CreationDate: {DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")}\n" +
                            "%%LanguageLevel: 1\n" +
                            "%%EndComments\n" +
                            "%%BeginProlog\n" +
                            "%%BeginProcSet: barcode4j-procset 1.1\n" +
                            "/rf {\n" +
                            "newpath\n" +
                            "4 -2 roll moveto\n" +
                            "dup neg 0 exch rlineto\n" +
                            "exch 0 rlineto\n" +
                            "0 neg exch rlineto\n" +
                            "closepath fill\n" +
                            "} def\n" +
                            "/ct {\n" +
                            "moveto dup stringwidth\n" +
                            "2 div neg exch 2 div neg exch\n" +
                            "rmoveto show\n" +
                            "} def\n" +
                            "/rt {\n" +
                            "4 -1 roll dup stringwidth pop\n" +
                            "5 -2 roll 1 index sub\n" +
                            "3 -1 roll sub\n" +
                            "add\n" +
                            "3 -1 roll moveto show\n" +
                            "} def\n" +
                            "/jt {\n" +
                            "4 -1 roll dup stringwidth pop\n" +
                            "5 -2 roll 1 index sub\n" +
                            "3 -1 roll sub\n" +
                            "2 index length\n" +
                            "1 sub div\n" +
                            "0 4 -1 roll 4 -1 roll 5 -1 roll\n" +
                            "moveto ashow\n" +
                            "} def\n" +
                            "%%EndProcSet: barcode4j-procset 1.0\n" +
                            "%%EndProlog");

                        for (int i = 0; i < rawHeight; i++)
                        {
                            for (int j = 0; j < rawWidth; j++)
                            {
                                if (dataMatrixRawCode[j, i])
                                {
                                    await streamWriter.WriteLineAsync($"{recWidth * (j + 1)} {recHeight * (rawHeight + 1 - i)} {recWidth} {recHeight} rf");
                                }
                            }
                        }

                        await streamWriter.WriteLineAsync("%%EOF");
                    }
                }

                indx++;
            }
        }
    }
}
