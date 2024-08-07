﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.ModelBase;
using DataContextManagementUnit.DataAccess.Contexts.Abt;
using WebSystems.Models;

namespace HonestMarkSystem.Models
{
    public class ChangeMarkedCodesModel : ViewModelBase
    {
        private ConsignorOrganization _myOrganization;
        KeyValuePair<WebSystems.ReasonOfWithdrawalFromTurnover, string> _selectedReason;
        KeyValuePair<WebSystems.ProductGroupsEnum, string> _selectedProductGroup;

        public ChangeMarkedCodesModel(List<DocGoodsDetailsLabels> markedCodes, ConsignorOrganization myOrganization)
        {
            _myOrganization = myOrganization;
            MarkedCodes = markedCodes;
            SelectedCodes = new List<DocGoodsDetailsLabels>();

            AllReasons = typeof(WebSystems.ReasonOfWithdrawalFromTurnover).GetEnumValues()
                .Cast<WebSystems.ReasonOfWithdrawalFromTurnover>()
                .Select(m => 
                {
                    var memberInfo = typeof(WebSystems.ReasonOfWithdrawalFromTurnover).GetMember(m.ToString())[0];
                    var description = memberInfo.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)?.FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
                    return new KeyValuePair<WebSystems.ReasonOfWithdrawalFromTurnover, string>(m, description?.Description);
                });

            SelectedReason = AllReasons.First(a => a.Key == WebSystems.ReasonOfWithdrawalFromTurnover.DamageLoss);

            _selectedProductGroup = default(KeyValuePair<WebSystems.ProductGroupsEnum, string>);
        }

        public List<DocGoodsDetailsLabels> MarkedCodes { get; set; }
        public List<DocGoodsDetailsLabels> SelectedCodes { get; set; }

        public IEnumerable<KeyValuePair<WebSystems.ReasonOfWithdrawalFromTurnover, string>> AllReasons { get; set; }
        public KeyValuePair<WebSystems.ReasonOfWithdrawalFromTurnover, string> SelectedReason
        {
            get {
                return _selectedReason;
            }
            set {
                _selectedReason = value;
                OnPropertyChanged("SelectedReason");
                OnPropertyChanged("IsOtherReason");
            }
        }

        public IEnumerable<KeyValuePair<WebSystems.ProductGroupsEnum, string>> AllProductGroups { get; set; }
        public KeyValuePair<WebSystems.ProductGroupsEnum, string> SelectedProductGroup {
            get {
                return _selectedProductGroup;
            }
            set {
                _selectedProductGroup = value;
                OnPropertyChanged("SelectedProductGroup");
            }
        }

        public string OtherReason { get; set; }
        public string DocumentName { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime? DocumentDate { get; set; }

        public bool IsOtherReason => SelectedReason.Key == WebSystems.ReasonOfWithdrawalFromTurnover.Other;

        public bool WithdrawalFromTurnover()
        {
            _log.Log("WithdrawalFromTurnover : вывод из оборота кодов маркировки");
            string documentId = null;
            try
            {
                if (string.IsNullOrEmpty(DocumentNumber))
                {
                    System.Windows.MessageBox.Show("Не указан номер первичного документа.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return false;
                }

                if (DocumentDate == null)
                {
                    System.Windows.MessageBox.Show("Не указана дата первичного документа.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return false;
                }

                if (string.IsNullOrEmpty(DocumentName))
                {
                    System.Windows.MessageBox.Show("Не указано наименование первичного документа.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return false;
                }

                var document = new WithdrawalFromTurnoverDocument
                {
                    Inn = _myOrganization.OrgInn,
                    ActionDateStr = DateTime.Now.ToString("yyyy-MM-dd"),
                    Action = _selectedReason.Key,
                    DocumentType = "OTHER",
                    DocumentNumber = this.DocumentNumber,
                    DocumentDateStr = DocumentDate.Value.ToString("yyyy-MM-dd"),
                    PrimaryDocumentCustomName = DocumentName
                };

                if (IsOtherReason)
                    document.WithdrawalTypeOther = OtherReason;

                document.Products = SelectedCodes.Select(s => new WithdrawalFromTurnoverDetail { Cis = s.DmLabel }).ToArray();
                documentId = _myOrganization.HonestMarkSystem.SendDocument(SelectedProductGroup.Key, WebSystems.DocumentFormatsEnum.Manual, "LK_RECEIPT", document);

                DocumentInfo docInfo = null;
                short i = 0;
                Exception checkStatusException = null;

                do
                {
                    try
                    {
                        System.Threading.Thread.Sleep(1000);
                        docInfo = _myOrganization.HonestMarkSystem.GetDocumentInfo(SelectedProductGroup.Key, documentId);
                        checkStatusException = null;
                    }
                    catch(Exception ex)
                    {
                        checkStatusException = ex;
                        i++;
                    }
                }
                while ((i < 100 && checkStatusException != null) || docInfo?.Status == WebSystems.DocumentProcessStatusesEnum.InProgress);

                if (checkStatusException != null)
                    throw checkStatusException;

                if (docInfo == null)
                    throw new Exception("Не удалось получить статус обрабатываемого документа");

                if (docInfo.Status == WebSystems.DocumentProcessStatusesEnum.CheckedNotOk)
                    throw new Exception("Документ обработан с ошибками.");

                if (docInfo.Status == WebSystems.DocumentProcessStatusesEnum.ParseError)
                    throw new Exception("Ошибка парсинга в документе.");

                if (docInfo.Status == WebSystems.DocumentProcessStatusesEnum.ProcessingError)
                    throw new Exception("Произошла ошибка обработки.");

                _log.Log($"WithdrawalFromTurnover : завершено со статусом {docInfo.Status}");
                return docInfo.Status == WebSystems.DocumentProcessStatusesEnum.Accepted || docInfo.Status == WebSystems.DocumentProcessStatusesEnum.CheckedOk;
            }
            catch(System.Net.WebException webEx)
            {
                string errorMessage = _log.GetRecursiveInnerException(webEx);

                if (documentId != null)
                    _log.Log($"Ошибка ввода в оборот на удалённом сервере, ID документа - {documentId}\n{errorMessage}");
                else
                    _log.Log($"Ошибка ввода в оборот на удалённом сервере.\n{errorMessage}");

                throw webEx;
            }
            catch(Exception ex)
            {
                string errorMessage = _log.GetRecursiveInnerException(ex);

                if(documentId != null)
                    _log.Log($"Ошибка ввода в оборот, ID документа - {documentId}\n{errorMessage}");
                else
                    _log.Log($"Ошибка ввода в оборот.\n{errorMessage}");

                throw ex;
            }
        }
    }
}
