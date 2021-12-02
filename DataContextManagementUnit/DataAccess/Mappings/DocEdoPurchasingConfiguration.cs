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
                .Property(p => p.IdDocPurchasing)
                .HasColumnName("ID_DOC_PURCHASING");

            this
                .Property(p => p.SenderEdoId)
                .HasColumnName("SENDER_EDO_ID")
                .HasMaxLength(100);

            this
                .Property(p => p.ReceiverEdoId)
                .HasColumnName("RECEIVER_EDO_ID")
                .HasMaxLength(100);

            this
                .Property(p => p.SenderEdoOrgName)
                .HasColumnName("SENDER_EDO_ORG_NAME")
                .HasMaxLength(200);

            this
                .Property(p => p.SenderEdoOrgInn)
                .HasColumnName("SENDER_EDO_ORG_INN")
                .HasMaxLength(20);

            this
                .Property(p => p.SenderEdoOrgId)
                .HasColumnName("SENDER_EDO_ORG_ID")
                .HasMaxLength(10);

            this
                .Property(p => p.FileName)
                .HasColumnName("FILE_NAME")
                .HasMaxLength(500);

            this
                .Property(p => p.SignatureFileName)
                .HasColumnName("SIGNATURE_FILE_NAME")
                .HasMaxLength(500);

            this
                .Property(p => p.ErrorMessage)
                .HasColumnName("ERROR_MESSAGE")
                .HasMaxLength(1000);

            this
                .Property(p => p.UserName)
                .HasColumnName("USER_NAME")
                .HasMaxLength(100);

            this
                .Property(p => p.CounteragentEdoBoxId)
                .HasColumnName("COUNTERAGENT_EDO_BOX_ID")
                .HasMaxLength(128);

            this
                .HasOptional(p => p.Status)
                .WithMany()
                .HasForeignKey(p => p.DocStatus)
                .WillCascadeOnDelete(false);

            OnCreated();
        }

        partial void OnCreated();
    }
}
