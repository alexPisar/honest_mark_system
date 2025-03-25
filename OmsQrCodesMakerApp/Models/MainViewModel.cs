using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.ModelBase;

namespace OmsQrCodesMakerApp.Models
{
    public class MainViewModel : ListViewModel<ViewModels.OmsOrder>
    {
        private WebSystems.WebClients.OrderManagementStationClient _omsClient;

        public override RelayCommand RefreshCommand => new RelayCommand((o) => { Refresh(); });
        public override RelayCommand EditCommand => new RelayCommand((o) => { Edit(); });

        public MainViewModel()
        {
            _omsClient = WebSystems.WebClients.OrderManagementStationClient.GetInstance();
        }

        public override void Refresh()
        {
            try
            {
                var orders = _omsClient.GetOrdersList()?.OrderInfos?.Select(o => new ViewModels.OmsOrder
                {
                    OrderInfo = o
                })?.ToList() ?? new List<ViewModels.OmsOrder>();

                Parallel.ForEach(orders,
                    (o) =>
                    {
                        o.OrderStatuses = _omsClient?.GetOrderStatus(o.OrderId)?.ToList() ?? new List<WebSystems.Models.OMS.BufferInfo>();
                        o.Products = _omsClient.GetProductListFromOrder(o.OrderId)?.Select(p =>
                        new ViewModels.OmsProduct
                        {
                            Gtin = p.Key,
                            Product = p.Value,
                            OrderStatus = o.OrderStatuses.FirstOrDefault(s => s.Gtin == p.Key)
                        })?.ToList();
                    });

                ItemsList = new System.Collections.ObjectModel.ObservableCollection<ViewModels.OmsOrder>(orders);
                SelectedItem = null;
                OnPropertyChanged("ItemsList");
                OnPropertyChanged("SelectedItem");
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

                var errorsWindow = new ErrorsWindow("Произошла ошибка получения заказов.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();
            }
        }

        public override void Edit()
        {
            if (SelectedItem == null)
            {
                DevExpress.Xpf.Core.DXMessageBox.Show("Не выбран заказ!", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            var orderWindow = new OrderSingleWindow();
            var orderModel = new OrderSingleModel(SelectedItem, _omsClient);
            orderWindow.DataContext = orderModel;
            orderWindow.ShowDialog();
        }
    }
}
