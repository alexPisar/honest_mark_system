using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace UtilitesLibrary.Service
{
    public static class KnownFolders
    {
        private static readonly Dictionary<Enums.KnownFolder, Guid> _guids = new Dictionary<Enums.KnownFolder, Guid>()
        {
            [Enums.KnownFolder.Contacts] = new Guid("56784854-C6CB-462B-8169-88E350ACB882"),
            [Enums.KnownFolder.Downloads] = new Guid("374DE290-123F-4565-9164-39C4925E467B"),
            [Enums.KnownFolder.Favorites] = new Guid("1777F761-68AD-4D8A-87BD-30B759FA33DD"),
            [Enums.KnownFolder.Links] = new Guid("BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968")
        };

        public static string GetPath(Enums.KnownFolder knownFolder)
        {
            return SHGetKnownFolderPath(_guids[knownFolder], 0);
        }

        [DllImport("shell32", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        private static extern string SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, int hToken = 0);
    }
}
