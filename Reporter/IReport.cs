using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter
{
    public abstract class IReport
    {
        public virtual void Parse(string content) { }

        public abstract void Parse(byte[] content);
    }
}
