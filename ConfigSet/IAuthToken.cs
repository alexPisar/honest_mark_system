using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigSet
{
    public abstract class IAuthToken<TConfig> : Configuration<TConfig>
    {
        public abstract string Token { get; set; }
    }
}
