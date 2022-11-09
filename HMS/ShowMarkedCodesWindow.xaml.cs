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
using HonestMarkSystem.Interfaces;

namespace HonestMarkSystem
{
    /// <summary>
    /// Логика взаимодействия для ShowMarkedCodesWindow.xaml
    /// </summary>
    public partial class ShowMarkedCodesWindow : Window
    {
        public ShowMarkedCodesWindow()
        {
            InitializeComponent();
        }

        public void SetMarkedItems()
        {
            if (DataContext as ITreeListView == null)
                return;

            (DataContext as ITreeListView).SetItems(treeList.Nodes);
            MarkedCodesDataGrid.RefreshData();
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IEnumerable<DevExpress.Xpf.Grid.TreeListNode> nodes = treeList?.Nodes;

                if (nodes == null)
                    nodes = new List<DevExpress.Xpf.Grid.TreeListNode>();

                (DataContext as ITreeListView).ProcessingSelectedItems(nodes);
                SetMarkedItems();
            }
            catch(Exception ex)
            {

            }
        }
    }
}
