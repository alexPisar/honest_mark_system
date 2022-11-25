using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSystems.Models.FinDb
{
    public class FinDbDocumentComissionInfo
    {
        public string IdDoc { get; set; }
        public string SenderInn { get; set; }
        public string ReceiverInn { get; set; }
        public string FileName { get; set; }
    }
}
