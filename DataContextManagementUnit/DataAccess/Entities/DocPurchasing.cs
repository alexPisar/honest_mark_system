using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt
{
    public partial class DocPurchasing
    {
        public DocPurchasing()
        {
            OnCreated();
        }

        #region Properties
        public virtual decimal Id { get; set; }

        public virtual decimal? IdDocType { get; set; }

        public virtual DateTime? StoreShipmentDate { get; set; }

        public virtual DateTime? RrShipmentDate { get; set; }

        public virtual decimal? IdCity { get; set; }

        public virtual decimal? IdForwarder { get; set; }

        public virtual decimal? IdSupplierJur { get; set; }

        public virtual decimal? IdFirm { get; set; }

        public virtual decimal? IdFilial { get; set; }

        public virtual decimal? IdDelivery { get; set; }

        public virtual decimal? IdCurrency { get; set; }

        public virtual decimal? CurrencyRate { get; set; }

        public virtual string InvoiceCode { get; set; }

        public virtual string WaybillCode { get; set; }

        public virtual decimal? Amount { get; set; }

        public virtual decimal? AmountWithoutVat { get; set; }

        public virtual decimal? DiscountSumm { get; set; }

        public virtual DateTime? ExpectedDate { get; set; }

        public virtual DateTime? ArrivalDate { get; set; }

        public virtual DateTime? TraderTransactDate { get; set; }

        public virtual string Comments { get; set; }

        public virtual string DiscountComments { get; set; }

        public virtual decimal? Weight { get; set; }

        public virtual decimal? Volume { get; set; }

        public virtual decimal? InvoicePresence { get; set; }

        public virtual decimal? WaybillPresence { get; set; }

        public virtual decimal? Subdivision { get; set; }

        public virtual decimal? IdDocLink { get; set; }

        public virtual string UserName { get; set; }

        public virtual DateTime? CreateDate { get; set; }

        public virtual int? PayDelay { get; set; }

        public virtual int? Deleted { get; set; }

        public virtual DateTime? InvoiceReceiptDate { get; set; }

        public virtual DateTime? WaybillReceiptDate { get; set; }

        public virtual DateTime? InvoiceDate { get; set; }

        public virtual DateTime? WaybillDate { get; set; }

        public virtual decimal? Vat20 { get; set; }

        public virtual decimal? Vat18 { get; set; }

        public virtual decimal? Vat10 { get; set; }

        public virtual decimal? Vat0 { get; set; }

        public virtual string UpdCode { get; set; }

        public virtual DateTime? UpdDate { get; set; }

        public virtual decimal? UpdPresence { get; set; }

        public virtual DateTime? UpdReceiptDate { get; set; }

        public virtual string InvCode { get; set; }

        public virtual int? WoVat { get; set; }
        #endregion

        #region Navigation Properties
        public virtual DocJournal DocLink { get; set; }

        public virtual RefContractor Supplier { get; set; }

        public virtual RefContractor Firm { get; set; }
        #endregion

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }
}
