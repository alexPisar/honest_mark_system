using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Entities
{
    public class InvoiceCorrectionInfo : Base.IReportEntity<InvoiceCorrectionInfo>
    {
        public string DocNumber { get; set; }
        public DateTime DocDate { get; set; }
    }
}
