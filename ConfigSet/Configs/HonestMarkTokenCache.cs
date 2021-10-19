using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigSet.Configs
{
    public class HonestMarkTokenCache : IAuthToken<HonestMarkTokenCache>
    {
        private const string localPath = "honestmark";
        protected override string Path(string FileName) => ConfigFolder + "\\" + localPath + "\\" + FileName;

        public HonestMarkTokenCache()
        {
            if (!System.IO.Directory.Exists(ConfigFolder + "\\" + localPath))
                System.IO.Directory.CreateDirectory(ConfigFolder + "\\" + localPath);
        }

        public override string Token { get; set; }
        public DateTime TokenCreationDate { get; set; }
        public DateTime TokenExpirationDate { get; set; }
    }
}
