using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UtilitesLibrary.Controls
{
    /// <summary>
    /// Логика взаимодействия для LoadControl.xaml
    /// </summary>
    public partial class LoadControl : UserControl
    {
        public EventHandler<RoutedEventArgs> OkButtonClick { get; set; }

        public LoadControl()
        {
            InitializeComponent();
            DataContext = new ModelBase.LoadModel();
        }

        private void SetSuccessFullLoad(string text = null)
        {
            ((ModelBase.LoadModel)DataContext).Text = text ?? "Загрузка завершена успешно.";
            ((ModelBase.LoadModel)DataContext).PathToImage = "/UtilitesLibrary;component/Resources/OK.png";
            ((ModelBase.LoadModel)DataContext).OkEnable = true;
            ((ModelBase.LoadModel)DataContext).OnAllPropertyChanged();
        }

        public string LoadText
        {
            get {
                return ((ModelBase.LoadModel)DataContext).Text;
            }

            set {
                ((ModelBase.LoadModel)DataContext).Text = value;
                ((ModelBase.LoadModel)DataContext).OnPropertyChanged("Text");
            }
        }

        public string TextAfterSuccessLoading
        {
            set {
                SetSuccessFullLoad(value);
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            OkButtonClick?.Invoke(sender, e);
        }
    }
}
