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

namespace Reporter.Controls.Base
{
    /// <summary>
    /// Логика взаимодействия для ReportAddedControl.xaml
    /// </summary>
    public partial class ReportAddedControl : UserControl
    {
        private int _minOccurs;
        private int _maxOccurs;

        public ReportAddedControl()
        {
            InitializeComponent();
            DataContext = new List<object>();
            _minOccurs = 0;
            _maxOccurs = 0;
        }

        private UIElement GetClonedControl(UIElement sourceControl)
        {
            Type controlType = sourceControl.GetType();
            object instance = Activator.CreateInstance(controlType);

            System.Reflection.PropertyInfo[] info = controlType.GetProperties();
            
            foreach (var pi in info)
            {
                if(pi.CanWrite)
                    pi.SetValue(instance, pi.GetValue(sourceControl, null), null);
                else if(sourceControl as ItemsControl != null && pi.Name == "Items")
                {
                    var itemsControl = sourceControl as ItemsControl;

                    foreach (var item in itemsControl.Items)
                        ((ItemsControl)instance).Items.Add(GetClonedControl((UIElement)item));
                }
            }
            return (UIElement)instance;
        }

        public int MinOccurs {
            get {
                return _minOccurs;
            }
            set {
                CurrentTabControl.Items.Clear();
                if (value > 0)
                {
                    AddItem(value);
                }

                RemoveButton.IsEnabled = false;

                _minOccurs = value;
            }
        }

        public int MaxOccurs {
            get 
                {
                return _maxOccurs;
            }
            set 
                {
                if (value <= CurrentTabControl?.Items?.Count)
                    AddButton.IsEnabled = false;

                _maxOccurs = value;
            }
        }

        public string TabText { get; set; }

        public UIElement ContentPage { get; set; }

        public ItemCollection Controls => CurrentTabControl.Items;

        private void OnItemsChanged()
        {
            var tabItems = CurrentTabControl.Items.Cast<TabItem>();

            if (ContentPage.GetType() == typeof(ReportSwitchTabControl))
                DataContext = tabItems.Select(r => ((ReportSwitchTabControl)r.Content).SelectedValue).ToList();
            else if (ContentPage.GetType() == typeof(ReportAddedControl))
                DataContext = tabItems.SelectMany(r => (List<object>)((ReportAddedControl)r.Content).DataContext).ToList();
            else if (ContentPage.GetType() == typeof(ReportComboBox))
                DataContext = tabItems.Select(r => ((ReportComboBox)r.Content).SelectedValue).ToList();
            else if (ContentPage as DevExpress.Xpf.Editors.TextEdit != null)
                DataContext = tabItems.Select(r => ((DevExpress.Xpf.Editors.TextEdit)r.Content).Text).ToList<object>();
            else if (ContentPage as TextBox != null)
                DataContext = tabItems.Select(r => ((TextBox)r.Content).Text).ToList<object>();
            else
                DataContext = tabItems.Select(r => ((FrameworkElement)r.Content).DataContext).ToList();
        }

        public void AddItem(int count)
        {
            for(int i = 0; i < count; i++)
            {
                var tabItem = new TabItem();
                tabItem.Content = GetClonedControl(ContentPage);
                tabItem.Header = TabText;

                object newElementObj;

                if (ContentPage.GetType() == typeof(ReportSwitchTabControl))
                {
                    var reportSwitchTabControl = (ReportSwitchTabControl)tabItem.Content;
                    reportSwitchTabControl.SelectionChanged += (object sender, SelectionChangedEventArgs e) => { OnItemsChanged(); };
                    reportSwitchTabControl.SelectedItem = reportSwitchTabControl?.Items[0];
                }
                else if (ContentPage.GetType() == typeof(ReportComboBox))
                {
                    var reportComboBox = (ReportComboBox)tabItem.Content;
                    reportComboBox.SelectionChanged += (object sender, SelectionChangedEventArgs e) => { OnItemsChanged(); };
                }
                else if (ContentPage as DevExpress.Xpf.Editors.TextEdit != null)
                {
                    var textEdit = (DevExpress.Xpf.Editors.TextEdit)tabItem.Content;
                    textEdit.EditValueChanged += (object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e) => { OnItemsChanged(); };
                }
                else if (ContentPage as TextBox != null)
                {
                    var textBox = (TextBox)tabItem.Content;
                    textBox.TextChanged += (object sender, TextChangedEventArgs e) => { OnItemsChanged(); };
                }

                CurrentTabControl.Items.Add(tabItem);
                OnItemsChanged();
                CurrentTabControl.DataContext = this.DataContext;
                this.Controls.Refresh();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddItem(1);

            if (_maxOccurs <= CurrentTabControl?.Items?.Count)
                AddButton.IsEnabled = false;

            RemoveButton.IsEnabled = true;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentTabControl.Items.Remove(CurrentTabControl.SelectedItem);

            if (_minOccurs >= CurrentTabControl?.Items?.Count)
                RemoveButton.IsEnabled = false;

            AddButton.IsEnabled = true;
        }
    }
}
