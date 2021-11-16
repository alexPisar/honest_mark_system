using System.Data.Entity.ModelConfiguration;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt.Mapping
{
    public partial class RefEdoStatusConfiguration : EntityTypeConfiguration<RefEdoStatus>
    {
        public RefEdoStatusConfiguration()
        {
            this
                .HasKey(r => r.Id)
                .ToTable("REF_EDO_STATUSES", "EDI");

            this
                .Property(r => r.Id)
                .HasColumnName("ID");

            this
                .Property(r => r.Name)
                .HasColumnName("NAME")
                .HasMaxLength(100);

            OnCreated();
        }

        partial void OnCreated();
    }
}
