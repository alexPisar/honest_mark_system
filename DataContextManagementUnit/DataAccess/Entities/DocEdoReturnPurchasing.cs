using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt
{
    public partial class DocEdoReturnPurchasing
    {
        public DocEdoReturnPurchasing()
        {
            OnCreated();
        }

        #region Properties

        public virtual string Id { get; set; }

        public virtual decimal? IdDocJournal { get; set; }

        public virtual string MessageId { get; set; }

        public virtual string EntityId { get; set; }

        public virtual string SellerFileName { get; set; }

        public virtual string BuyerFileName { get; set; }

        public virtual DateTime DocDate { get; set; }

        public virtual string UserName { get; set; }

        public virtual string SenderInn { get; set; }

        public virtual string SenderName { get; set; }

        public virtual string ReceiverInn { get; set; }

        public virtual string ReceiverName { get; set; }

        public virtual int DocStatus { get; set; }

        public virtual string ErrorMessage { get; set; }

        #endregion

        #region Navigation Properties

        public virtual RefEdoStatus Status { get; set; }

        #endregion

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }
}
