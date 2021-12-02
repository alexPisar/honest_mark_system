using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diadoc.Api.Proto.Documents;

namespace WebSystems.Models
{
    public class DiadocEdoDocument : IEdoSystemDocument<string>
    {
        public Document Document { get; set; }

        public override string EdoId
        {
            get {
                return Document?.MessageId;
            }
            set {
                Document.MessageId = value;
            }
        }

        public string EntityId
        {
            get {
                return Document?.EntityId;
            }
            set {
                Document.EntityId = value;
            }
        }

        public string CounteragentBoxId
        {
            get {
                return Document?.CounteragentBoxId;
            }
            set {
                Document.CounteragentBoxId = value;
            }
        }

        public string FileName
        {
            get {
                return Document?.FileName;
            }
        }

        public string Title
        {
            get {
                return Document?.DocumentNumber;
            }
        }

        public Diadoc.Api.Proto.DocumentType? DocumentType
        {
            get {
                return Document?.DocumentType;
            }
        }

        public DateTime? CreatedDate
        {
            get {
                return Document?.CreationTimestamp;
            }
        }

        public DateTime? DeliveryDate
        {
            get {
                return Document?.DeliveryTimestamp;
            }
        }

        public string DocStatus
        {
            get {
                return Document?.DocflowStatus?.PrimaryStatus?.Severity;
            }
        }

        public string DocStatusText
        {
            get {
                return Document?.DocflowStatus?.PrimaryStatus?.StatusText;
            }
        }
    }
}
