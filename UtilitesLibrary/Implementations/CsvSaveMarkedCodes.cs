using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitesLibrary.Implementations
{
    public class CsvSaveMarkedCodes : Interfaces.ISaveMarkedCodes
    {
        public async Task SaveMarkedCodes(string pathFolder, string savedFileName, List<string> fullMarkedCodes)
        {
            using (var fileStream = new System.IO.FileStream($"{pathFolder}\\{savedFileName}.csv", System.IO.FileMode.Create))
            {
                using (var streamWriter = new System.IO.StreamWriter(fileStream))
                {
                    foreach (var markedCode in fullMarkedCodes)
                        await streamWriter.WriteLineAsync(markedCode);
                }
            }
        }
    }
}
