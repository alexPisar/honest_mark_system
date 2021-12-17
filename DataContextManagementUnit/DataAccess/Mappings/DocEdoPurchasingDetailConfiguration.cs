using System.Data.Entity.ModelConfiguration;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt.Mapping
{
    public partial class DocEdoPurchasingDetailConfiguration : EntityTypeConfiguration<DocEdoPurchasingDetail>
    {
        public DocEdoPurchasingDetailConfiguration()
        {
            this
                .HasKey(d => new { d.BarCode, d.IdDocEdoPurchasing })
                .ToTable("DOC_EDO_PURCHASING_DETAILS", "EDI");

            this
                .Property(d => d.IdDocEdoPurchasing)
                .HasColumnName("ID_DOC_EDO_PURCHASING")
                .HasMaxLength(36);

            this
                .Property(d => d.BarCode)
                .HasColumnName("BAR_CODE")
                .HasMaxLength(20);

            this
                .Property(d => d.Description)
                .HasColumnName("DESCRIPTION")
                .HasMaxLength(500);

            this
                .Property(d => d.Quantity)
                .HasColumnName("QUANTITY");

            this
                .Property(d => d.Price)
                .HasColumnName("PRICE");

            this
                .Property(d => d.Subtotal)
                .HasColumnName("SUBTOTAL");

            this
                .Property(d => d.TaxAmount)
                .HasColumnName("TAX_AMOUNT");

            OnCreated();
        }

        partial void OnCreated();
    }
}
