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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Core;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt
{
    public partial class AbtDbContext : DbContext
    {
        #region Constructors

        /// <summary>
        /// Initialize a new AbtDbContext object.
        /// </summary>
        public AbtDbContext() :
                base(GetDefaultConnection(), true)
        {
            Configure();
        }
		
        /// <summary>
        /// Initializes a new AbtDbContext object using the connection string found in the 'AbtDbContext' section of the application configuration file.
        /// </summary>
        public AbtDbContext(string nameOrConnectionString) :
                base(nameOrConnectionString)
        {
            Configure();
        }

        /// <summary>
        /// Initialize a new AbtDbContext object.
        /// </summary>
        public AbtDbContext(DbConnection existingConnection, bool contextOwnsConnection) :
                base(existingConnection, contextOwnsConnection)
        {
            Configure();
        }

        /// <summary>
        /// Initialize a new AbtDbContext object.
        /// </summary>
        public AbtDbContext(ObjectContext objectContext, bool dbContextOwnsObjectContext) :
                base(objectContext, dbContextOwnsObjectContext)
        {
            Configure();
        }

        /// <summary>
        /// Initialize a new AbtDbContext object.
        /// </summary>
        public AbtDbContext(string nameOrConnectionString, DbCompiledModel model) :
                base(nameOrConnectionString, model)
        {
            Configure();
        }

        /// <summary>
        /// Initialize a new AbtDbContext object.
        /// </summary>
        public AbtDbContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection) :
                base(existingConnection, model, contextOwnsConnection)
        {
            Configure();
        }

        private void Configure()
        {
            this.Configuration.AutoDetectChangesEnabled = true;
            this.Configuration.LazyLoadingEnabled = true;
            this.Configuration.ProxyCreationEnabled = true;
            this.Configuration.ValidateOnSaveEnabled = true;
            this.Database.CommandTimeout = 300;
        }


        #endregion

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new Mapping.RefItemConfiguration());
            modelBuilder.Configurations.Add(new Mapping.RefGoodConfiguration());
            modelBuilder.Configurations.Add(new Mapping.RefContractorConfiguration());
            modelBuilder.Configurations.Add(new Mapping.RefBarCodeConfiguration());
            modelBuilder.Configurations.Add(new Mapping.DocJournalConfiguration());
            modelBuilder.Configurations.Add(new Mapping.DocGoodsDetailConfiguration());
            modelBuilder.Configurations.Add(new Mapping.DocGoodConfiguration());
            modelBuilder.Configurations.Add(new Mapping.DocGoodsDetailsIConfiguration());
            modelBuilder.Configurations.Add(new Mapping.DocGoodsIConfiguration());
			modelBuilder.Configurations.Add( new Mapping.RefCountryConfiguration() );
            modelBuilder.Configurations.Add( new Mapping.RefCityConfiguration() );
            modelBuilder.Configurations.Add( new Mapping.RefDistrictConfiguration() );
            modelBuilder.Configurations.Add( new Mapping.RefChannelConfiguration() );
            modelBuilder.Configurations.Add( new Mapping.RefContractorAgentConfiguration() );
            modelBuilder.Configurations.Add(new Mapping.RefAgentConfiguration());
            modelBuilder.Configurations.Add(new Mapping.RefCustomerConfiguration());
            modelBuilder.Configurations.Add(new Mapping.DocGoodsDetailsLabelsConfiguration());
            modelBuilder.Configurations.Add(new Mapping.DocGoodsMarkShipmentConfiguration());
            modelBuilder.Configurations.Add(new Mapping.DocPurchasingConfiguration());
            modelBuilder.Configurations.Add(new Mapping.DocEdoPurchasingConfiguration());

            CustomizeMapping(modelBuilder);
        }

        partial void CustomizeMapping(DbModelBuilder modelBuilder);

		public virtual DbSet<RefItem> RefItems { get; set; }
        public virtual DbSet<RefGood> RefGoods { get; set; }
        public virtual DbSet<RefContractor> RefContractors { get; set; }
        public virtual DbSet<RefBarCode> RefBarCodes { get; set; }
        public virtual DbSet<DocJournal> DocJournals { get; set; }
        public virtual DbSet<DocGoodsDetail> DocGoodsDetails { get; set; }
        public virtual DbSet<DocGood> DocGoods { get; set; }
        public virtual DbSet<DocGoodsDetailsI> DocGoodsDetailsIs { get; set; }
        public virtual DbSet<DocGoodsI> DocGoodsIs { get; set; }
		public virtual DbSet<RefCountry> RefCountries { get; set; }
        public virtual DbSet<RefCity> RefCities { get; set; }
        public virtual DbSet<RefDistrict> RefDistricts { get; set; }
        public virtual DbSet<RefChannel> RefChannels { get; set; }
        public virtual DbSet<RefContractorAgent> RefContractorAgents { get; set; }
        public virtual DbSet<RefAgent> RefAgents { get; set; }
        public virtual DbSet<RefCustomer> RefCustomers { get; set; }
        public virtual DbSet<DocGoodsDetailsLabels> DocGoodsDetailsLabels { get; set; }
        public virtual DbSet<DocGoodsMarkShipment> DocGoodsMarkShipments { get; set; }
        public virtual DbSet<DocPurchasing> DocPurchasings { get; set; }
        public virtual DbSet<DocEdoPurchasing> DocEdoPurchasings { get; set; }
    }
}
