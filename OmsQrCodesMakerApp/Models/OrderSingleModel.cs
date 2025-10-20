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

            releaseCodesWindow.ShowDialog();

            this.Refresh();
        }

        public async void RetryGetCodes()
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
                var savePrintDataMatrixModel = new SavePrintDataMatrixModel(_order.OrderId, SelectedItem.Gtin, markedCodes.Count);
                savePrintDataMatrixWindow.DataContext = savePrintDataMatrixModel;

                if (savePrintDataMatrixWindow.ShowDialog() == true)
                {
                    var pathFolder = savePrintDataMatrixModel.FolderPath;
                    var loadWindow = new LoadWindow("Идёт загрузка...");
                    loadWindow.Show();

                    try
                    {
                        UtilitesLibrary.Interfaces.ISaveMarkedCodes saveCodesObj =
                            new UtilitesLibrary.Implementations.SaveMarkedCodesOptionFactory(savePrintDataMatrixModel.SelectedFileType.Value)
                            .GetSaveMarkedCodesOption();

                        if(saveCodesObj as UtilitesLibrary.Implementations.ImageSaveMarkedCodes != null)
                        {
                            var imageSaveCodesObj = saveCodesObj as UtilitesLibrary.Implementations.ImageSaveMarkedCodes;
                            imageSaveCodesObj.Index = savePrintDataMatrixWindow.Index.Value;

                            if (saveCodesObj as UtilitesLibrary.Implementations.EpsSaveMarkedCodes != null)
                            {
                                var epsSaveCodesObj = saveCodesObj as UtilitesLibrary.Implementations.EpsSaveMarkedCodes;
                                epsSaveCodesObj.InkscapeLnkPath = savePrintDataMatrixWindow.InkscapeLnkPath;
                            }
                        }

                        var task = saveCodesObj.SaveMarkedCodes(pathFolder, savePrintDataMatrixModel.SavedFileName, markedCodes);
                        await task;

                        loadWindow.AfterSuccessfullLoading("Коды успешно сохранены.");
                    }
                    finally
                    {
                        if(System.Windows.Application.Current?.Windows?.OfType<LoadWindow>()?.Contains(loadWindow) ?? false)
                            loadWindow.Close();
                    }
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
