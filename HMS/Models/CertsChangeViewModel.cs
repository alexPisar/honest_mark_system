using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.ModelBase;
using System.Security.Cryptography.X509Certificates;
using WebSystems.WebClients;
using UtilitesLibrary.Service;

namespace HonestMarkSystem.Models
{
    public class CertsChangeViewModel : ListViewModel<X509Certificate2>, Interfaces.IAuthView
    {
        public RelayCommand CertsChangeViewLoaded => new RelayCommand(o => { SetAuthCertsList(); });

        private void UpdateProperties()
        {
            OnPropertyChanged("ItemsList");
            OnPropertyChanged("SelectedItem");
        }

        public void SetAuthCertsList()
        {
            var certs = new CryptoUtil().GetPersonalCertificates();
            ItemsList = new System.Collections.ObjectModel.ObservableCollection<X509Certificate2>(certs);
            SelectedItem = null;
            UpdateProperties();
        }

        public bool Authentification()
        {
            try
            {
                if (SelectedItem == null)
                {
                    System.Windows.MessageBox.Show(
                        "Не выбран сертификат.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

                    return false;
                }

                var edoWebClient = EdoLiteClient.GetInstance();
                bool result = edoWebClient.Authorization(SelectedItem);
                return result;
            }
            catch(System.Net.WebException webEx)
            {
                string errorMessage = _log.GetRecursiveInnerException(webEx);
                _log.Log($"Произошла ошибка на удалённом сервере: {errorMessage}");

                var errorsWindow = new ErrorsWindow("Произошла ошибка на удалённом сервере.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();

                return false;
            }
            catch (Exception ex)
            {
                string errorMessage = _log.GetRecursiveInnerException(ex);
                _log.Log(errorMessage);

                var errorsWindow = new ErrorsWindow("Произошла ошибка.", new List<string>(new string[] { errorMessage }));
                errorsWindow.ShowDialog();

                return false;
            }
        }
    }
}
