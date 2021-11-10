﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContextManagementUnit.DataAccess.Contexts.Abt;
using HonestMarkSystem.Interfaces;
using WebSystems.Models;
using Reporter.Reports;

namespace HonestMarkSystem.Implementations
{
    public class EdoLiteToDataBase : IEdoDataBaseAdapter<AbtDbContext>
    {
        private const string providerName = "EDO LITE";

        private string _orgName;
        private string _orgInn;
        private AbtDbContext _abt;
        private List<DocEdoPurchasing> _documents;

        public void InitializeContext()
        {
            _abt = new AbtDbContext();
            _documents = _abt.DocEdoPurchasings
                .Where(d => d.EdoProviderName == providerName)
                .ToList();
        }

        public object AddDocumentToDataBase(IEdoSystemDocument<string> document, byte[] content, WebSystems.DocumentInOutType inOutType = WebSystems.DocumentInOutType.None)
        {
            var doc = (EdoLiteDocuments)document;

            var reporterDll = new Reporter.ReporterDll();
            var report = reporterDll.ParseDocument<UniversalTransferSellerDocument>(content);

            var newDocInDb = new DocEdoPurchasing()
            {
                IdDocEdo = doc.EdoId,
                EdoProviderName = providerName,
                CreateDate = doc.CreateDateTime,
                ReceiveDate = doc.Date,
                DocStatus = doc.Status,
                Name = doc.Number,
                TotalPrice = doc.TotalPrice,
                TotalVatAmount = doc.TotalVatAmount,
                IdDocType = doc.DocType,
                SenderEdoId = report.SenderEdoId,
                ReceiverEdoId = report.ReceiverEdoId,
                SenderEdoOrgName = report.EdoProviderOrgName,
                SenderEdoOrgInn = report.ProviderInn,
                SenderEdoOrgId = report.EdoId,
                FileName = report.FileName
            };

            if (inOutType == WebSystems.DocumentInOutType.Inbox)
            {
                newDocInDb.SenderInn = doc?.Sender?.Inn.ToString();
                newDocInDb.SenderName = doc?.Sender?.Name;
                newDocInDb.ReceiverInn = _orgInn;
                newDocInDb.ReceiverName = _orgName;
            }
            else if (inOutType == WebSystems.DocumentInOutType.Outbox)
            {
                newDocInDb.SenderInn = _orgInn;
                newDocInDb.SenderName = _orgName;
                newDocInDb.ReceiverInn = doc?.Recipient?.Inn.ToString();
                newDocInDb.ReceiverName = doc?.Recipient?.Name;
            }

            _abt.DocEdoPurchasings.Add(newDocInDb);
            _documents.Add(newDocInDb);

            return newDocInDb;
        }

        public bool ExistsDocumentInDataBase(IEdoSystemDocument<string> document)
        {
            return _documents.Exists(d => d.IdDocEdo == document.EdoId);
        }

        public object GetDocumentFromDb(IEdoSystemDocument<string> document)
        {
            return _documents.FirstOrDefault(d => d.IdDocEdo == document.EdoId);
        }

        public object[] GetAllDocuments()
        {
            return _documents.ToArray();
        }

        public object[] GetPurchasingDocuments()
        {
            return _abt.DocPurchasings
                .Where(d => d.Firm != null && d.Firm.Customer != null && d.Firm.Customer.Inn == _orgInn)
                .ToArray();
        }

        public void SaveOrgData(string orgInn, string orgName)
        {
            _orgInn = orgInn;
            _orgName = orgName;
        }

        public void SaveMarkedCodes(decimal idDocPurchasing, KeyValuePair<string, string>[] markedCodesByBar)
        {
            var docPurchasing = _abt.DocPurchasings.FirstOrDefault(d => d.Id == idDocPurchasing);

            if (docPurchasing == null)
                throw new Exception("Не найден документ закупок в базе.");

            if(docPurchasing.IdDocLink == null)
                throw new Exception("Для документа закупок не найден трейдер документ.");

            var idDoc = docPurchasing.IdDocLink.Value;

            foreach(var code in markedCodesByBar)
            {
                var markedCode = code.Key;

                if (_abt.DocGoodsDetailsLabels.FirstOrDefault(l => l.IdDoc == idDoc && l.DmLabel == markedCode) != null)
                    continue;

                var barCode = code.Value;
                var idGood = _abt.RefBarCodes?
                    .FirstOrDefault(b => b.BarCode == barCode && b.IsPrimary == false)?
                    .IdGood;

                if (idGood == null)
                    throw new Exception($"Товара со штрихкодом {barCode} нет в базе данных.");

                var label = new DocGoodsDetailsLabels
                {
                    IdDoc = idDoc,
                    DmLabel = markedCode,
                    IdGood = idGood.Value,
                    InsertDateTime = DateTime.Now
                };

                _abt.DocGoodsDetailsLabels.Add(label);
            }
        }

        public void Commit()
        {
            _abt.SaveChanges();
        }

        public void Rollback()
        {
            InitializeContext();
        }
    }
}
