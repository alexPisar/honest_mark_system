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
using System.Windows.Shapes;

namespace HonestMarkSystem
{
    /// <summary>
    /// Логика взаимодействия для LoadWindow.xaml
    /// </summary>
    public partial class LoadWindow : Window
    {
        public LoadWindow(string loadText = null)
        {
            InitializeComponent();

            if(!string.IsNullOrEmpty(loadText))
                loadControl.LoadText = loadText;

            loadControl.OkButtonClick += this.OkButtonClick;
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void AfterSuccessfullLoading(string text)
        {
            loadControl.TextAfterSuccessLoading = text;
        }

        public void SetLoadingText(string text)
        {
            loadControl.LoadText = text;
        }

        public UtilitesLibrary.ModelBase.LoadModel GetLoadContext()
        {
            return (UtilitesLibrary.ModelBase.LoadModel)loadControl.DataContext;
        }

        private void LoadWindow_Closed(object sender, EventArgs e)
        {
            if (Owner != null)
                Owner.IsEnabled = true;
        }

        private void LoadWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Owner != null)
                Owner.IsEnabled = false;
        }
    }
}
