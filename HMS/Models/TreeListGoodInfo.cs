using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonestMarkSystem.Models
{
    public class TreeListGoodInfo
    {
        public TreeListGoodInfo()
        {
            Children = new List<TreeListGoodInfo>();
        }

        public string Name { get; set; }
        public int? Quantity { get; set; } = null;
        public string BarCode { get; set; }
        public bool NotAllDocumentsMarked { get; set; }
        public bool NotMarked { get; set; }
        public List<TreeListGoodInfo> Children { get; set; }
    }
}
