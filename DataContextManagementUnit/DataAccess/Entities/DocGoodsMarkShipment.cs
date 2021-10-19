using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt
{
    public partial class DocGoodsMarkShipment
    {
        public DocGoodsMarkShipment()
        {
            OnCreated();
        }

        #region Properties
        public virtual decimal IdDocJournal { get; set; }

        public virtual string SenderInn { get; set; }

        public virtual string ReceiverInn { get; set; }

        public virtual string IdDocument { get; set; }

        public virtual string IdReceiveDocument { get; set; }

        public virtual global::System.DateTime DocCreateDate { get; set; }

        public virtual global::System.DateTime TransferDate { get; set; }

        public virtual int? DocStatus { get; set; }

        public virtual string ErrorMessage { get; set; }
        #endregion

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }
}
