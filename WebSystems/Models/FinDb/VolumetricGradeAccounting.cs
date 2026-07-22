using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSystems.Models.FinDb
{
    public class VolumetricGradeAccounting
    {
        public int ProductGroupId { get; set; }
        public string ProductGroup { get; set; }
        public DateTime MarkDatetime { get; set; }
    }
}
