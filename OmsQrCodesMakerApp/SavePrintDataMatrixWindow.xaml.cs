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

namespace OmsQrCodesMakerApp
{
    /// <summary>
    /// Interaction logic for SavePrintDataMatrixWindow.xaml
    /// </summary>
    public partial class SavePrintDataMatrixWindow : Window
    {
        public SavePrintDataMatrixWindow()
        {
            InitializeComponent();
            SavedFileNameStackPanel.Visibility = Visibility.Hidden;
            IndexStackPanel.Visibility = Visibility.Hidden;
        }

        public int? Index
        {
            get
            {
                if (IndexSpinEdit?.EditValue == null)
                    return null;

                return Convert.ToInt32(IndexSpinEdit.EditValue);
            }
        }

        public string InkscapeLnkPath { get; set; }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if((DataContext as Models.SavePrintDataMatrixModel)?.SelectedFileType == null)
            {
                DevExpress.Xpf.Core.DXMessageBox.Show("Не указан формат файла", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(string.IsNullOrEmpty((DataContext as Models.SavePrintDataMatrixModel)?.FolderPath))
            {
                DevExpress.Xpf.Core.DXMessageBox.Show("Не указана папка для сохранения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(!System.IO.Directory.Exists((DataContext as Models.SavePrintDataMatrixModel).FolderPath))
            {
                var defaultDirectory = (DataContext as Models.SavePrintDataMatrixModel)?.DefaultFolderPath;

                if ((DataContext as Models.SavePrintDataMatrixModel).FolderPath == defaultDirectory)
                {
                    System.IO.Directory.CreateDirectory(defaultDirectory);
                }
                else
                {
                    DevExpress.Xpf.Core.DXMessageBox.Show("Не найдена папка для сохранения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if(string.IsNullOrEmpty((DataContext as Models.SavePrintDataMatrixModel)?.SavedFileName))
            {
                var errorMessage = string.Empty;

                if ((DataContext as Models.SavePrintDataMatrixModel).SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Pdf ||
                    (DataContext as Models.SavePrintDataMatrixModel).SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Csv)
                    errorMessage = "Не указано наименование файла!";
                else
                    errorMessage = "Не указан префикс!";

                DevExpress.Xpf.Core.DXMessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if((DataContext as Models.SavePrintDataMatrixModel).SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Eps)
            {
                if (System.IO.File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)}\\Programs\\Inkscape.lnk"))
                {
                    InkscapeLnkPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)}\\Programs\\Inkscape.lnk";
                }
                else if (System.IO.File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)}\\Programs\\Inkscape\\Inkscape.lnk"))
                {
                    InkscapeLnkPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)}\\Programs\\Inkscape\\Inkscape.lnk";
                }
                else
                {
                    InkscapeLnkPath = null;
                    DevExpress.Xpf.Core.DXMessageBox.Show("Невозможно экспортировать коды в формате EPS.\nНе установлена программа Inkscape.\nПроверьте установку программы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void ChangeFolderButton_Click(object sender, RoutedEventArgs e)
        {
            UtilitesLibrary.Interfaces.IOpenDialogsWpf openPathDialog = new UtilitesLibrary.Service.OokiiDialogsWpf();
            (DataContext as Models.SavePrintDataMatrixModel).FolderPath = openPathDialog.ChangePathDialog("Выберите папку для сохранения файлов");
            (DataContext as Models.SavePrintDataMatrixModel).OnPropertyChanged("FolderPath");
        }

        private void ChangeFileTypeComboBox_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            if((DataContext as Models.SavePrintDataMatrixModel)?.SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Pdf ||
                (DataContext as Models.SavePrintDataMatrixModel)?.SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Csv)
            {
                (DataContext as Models.SavePrintDataMatrixModel).SavedFileName = $"order_{(DataContext as Models.SavePrintDataMatrixModel)?.OrderId}_gtin_{(DataContext as Models.SavePrintDataMatrixModel)?.Gtin}_quantity_{(DataContext as Models.SavePrintDataMatrixModel)?.Quantity}";
                (DataContext as Models.SavePrintDataMatrixModel).OnPropertyChanged("SavedFileName");

                SavedFileNameStackPanel.Visibility = Visibility.Visible;
                IndexStackPanel.Visibility = Visibility.Hidden;
                SavedFileNameLabel.Content = "Наименование";
            }
            else if ((DataContext as Models.SavePrintDataMatrixModel)?.SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Svg ||
                (DataContext as Models.SavePrintDataMatrixModel)?.SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Eps)
            {
                (DataContext as Models.SavePrintDataMatrixModel).SavedFileName = $"order_{(DataContext as Models.SavePrintDataMatrixModel)?.OrderId}_gtin_{(DataContext as Models.SavePrintDataMatrixModel)?.Gtin}";
                (DataContext as Models.SavePrintDataMatrixModel).OnPropertyChanged("SavedFileName");

                SavedFileNameStackPanel.Visibility = Visibility.Visible;
                IndexStackPanel.Visibility = Visibility.Visible;
                SavedFileNameLabel.Content = "Префикс";
            }
        }
    }
}
