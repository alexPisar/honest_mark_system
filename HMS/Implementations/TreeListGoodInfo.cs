using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonestMarkSystem.Implementations
{
    public class TreeListGoodInfo
    {
        public decimal? IdDoc { get; set; }
        public decimal? IdGood { get; set; }
        public decimal? Quantity { get; set; }
        public string Name { get; set; }
        public string BarCode { get; set; }
        public bool NotAllMarked { get; set; }
        public bool NotMarked { get; set; }
        public bool IsMarkedCode { get; set; }
        public decimal? Price { get; set; }
        public decimal? TaxAmount { get; set; }
    }
}
