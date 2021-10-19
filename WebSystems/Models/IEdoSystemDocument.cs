using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSystems.Models
{
    public abstract class IEdoSystemDocument<TIdType>
    {
        public virtual TIdType EdoId { get; set; }
        public virtual int DocType { get; set; }
    }
}
