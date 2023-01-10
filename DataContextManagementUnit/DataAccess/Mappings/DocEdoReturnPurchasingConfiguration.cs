using System.Data.Entity.ModelConfiguration;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt.Mapping
{
    public partial class DocEdoReturnPurchasingConfiguration : EntityTypeConfiguration<DocEdoReturnPurchasing>
    {
        public DocEdoReturnPurchasingConfiguration()
        {
            this
                .HasKey(r => r.Id)
                .ToTable("DOC_EDO_RETURN_PURCHASING", "EDI");

            this
                .Property(r => r.Id)
                .HasColumnName("ID")
                .IsRequired()
                .HasMaxLength(36);

            this
                .Property(r => r.IdDocJournal)
                .HasColumnName("ID_DOC_JOURNAL");

            this
                .Property(r => r.MessageId)
                .HasColumnName("MESSAGE_ID")
                .HasMaxLength(36);

            this
                .Property(r => r.EntityId)
                .HasColumnName("ENTITY_ID")
                .HasMaxLength(36);

            this
                .Property(r => r.SellerFileName)
                .HasColumnName("SELLER_FILE_NAME")
                .HasMaxLength(36);

            this
                .Property(r => r.BuyerFileName)
                .HasColumnName("BUYER_FILE_NAME")
                .HasMaxLength(36);

            this
                .Property(r => r.DocDate)
                .HasColumnName("DOC_DATE")
                .IsRequired();

            this
                .Property(r => r.UserName)
                .HasColumnName("USER_NAME")
                .HasMaxLength(100);

            this
                .Property(r => r.SenderInn)
                .HasColumnName("SENDER_INN")
                .HasMaxLength(20);

            this
                .Property(r => r.SenderName)
                .HasColumnName("SENDER_NAME")
                .HasMaxLength(200);

            this
                .Property(r => r.ReceiverInn)
                .HasColumnName("RECEIVER_INN")
                .HasMaxLength(20);

            this
                .Property(r => r.ReceiverName)
                .HasColumnName("RECEIVER_NAME")
                .HasMaxLength(200);

            this
                .Property(r => r.DocStatus)
                .HasColumnName("DOC_STATUS")
                .IsRequired();

            this
                .Property(r => r.ErrorMessage)
                .HasColumnName("ERROR_MESSAGE")
                .HasMaxLength(1000);

            this
                .HasRequired(r => r.Status)
                .WithMany()
                .HasForeignKey(r => r.DocStatus)
                .WillCascadeOnDelete(false);

            OnCreated();
        }

        partial void OnCreated();
    }
}
