using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Entities
{
    public class Product : Base.IReportEntity<Product>
    {

        public string Description { get; set; }

        public decimal? Quantity { get; set; }

        public string BarCode { get; set; }

        public decimal? Price { get; set; }

        public decimal? TaxAmount { get; set; }

        public decimal? Subtotal { get; set; }

        public List<string> MarkedCodes { get; set; }

        public List<string> TransportPackingIdentificationCode { get; set; }
    }
}
