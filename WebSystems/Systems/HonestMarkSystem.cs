﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using WebSystems.WebClients;

namespace WebSystems.Systems
{
    public class HonestMarkSystem
    {
        private string _emchdOrgInn;
        private X509Certificate2 _certificate;

        public HonestMarkSystem(X509Certificate2 certificate)
        {
            _certificate = certificate;
        }

        public HonestMarkSystem(X509Certificate2 certificate, string emchdOrgInn)
        {
            _emchdOrgInn = emchdOrgInn;
            _certificate = certificate;
        }

        public bool Authorization()
        {
            return HonestMarkClient.GetInstance().Authorization(_certificate, _emchdOrgInn);
        }

        public List<KeyValuePair<string, string>> GetCodesByThePiece(IEnumerable<string> sourceCodes,
            List<KeyValuePair<string, string>> resultCodes, bool isAggregatedCodes = true)
        {
            string[] markedCodes;

            if (isAggregatedCodes)
                markedCodes = HonestMarkClient.GetInstance()
                    .GetAggregatedCodes(ProductGroupsEnum.None, sourceCodes.ToArray());
            else
                markedCodes = sourceCodes.ToArray();

            Func<string, KeyValuePair<string, string>> predicate = s =>
            {
                if (s.Length == 31)
                {
                    var barCode = s.Substring(0, 16).TrimStart('0', '1').TrimStart('0');
                    return new KeyValuePair<string, string>(s, barCode);
                }
                else
                    return new KeyValuePair<string, string>(s, string.Empty);
            };

            IEnumerable<KeyValuePair<string, string>> codes = markedCodes.Select(s => predicate(s));

            resultCodes.AddRange(codes);

            if(sourceCodes.Count() == (codes?.Count() ?? 0))
                resultCodes = resultCodes.Distinct().ToList();

            return resultCodes;
        }

        public async Task<List<KeyValuePair<string, string>>> GetCodesByThePieceAsync(IEnumerable<string> sourceCodes,
            List<KeyValuePair<string, string>> resultCodes, bool isAggregatedCodes = true)
        {
            string[] markedCodes;

            if (isAggregatedCodes)
                markedCodes = await HonestMarkClient.GetInstance()
                    .GetAggregatedCodesAsync(ProductGroupsEnum.None, sourceCodes.ToArray());
            else
                markedCodes = sourceCodes.ToArray();

            Func<string, KeyValuePair<string, string>> predicate = s =>
            {
                if (s.Length == 31)
                {
                    var barCode = s.Substring(0, 16).TrimStart('0', '1').TrimStart('0');
                    return new KeyValuePair<string, string>(s, barCode);
                }
                else
                    return new KeyValuePair<string, string>(s, string.Empty);
            };

            IEnumerable<KeyValuePair<string, string>> codes = markedCodes.Select(s => predicate(s));

            resultCodes.AddRange(codes);

            if (sourceCodes.Count() == (codes?.Count() ?? 0))
                resultCodes = resultCodes.Distinct().ToList();

            return resultCodes;
        }

        public Models.MarkCodeInfo[] GetMarkedCodesInfo(ProductGroupsEnum productGroup, string[] markCodes)
        {
            return HonestMarkClient.GetInstance().GetMarkCodesInfo(productGroup, markCodes);
        }

        public Models.DocumentEdoProcessResultInfo GetEdoDocumentProcessInfo(string documentId)
        {
            return HonestMarkClient.GetInstance().GetEdoDocumentProcessInfo(documentId);
        }

        public string SendDocument(ProductGroupsEnum productGroup, DocumentFormatsEnum documentFormat, string codeDocType, Models.IDocument documentData)
        {
            return HonestMarkClient.GetInstance().CreateDocument(productGroup, documentFormat, codeDocType, documentData);
        }

        public Models.DocumentInfo GetDocumentInfo(ProductGroupsEnum productGroup, string docId)
        {
            return HonestMarkClient.GetInstance().GetDocumentInfo(productGroup, docId);
        }

        public string GetEdoIdByInn(string inn)
        {
            return HonestMarkClient.GetInstance().GetEdoIdByInn(inn);
        }
    }
}
