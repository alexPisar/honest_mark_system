﻿using System.Data.Entity.ModelConfiguration;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt.Mapping
{
    public partial class RefUserByEdoShipperConfiguration : EntityTypeConfiguration<RefUserByEdoShipper>
    {
        public RefUserByEdoShipperConfiguration()
        {
            this
                .HasKey(r => new { r.IdCustomer, r.UserName})
                .ToTable("REF_USERS_BY_EDO_SHIPPERS", "EDI");

            this
                .Property(r => r.IdCustomer)
                .HasColumnName("ID_CUSTOMER");

            this
                .Property(r => r.UserName)
                .HasColumnName("USER_NAME")
                .HasMaxLength(100);

            OnCreated();
        }

        partial void OnCreated();
    }
}
