using System.Data.Entity.ModelConfiguration;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt.Mapping
{
    public partial class RefHonestMarkProductGroupConfiguration : EntityTypeConfiguration<RefHonestMarkProductGroup>
    {
        public RefHonestMarkProductGroupConfiguration()
        {
            this
                .HasKey(r => r.Id)
                .ToTable("REF_HONEST_MARK_PRODUCT_GROUPS", "EDI");

            this
                .Property(r => r.Id)
                .HasColumnName("ID");

            this
                .Property(r => r.Name)
                .HasColumnName("NAME")
                .HasMaxLength(50);

            this
                .Property(r => r.Description)
                .HasColumnName("DESCRIPTION")
                .HasMaxLength(500);


            OnCreated();
        }

        partial void OnCreated();
    }
}
