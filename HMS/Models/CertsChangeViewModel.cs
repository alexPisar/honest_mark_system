using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.ModelBase;
using System.Security.Cryptography.X509Certificates;
using DataContextManagementUnit.DataAccess.Contexts.Abt;
using WebSystems.WebClients;
using UtilitesLibrary.Service;
using WebSystems;
using WebSystems.EdoSystems;

namespace HonestMarkSystem.Models
{
    public class CertsChangeViewModel : ListViewModel<X509Certificate2>, Interfaces.IAuthView
    {
        public RelayCommand CertsChangeViewLoaded => new RelayCommand(o => { SetAuthCertsList(); });

        public IEdoSystem EdoSystem { get; set; }

        public WebSystems.Systems.HonestMarkSystem MarkSystem { get; set; }

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

                EdoSystem = new EdoLiteSystem(SelectedItem);
                bool result = EdoSystem.Authorization();

                MarkSystem = new WebSystems.Systems.HonestMarkSystem(SelectedItem);
                result = result && MarkSystem.Authorization();

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
