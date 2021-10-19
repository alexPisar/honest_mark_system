﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.ModelConfiguration;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt.Mapping
{
    public partial class RefCityConfiguration : EntityTypeConfiguration<RefCity>
    {
        public RefCityConfiguration()
        {
            this
                .HasKey( c => c.Id )
                .ToTable( "REF_CITIES", "ABT" );

            this
                .Property( c => c.Id )
                .HasColumnName( "ID" )
                .IsRequired()
                .HasDatabaseGeneratedOption( System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None );

            this
                .Property( c => c.Name )
                .HasColumnName( "NAME" )
                .HasMaxLength( 128 )
                .IsUnicode( false );

            OnCreated();
        }

        partial void OnCreated();
    }
}
