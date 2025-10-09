using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileWorker;

namespace UtilitesLibrary.Implementations
{
    public class XlsxSaveMarkedCodes : Interfaces.ISaveMarkedCodes
    {
        public async Task SaveMarkedCodes(string pathFolder, string savedFileName, List<string> fullMarkedCodes)
        {
            ExcelColumnCollection columnCollection = new ExcelColumnCollection();
            string sheetName = "Лист1";

            var docs = new List<ExcelDocumentData>();
            docs.Add(new ExcelDocumentData(columnCollection, sheetName));
            docs.First().ExportColumnNames = false;
            docs.First().SheetName = sheetName;

            var worker = new ExcelFileWorker($"{pathFolder}\\{savedFileName}.xlsx", docs);

            await Task.Run(() =>
            {
                IEnumerable<string> markedCodes = fullMarkedCodes.Select(m => m?.Substring(0, m.IndexOf((char)29)));

                foreach (var markedCode in markedCodes)
                    worker.ExportRow(markedCode, sheetName);
            });

            await worker.SaveFileAsync();
        }
    }
}
