using System.Data.Entity.ModelConfiguration;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt.Mapping
{
    public partial class DocGoodsMarkShipmentConfiguration : EntityTypeConfiguration<DocGoodsMarkShipment>
    {
        public DocGoodsMarkShipmentConfiguration()
        {
            this
                .HasKey(d => new { d.IdDocJournal, d.IdDocument })
                .ToTable("DOC_GOODS_MARK_SHIPMENTS", "ABT");

            this
                .Property(d => d.IdDocJournal)
                .HasColumnName("ID_DOC_JOURNAL");

            this
                .Property(d => d.SenderInn)
                .HasColumnName("INN_SENDER")
                .HasMaxLength(20);

            this
                .Property(d => d.ReceiverInn)
                .HasColumnName("INN_RECEIVER")
                .HasMaxLength(20);

            this
                .Property(d => d.IdDocument)
                .HasColumnName("ID_DOCUMENT")
                .HasMaxLength(50);

            this
                .Property(d => d.DocCreateDate)
                .HasColumnName("DOC_CREATE_DATE");

            this
                .Property(d => d.TransferDate)
                .HasColumnName("TRANSFER_DATE");

            this
                .Property(d => d.DocStatus)
                .HasColumnName("DOC_STATUS");

            this
                .Property(d => d.ErrorMessage)
                .HasColumnName("ERROR_MESSAGE")
                .HasMaxLength(200);

            this
                .Property(d => d.IdReceiveDocument)
                .HasColumnName("ID_RECEIVE_DOCUMENT")
                .HasMaxLength(50);

            OnCreated();
        }

        partial void OnCreated();
    }
}
