using System.Data.Entity.ModelConfiguration;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt.Mapping
{
    public partial class DocEdoPurchasingConfiguration : EntityTypeConfiguration<DocEdoPurchasing>
    {
        public DocEdoPurchasingConfiguration()
        {
            this
                .HasKey(p => p.IdDocEdo)
                .ToTable("DOC_EDO_PURCHASING", "EDI");

            this
                .Property(p => p.IdDocEdo)
                .HasColumnName("ID_DOC_EDO")
                .IsRequired()
                .HasMaxLength(36);

            this
                .Property(p => p.EdoProviderName)
                .HasColumnName("EDO_PROVIDER_NAME")
                .HasMaxLength(100);

            this
                .Property(p => p.DocStatus)
                .HasColumnName("DOC_STATUS");

            this
                .Property(p => p.Name)
                .HasColumnName("NAME")
                .HasMaxLength(500);

            this
                .Property(p => p.IdDocType)
                .HasColumnName("ID_DOC_TYPE");

            this
                .Property(p => p.CreateDate)
                .HasColumnName("DATE_CREATE");

            this
                .Property(p => p.ReceiveDate)
                .HasColumnName("DATE_RECEIVE");

            this
                .Property(p => p.TotalPrice)
                .HasColumnName("TOTAL_PRICE")
                .HasMaxLength(100);

            this
                .Property(p => p.TotalVatAmount)
                .HasColumnName("TOTAL_VAT_AMOUNT")
                .HasMaxLength(100);

            this
                .Property(p => p.SenderInn)
                .HasColumnName("SENDER_INN")
                .HasMaxLength(20);

            this
                .Property(p => p.SenderName)
                .HasColumnName("SENDER_NAME")
                .HasMaxLength(200);

            this
                .Property(p => p.ReceiverInn)
                .HasColumnName("RECEIVER_INN")
                .HasMaxLength(20);

            this
                .Property(p => p.ReceiverName)
                .HasColumnName("RECEIVER_NAME")
                .HasMaxLength(200);

            this
                .Property(p => p.IdDocPurchaising)
                .HasColumnName("ID_DOC_PURCHASING");

            OnCreated();
        }

        partial void OnCreated();
    }
}
