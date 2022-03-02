using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Entities
{
    public class CorrectionProduct : Base.IReportEntity<CorrectionProduct>
    {
        public string BarCode { get; set; }
        public int Number { get; set; }
        public string Description { get; set; }

        public decimal? PriceBefore { get; set; }
        public decimal? QuantityBefore { get; set; }
        public decimal? TaxAmountBefore { get; set; }
        public decimal? SubtotalBefore { get; set; }
        public List<string> MarkedCodesBefore { get; set; }
        public List<string> TransportPackingIdentificationCodeBefore { get; set; }

        public decimal? PriceAfter { get; set; }
        public decimal? QuantityAfter { get; set; }
        public decimal? TaxAmountAfter { get; set; }
        public decimal? SubtotalAfter { get; set; }
        public List<string> MarkedCodesAfter { get; set; }
        public List<string> TransportPackingIdentificationCodeAfter { get; set; }
    }
}
