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
    /// Логика взаимодействия для ChangeMarkedCodesWindow.xaml
    /// </summary>
    public partial class ChangeMarkedCodesWindow : Window
    {
        public ChangeMarkedCodesWindow()
        {
            InitializeComponent();
        }

        private async void WithdrawalButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Models.ChangeMarkedCodesModel)DataContext).SelectedCodes.Count == 0)
            {
                MessageBox.Show("Не выбраны коды маркировки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (((Models.ChangeMarkedCodesModel)DataContext).SelectedProductGroup.Key == default(KeyValuePair<WebSystems.ProductGroupsEnum, string>).Key)
            {
                MessageBox.Show("Не выбрана товарная группа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (docDatePicker?.SelectedDate == null)
            {
                MessageBox.Show("Не указана дата первичного документа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Exception exception = null;
            var context = (Models.ChangeMarkedCodesModel)DataContext;
            context.DocumentDate = docDatePicker.SelectedDate;

            var loadWindow = new LoadWindow("Подождите, идёт обработка данных");
            loadWindow.Owner = this;
            var loadContext = loadWindow.GetLoadContext();
            loadWindow.Show();

            await Task.Run(() =>
            {
                try
                {
                    var result = context.WithdrawalFromTurnover();

                    if (result)
                        loadContext.SetSuccessFullLoad("Данные были успешно загружены.");
                    else
                        throw new Exception("Обработка документа - Вывод из оборота - не была корректной");
                }
                catch(Exception ex)
                {
                    exception = ex;
                }
            });

            if(exception != null)
            {
                loadWindow.Close();
                var errorsWindow = new ErrorsWindow("Произошла ошибка.", new List<string>(new string[] { $"{exception.Message}\n{(exception.InnerException?.Message ?? "")}" }));
                errorsWindow.ShowDialog();
            }
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            var changeWindow = new MarkedCodesWindow();

            var changeContext = new Models.MarkedCodesModel(((Models.ChangeMarkedCodesModel)DataContext).MarkedCodes);
            changeContext.SelectedItems = ((Models.ChangeMarkedCodesModel)DataContext).SelectedCodes;
            changeWindow.DataContext = changeContext;

            if(changeWindow.ShowDialog() == true)
            {
                ((Models.ChangeMarkedCodesModel)DataContext).SelectedCodes = changeContext.SelectedItems;
                ((Models.ChangeMarkedCodesModel)DataContext).OnPropertyChanged("SelectedCodes");
                gridControl.RefreshData();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ((Models.ChangeMarkedCodesModel)DataContext).SelectedCodes = new List<DataContextManagementUnit.DataAccess.Contexts.Abt.DocGoodsDetailsLabels>();
            ((Models.ChangeMarkedCodesModel)DataContext).OnPropertyChanged("SelectedCodes");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
