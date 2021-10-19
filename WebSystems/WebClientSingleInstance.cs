using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSystems
{
    public abstract class WebClientSingleInstance<TClient> : IWebClient where TClient: WebClientSingleInstance<TClient>
    {
        protected static TClient _instance;

        public static TClient GetInstance()
        {
            return _instance;
        }
    }
}
