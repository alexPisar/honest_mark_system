﻿using System;
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
            PathToImage = "/UtilitesLibrary;component/Resources/download.gif";
            OkEnable = false;
            Text = "Подождите, идёт загрузка данных";
        }

        public void SetLoadingText(string text)
        {
            this.Text = text;
            OnPropertyChanged("Text");
        }

        public void SetSuccessFullLoad(string text = null)
        {
            Text = text ?? "Загрузка завершена успешно.";
            PathToImage = "/UtilitesLibrary;component/Resources/OK.png";
            OkEnable = true;
            OnAllPropertyChanged();
        }
    }
}
