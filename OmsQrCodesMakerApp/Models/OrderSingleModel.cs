using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.ModelBase;

namespace OmsQrCodesMakerApp.Models
{
    public class OrderSingleModel : ListViewModel<ViewModels.OmsProduct>
    {
        private ViewModels.OmsOrder _order;
        private WebSystems.WebClients.OrderManagementStationClient _omsClient;

        public override RelayCommand RefreshCommand => new RelayCommand((o) => { Refresh(); });
        public RelayCommand ReleaseCodesCommand => new RelayCommand((o) => { ReleaseCodes(); });
        public RelayCommand RetryGetCodesCommand => new RelayCommand((o) => { RetryGetCodes(); });
        public RelayCommand RetryGetPrintCodesCommand => new RelayCommand((o) => { RetryGetPrintCodes(); });

        public OrderSingleModel(ViewModels.OmsOrder order, WebSystems.WebClients.OrderManagementStationClient omsClient)
        {
            ItemsList = new System.Collections.ObjectModel.ObservableCollection<ViewModels.OmsProduct>(order.Products);
            _order = order;
            _omsClient = omsClient;
        }

        public override void Refresh()
        {
            if (_order == null)
                return;

            try
            {
                _order.OrderStatuses = _omsClient?.GetOrderStatus(_order.OrderId)?.ToList() ?? new List<WebSystems.Models.OMS.BufferInfo>();
                _order.Products = _omsClient?.GetProductListFromOrder(_order.OrderId)?.Select(p =>
                new ViewModels.OmsProduct
                {
                    Gtin = p.Key,
                    Product = p.Value,
                    OrderStatus = _order.OrderStatuses.FirstOrDefault(s => s.Gtin == p.Key)
                })?.ToList();

                ItemsList = new System.Collections.ObjectModel.ObservableCollection<ViewModels.OmsProduct>(_order.Products);
                SelectedItem = null;
                OnPropertyChanged("ItemsList");
                OnPropertyChanged("SelectedItem");
            }
            catch (Exception ex)
            {
                string errorMessage = _log.GetRecursiveInnerException(ex);
                _log.Log(errorMessage);

                var errorsWindow = new ErrorsWindow("Произошла ошибка обновления.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();
            }
        }

        public void ReleaseCodes()
        {
            if (SelectedItem == null)
            {
                DevExpress.Xpf.Core.DXMessageBox.Show("Не выбран товар!", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.OrderStatus.TotalPassed == SelectedItem.OrderStatus.TotalCodes)
            {
                DevExpress.Xpf.Core.DXMessageBox.Show("Все коды товара уже были ранее получены!", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            var releaseCodesWindow = new ReleaseCodesWindow(_omsClient, SelectedItem.OrderStatus.TotalCodes - SelectedItem.OrderStatus.TotalPassed,
                SelectedItem.OrderStatus.TotalCodes - SelectedItem.OrderStatus.TotalPassed);
            releaseCodesWindow.Order = _order;
            releaseCodesWindow.Product = SelectedItem;

            if (releaseCodesWindow.ShowDialog() == true)
            {
                var loadWindow = new LoadWindow();
                loadWindow.AfterSuccessfullLoading("Коды успешно получены.");
                loadWindow.Show();
            }

            this.Refresh();
        }

        public void RetryGetCodes()
        {
            if(SelectedItem == null)
            {
                DevExpress.Xpf.Core.DXMessageBox.Show("Не выбран товар!", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.OrderStatus.TotalPassed == 0)
            {
                DevExpress.Xpf.Core.DXMessageBox.Show("Коды не были ранее получены!", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            try
            {
                var orderBlocks = _omsClient.GetBlockIdsFromOrder(_order.OrderId, SelectedItem.Gtin);
                List<string> markedCodes = new List<string>();

                if (orderBlocks?.Blocks != null)
                    foreach (var block in orderBlocks.Blocks)
                    {
                        var markedCodesObj = _omsClient.GetMarkedCodesRetry(block.BlockId);

                        if (markedCodesObj?.Codes != null && markedCodesObj.Codes.Length > 0)
                            markedCodes.AddRange(markedCodesObj.Codes);
                    }

                if (markedCodes.Count == 0)
                {
                    DevExpress.Xpf.Core.DXMessageBox.Show("Не удалось получить ранее сохранённые коды!", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                var savePathDialog = new Microsoft.Win32.SaveFileDialog();
                savePathDialog.Title = "Сохранение файла";
                savePathDialog.Filter = "CSV File|*.csv";
                savePathDialog.FileName = $"{_order.OrderId}_{SelectedItem.Gtin}";

                if (savePathDialog.ShowDialog() == true)
                {
                    using (var fileStream = new System.IO.FileStream(savePathDialog.FileName, System.IO.FileMode.Create))
                    {
                        using (var streamWriter = new System.IO.StreamWriter(fileStream))
                        {
                            streamWriter.WriteLine("DataMatrix");

                            foreach (var markedCode in markedCodes)
                                streamWriter.WriteLine(markedCode);
                        }
                    }

                    var loadWindow = new LoadWindow();
                    loadWindow.AfterSuccessfullLoading("Файл успешно сохранён.");
                    loadWindow.Show();
                }
            }
            catch (System.Net.WebException webEx)
            {
                string errorMessage = _log.GetRecursiveInnerException(webEx);
                _log.Log(errorMessage);

                var errorsWindow = new ErrorsWindow("Произошла ошибка на удалённом сервере.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                string errorMessage = _log.GetRecursiveInnerException(ex);
                _log.Log(errorMessage);

                var errorsWindow = new ErrorsWindow("Произошла ошибка.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();
            }
        }

        public async void RetryGetPrintCodes()
        {
            if (SelectedItem == null)
            {
                DevExpress.Xpf.Core.DXMessageBox.Show("Не выбран товар!", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedItem.OrderStatus.TotalPassed == 0)
            {
                DevExpress.Xpf.Core.DXMessageBox.Show("Коды не были ранее получены!", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            try
            {
                var orderBlocks = _omsClient.GetBlockIdsFromOrder(_order.OrderId, SelectedItem.Gtin);
                List<string> markedCodes = new List<string>();

                if (orderBlocks?.Blocks != null)
                    foreach (var block in orderBlocks.Blocks)
                    {
                        var markedCodesObj = _omsClient.GetMarkedCodesRetry(block.BlockId);

                        if (markedCodesObj?.Codes != null && markedCodesObj.Codes.Length > 0)
                            markedCodes.AddRange(markedCodesObj.Codes);
                    }

                if (markedCodes.Count == 0)
                {
                    DevExpress.Xpf.Core.DXMessageBox.Show("Не удалось получить ранее сохранённые коды!", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                UtilitesLibrary.Interfaces.IDataMatrixGenerator dataMatrixGenerator = new UtilitesLibrary.Service.DataMatrixNetGenerator();

                var savePrintDataMatrixWindow = new SavePrintDataMatrixWindow();
                var savePrintDataMatrixModel = new SavePrintDataMatrixModel($"order_{_order.OrderId}_gtin_{SelectedItem.Gtin}");
                savePrintDataMatrixWindow.DataContext = savePrintDataMatrixModel;

                if (savePrintDataMatrixWindow.ShowDialog() == true)
                {
                    var pathFolder = savePrintDataMatrixModel.FolderPath;
                    var loadWindow = new LoadWindow("Идёт загрузка...");
                    loadWindow.Show();

                    int indx;

                    if (savePrintDataMatrixModel.SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Svg ||
                        savePrintDataMatrixModel.SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Eps)
                        indx = savePrintDataMatrixWindow.Index.Value;
                    else
                        indx = 0;

                    var pdfPages = new List<PdfPage>();

                    string inkscapePath = null;
                    System.Diagnostics.Process process = null;

                    if (savePrintDataMatrixModel.SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Eps)
                    {
                        inkscapePath = savePrintDataMatrixWindow.InkscapeLnkPath;

                        if (!string.IsNullOrEmpty(inkscapePath))
                        {
                            inkscapePath = WindowsShortcutFactory.WindowsShortcut.Load(inkscapePath)?.Path;
                            process = new System.Diagnostics.Process();
                            process.StartInfo.FileName = inkscapePath;
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.StartInfo.CreateNoWindow = true;
                        }
                        else
                            throw new Exception("Невозможно экспортировать коды в формате EPS - не установлена программа Inkscape.\nПроверьте установку программы.");
                    }

                    foreach (var markedCode in markedCodes)
                    {
                        var markedCodeText = markedCode.Substring(0, markedCode.IndexOf((char)29));

                        if (savePrintDataMatrixModel.SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Svg ||
                            savePrintDataMatrixModel.SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Eps)
                        {
                            string indxStr = indx.ToString();

                            if (indxStr.Length < 5)
                                indxStr = indxStr.PadLeft(5, '0');

                            string fileName = $"{savePrintDataMatrixModel.SavedFileName}_{indxStr}";
                            string svgFilePath = $"{pathFolder}\\{savePrintDataMatrixModel.SavedFileName}_{indxStr}.svg";
                            var savedFileName = savePrintDataMatrixModel.SavedFileName;

                            await Task.Run(() =>
                            {
                                var img = dataMatrixGenerator.GenerateAndSaveDataMatrix(markedCode, fileName, pathFolder, 300, 300);

                                if (!string.IsNullOrEmpty(inkscapePath))
                                {
                                    string epsFilePath = $"{pathFolder}\\{savedFileName}_{indxStr}.eps";
                                    string arguments = $"-z \"{svgFilePath}\" --export-type=eps -o \"{epsFilePath}\"";

                                    process.StartInfo.Arguments = arguments;
                                    process.Start();
                                    string output = process.StandardOutput.ReadToEnd();
                                    process.WaitForExit();
                                }
                            });

                            if (!string.IsNullOrEmpty(inkscapePath))
                            {
                                if (process.ExitCode != 0)
                                    throw new Exception($"Ошибка при конвертации. Код выхода: {process.ExitCode}");

                                System.IO.File.Delete($"{pathFolder}\\{savePrintDataMatrixModel.SavedFileName}_{indxStr}.svg");
                            }

                            indx++;
                        }
                        else if (savePrintDataMatrixModel.SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Pdf)
                        {
                            var imageStream = dataMatrixGenerator.GetImageStreamByDataMatrix(markedCode, 300, 300);

                            var pdfPage = new PdfPage()
                            {
                                SizeType = UtilitesLibrary.Service.PdfContentTypes.Enums.PdfSizeType.Millimeter,
                                Height = 25,
                                Width = 43,
                                Contents = new List<IPdfContent>(new IPdfContent[] {
                                new UtilitesLibrary.Service.PdfContentTypes.PdfImage
                                {
                                    SizeType = UtilitesLibrary.Service.PdfContentTypes.Enums.PdfSizeType.Millimeter,
                                    LowerLeftX = 1,
                                    LowerLeftY = 2,
                                    UpperRightX = 22,
                                    UpperRightY = 23,
                                    ImageStream = imageStream
                                },
                                new UtilitesLibrary.Service.PdfContentTypes.PdfText
                                {
                                    SizeType = UtilitesLibrary.Service.PdfContentTypes.Enums.PdfSizeType.Millimeter,
                                    LowerLeftX = 23,
                                    LowerLeftY = 4,
                                    UpperRightX = 41,
                                    UpperRightY = 7,
                                    Text = markedCodeText.Substring(0, 12),
                                    FontFamily = "Arial",
                                    FontSize = 7,
                                    Bold = true
                                },
                                new UtilitesLibrary.Service.PdfContentTypes.PdfText
                                {
                                    SizeType = UtilitesLibrary.Service.PdfContentTypes.Enums.PdfSizeType.Millimeter,
                                    LowerLeftX = 23,
                                    LowerLeftY = 7,
                                    UpperRightX = 41,
                                    UpperRightY = 10,
                                    Text = markedCodeText.Substring(12, 12),
                                    FontFamily = "Arial",
                                    FontSize = 7,
                                    Bold = true
                                },
                                new UtilitesLibrary.Service.PdfContentTypes.PdfText
                                {
                                    SizeType = UtilitesLibrary.Service.PdfContentTypes.Enums.PdfSizeType.Millimeter,
                                    LowerLeftX = 23,
                                    LowerLeftY = 10,
                                    UpperRightX = 41,
                                    UpperRightY = 13,
                                    Text = markedCodeText.Substring(24),
                                    FontFamily = "Arial",
                                    FontSize = 7,
                                    Bold = true
                                }
                            })
                            };

                            pdfPages.Add(pdfPage);
                        }
                    }

                    if (savePrintDataMatrixModel.SelectedFileType == UtilitesLibrary.Enums.FileTypeEnum.Pdf)
                    {
                        UtilitesLibrary.Interfaces.IPdfFileWorker pdfWorker = new UtilitesLibrary.Service.PDFsharpWorker();
                        pdfWorker.GetPdfFileFromContents(pdfPages, $"{pathFolder}\\{savePrintDataMatrixModel.SavedFileName}.pdf");
                    }

                    loadWindow.AfterSuccessfullLoading("Коды успешно сохранёны.");
                }
            }
            catch (System.Net.WebException webEx)
            {
                string errorMessage = _log.GetRecursiveInnerException(webEx);
                _log.Log(errorMessage);

                var errorsWindow = new ErrorsWindow("Произошла ошибка на удалённом сервере.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                string errorMessage = _log.GetRecursiveInnerException(ex);
                _log.Log(errorMessage);

                var errorsWindow = new ErrorsWindow("Произошла ошибка.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();
            }
        }
    }
}
