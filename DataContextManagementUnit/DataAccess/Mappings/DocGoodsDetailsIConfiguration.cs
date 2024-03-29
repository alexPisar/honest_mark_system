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

    public partial class DocGoodsDetailsIConfiguration : EntityTypeConfiguration<DocGoodsDetailsI>
    {

        public DocGoodsDetailsIConfiguration()
        {
            this
                .HasKey(p => new { p.IdDoc, p.IdGood })
                .ToTable("DOC_GOODS_DETAILS_I", "ABT");
            // Properties:
            this
                .Property(p => p.IdDoc)
                    .HasColumnName(@"ID_DOC")
                    .IsRequired()
                    .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            this
                .Property(p => p.IdGood)
                    .HasColumnName(@"ID_GOOD")
                    .IsRequired()
                    .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            this
                .Property(p => p.Quantity)
                    .HasColumnName(@"QUANTITY")
                    .IsRequired();
            this
                .Property(p => p.Price)
                    .HasColumnName(@"PRICE")
                    .IsRequired();
            this
                .Property(p => p.IdItem)
                    .HasColumnName(@"ID_ITEM")
                    .IsRequired();
            this
                .Property(p => p.ItemQuantity)
                    .HasColumnName(@"ITEM_QUANTITY")
                    .IsRequired();
            this
                .Property(p => p.ItemPo)
                    .HasColumnName(@"ITEM_POS");
            this
                .Property(p => p.DiscountRate)
                    .HasColumnName(@"DISCOUNT_RATE")
                    .IsRequired();
            this
                .Property(p => p.DiscountSumm)
                    .HasColumnName(@"DISCOUNT_SUMM")
                    .IsRequired();
            this
                .Property(p => p.ChargeRate)
                    .HasColumnName(@"CHARGE_RATE")
                    .IsRequired();
            this
                .Property(p => p.ChargeSumm)
                    .HasColumnName(@"CHARGE_SUMM")
                    .IsRequired();
            this
                .Property(p => p.LockStatus)
                    .HasColumnName(@"LOCK_STATUS")
                    .IsRequired();
            this
                .Property(p => p.TaxRate)
                    .HasColumnName(@"TAX_RATE")
                    .IsRequired();
            this
                .Property(p => p.TaxSumm)
                    .HasColumnName(@"TAX_SUMM")
                    .IsRequired();
            OnCreated();
        }

        partial void OnCreated();

    }
}
