﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt
{
    public partial class DocEdoPurchasing
    {
        public DocEdoPurchasing()
        {
            OnCreated();
        }

        #region Properties
        public virtual string IdDocEdo { get; set; }

        public virtual string EdoProviderName { get; set; }

        public virtual int? DocStatus { get; set; }

        public virtual string Name { get; set; }

        public virtual int? IdDocType { get; set; }

        public virtual DateTime? CreateDate { get; set; }

        public virtual DateTime? ReceiveDate { get; set; }

        public virtual string TotalPrice { get; set; }

        public virtual string TotalVatAmount { get; set; }

        public virtual string SenderInn { get; set; }

        public virtual string SenderName { get; set; }

        public virtual string ReceiverInn { get; set; }

        public virtual string ReceiverName { get; set; }

        public virtual decimal? IdDocPurchasing { get; set; }
        #endregion

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }
}
