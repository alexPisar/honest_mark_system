using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitesLibrary.ModelBase
{
    public class LoadModel : ViewModelBase
    {
        public string PathToImage { get; set; }
        public bool OkEnable { get; set; }
        public string Text { get; set; }

        public LoadModel()
        {
            PathToImage = "pack://siteoforigin:,,,/Resources/download.gif";
            OkEnable = false;
            Text = "Подождите, идёт загрузка данных";
        }
    }
}
