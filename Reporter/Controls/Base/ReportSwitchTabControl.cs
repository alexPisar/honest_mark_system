using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Reporter.Controls.Base
{
    public class ReportSwitchTabControl : TabControl
    {
        public ReportSwitchTabControl()
        {
            SelectedValuePath = "DataEntityObject";
            SelectionChanged += ChangeItem;
        }

        private void ChangeItem(object sender, SelectionChangedEventArgs e)
        {
            if(SelectedItem.GetType() == typeof(ReportSwitchTabItem))
            {
                var item = SelectedItem as ReportSwitchTabItem;

                if(item.DataEntityObject != null)
                    item.DataContext = item.DataEntityObject;
            }
        }
    }
}
