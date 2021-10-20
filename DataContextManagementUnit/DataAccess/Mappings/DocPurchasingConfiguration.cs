using System.Data.Entity.ModelConfiguration;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt.Mapping
{
    public partial class DocPurchasingConfiguration : EntityTypeConfiguration<DocPurchasing>
    {
        public DocPurchasingConfiguration()
        {
            this
                .HasKey(p => p.Id)
                .ToTable("DOC_PURCHASING", "ABT");

            this
                .Property(p => p.Id)
                .HasColumnName("ID")
                .IsRequired();

            this
                .Property(p => p.IdDocType)
                .HasColumnName("ID_DOC_TYPE");

            this
                .Property(p => p.StoreShipmentDate)
                .HasColumnName("STORE_SHIPMENT_DATE");

            this
                .Property(p => p.StoreShipmentDate)
                .HasColumnName("STORE_SHIPMENT_DATE");

            this
                .Property(p => p.RrShipmentDate)
                .HasColumnName("RR_SHIPMENT_DATE");

            this
                .Property(p => p.IdCity)
                .HasColumnName("ID_CITY");

            this
                .Property(p => p.IdForwarder)
                .HasColumnName("ID_FORWARDER");

            this
                .Property(p => p.IdSupplierJur)
                .HasColumnName("ID_SUPPLIER_JUR");

            this
                .Property(p => p.IdFirm)
                .HasColumnName("ID_FIRM");

            this
                .Property(p => p.IdFilial)
                .HasColumnName("ID_FILIAL");

            this
                .Property(p => p.IdDelivery)
                .HasColumnName("ID_DELIVERY");

            this
                .Property(p => p.IdCurrency)
                .HasColumnName("ID_CURRENCY");

            this
                .Property(p => p.CurrencyRate)
                .HasColumnName("CURRENCY_RATE");

            this
                .Property(p => p.InvoiceCode)
                .HasColumnName("INVOICE_CODE")
                .HasMaxLength(20);

            this
                .Property(p => p.WaybillCode)
                .HasColumnName("WAYBILL_CODE")
                .HasMaxLength(20);

            this
                .Property(p => p.Amount)
                .HasColumnName("AMOUNT");

            this
                .Property(p => p.AmountWithoutVat)
                .HasColumnName("AMOUNT_WITHOUT_VAT");

            this
                .Property(p => p.DiscountSumm)
                .HasColumnName("DISCOUNT_SUMM");

            this
                .Property(p => p.ExpectedDate)
                .HasColumnName("EXPECTED_DATE");

            this
                .Property(p => p.ArrivalDate)
                .HasColumnName("ARRIVAL_DATE");

            this
                .Property(p => p.TraderTransactDate)
                .HasColumnName("TRADER_TRANSACT_DATE");

            this
                .Property(p => p.Comments)
                .HasColumnName("COMMENTS")
                .HasMaxLength(100);

            this
                .Property(p => p.DiscountComments)
                .HasColumnName("DISCOUNT_COMMENTS")
                .HasMaxLength(100);

            this
                .Property(p => p.Weight)
                .HasColumnName("WEIGHT");

            this
                .Property(p => p.Volume)
                .HasColumnName("VOLUME");

            this
                .Property(p => p.InvoicePresence)
                .HasColumnName("INVOICE_PRESENCE");

            this
                .Property(p => p.WaybillPresence)
                .HasColumnName("WAYBILL_PRESENCE");

            this
                .Property(p => p.Subdivision)
                .HasColumnName("SUBDIVISION");

            this
                .Property(p => p.IdDocLink)
                .HasColumnName("ID_DOC_LINK");

            this
                .Property(p => p.UserName)
                .HasColumnName("USER_NAME")
                .HasMaxLength(20);

            this
                .Property(p => p.CreateDate)
                .HasColumnName("CREATE_DATE");

            this
                .Property(p => p.PayDelay)
                .HasColumnName("PAY_DELAY");

            this
                .Property(p => p.Deleted)
                .HasColumnName("DELETED");

            this
                .Property(p => p.InvoiceReceiptDate)
                .HasColumnName("INVOICE_RECEIPT_DATE");

            this
                .Property(p => p.WaybillReceiptDate)
                .HasColumnName("WAYBILL_RECEIPT_DATE");

            this
                .Property(p => p.InvoiceDate)
                .HasColumnName("INVOICE_DATE");

            this
                .Property(p => p.WaybillDate)
                .HasColumnName("WAYBILL_DATE");

            this
                .Property(p => p.Vat20)
                .HasColumnName("VAT_20");

            this
                .Property(p => p.Vat18)
                .HasColumnName("VAT_18");

            this
                .Property(p => p.Vat10)
                .HasColumnName("VAT_10");

            this
                .Property(p => p.Vat0)
                .HasColumnName("VAT_0");

            this
                .Property(p => p.UpdCode)
                .HasColumnName("UPD_CODE")
                .HasMaxLength(20);

            this
                .Property(p => p.UpdDate)
                .HasColumnName("UPD_DATE");

            this
                .Property(p => p.UpdPresence)
                .HasColumnName("UPD_PRESENCE");

            this
                .Property(p => p.UpdReceiptDate)
                .HasColumnName("UPD_RECEIPT_DATE");

            this
                .Property(p => p.InvCode)
                .HasColumnName("INV_CODE")
                .HasMaxLength(20);

            this
                .Property(p => p.WoVat)
                .HasColumnName("WO_VAT");

            this
                .HasRequired(p => p.DocLink)
                .WithMany()
                .HasForeignKey(p => p.IdDocLink)
                .WillCascadeOnDelete(false);

            this
                .HasRequired(p => p.Supplier)
                .WithMany()
                .HasForeignKey(p => p.IdSupplierJur)
                .WillCascadeOnDelete(false);

            this
                .HasRequired(p => p.Firm)
                .WithMany()
                .HasForeignKey(p => p.IdFirm)
                .WillCascadeOnDelete(false);

            OnCreated();
        }

        partial void OnCreated();
    }
}
