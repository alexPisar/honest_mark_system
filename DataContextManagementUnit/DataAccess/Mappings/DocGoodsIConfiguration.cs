﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Devart Entity Developer tool using Entity Framework DbContext template.
// Code is generated on: 01.10.2020 15:53:46
//
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity.ModelConfiguration;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt.Mapping
{

    public partial class DocGoodsIConfiguration : EntityTypeConfiguration<DocGoodsI>
    {

        public DocGoodsIConfiguration()
        {
            this
                .HasKey(p => p.IdDoc)
                .ToTable("DOC_GOODS_I", "ABT");
            // Properties:
            this
                .Property(p => p.IdDoc)
                    .HasColumnName(@"ID_DOC")
                    .IsRequired()
                    .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            this
                .Property(p => p.IdPriceType)
                    .HasColumnName(@"ID_PRICE_TYPE")
                    .IsRequired();
            this
                .Property(p => p.DiscountRate)
                    .HasColumnName(@"DISCOUNT_RATE")
                    .IsRequired();
            this
                .Property(p => p.DiscountSumm)
                    .HasColumnName(@"DISCOUNT_SUMM")
                    .IsRequired();
            this
                .Property(p => p.TotalSumm)
                    .HasColumnName(@"TOTAL_SUMM")
                    .IsRequired();
            this
                .Property(p => p.IdSeller)
                    .HasColumnName(@"ID_SELLER")
                    .IsRequired();
            this
                .Property(p => p.IdCustomer)
                    .HasColumnName(@"ID_CUSTOMER")
                    .IsRequired();
            this
                .Property(p => p.IdDocReturn)
                    .HasColumnName(@"ID_DOC_RETURN");
            this
                .Property(p => p.ChargeRate)
                    .HasColumnName(@"CHARGE_RATE")
                    .IsRequired();
            this
                .Property(p => p.ChargeSumm)
                    .HasColumnName(@"CHARGE_SUMM")
                    .IsRequired();
            this
                .Property(p => p.IsReturn)
                    .HasColumnName(@"IS_RETURN")
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsFixedLength()
                    .IsUnicode(false);
            this
                .Property(p => p.LockStatus)
                    .HasColumnName(@"LOCK_STATUS")
                    .IsRequired();
            this
                .Property(p => p.TaxSumm)
                    .HasColumnName(@"TAX_SUMM")
                    .IsRequired();
            this
                .Property(p => p.IdSubdivision)
                    .HasColumnName(@"ID_SUBDIVISION");
            OnCreated();
        }

        partial void OnCreated();

    }
}
