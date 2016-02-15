using System;
using System.Data.Entity;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Data.Common;
using MySql.Data;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Security.Cryptography;
using System.IO;
using KrakenService.KrakenObjects;
using KrakenService.KrakenObjects.DataModels;

namespace KrakenService.Data
{
    public class MySqlIdentityDbContext : DbContext
    {

        public MySqlIdentityDbContext()
            : base("MyDbContextConnectionString")
        {
           Database.SetInitializer<MySqlIdentityDbContext>(new MyDbInitializer());
           //Database.SetInitializer<MySqlIdentityDbContext>(null);
        }

        public DbSet<TradingData> TradingDatas { get; set; }
        public DbSet<OrderBookAnalysedData> OrderBookDatas { get; set; }
        public DbSet<MarketData> MarketDatas { get; set; }

        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }

    public class MyDbInitializer : CreateDatabaseIfNotExists<MySqlIdentityDbContext>
    {
        protected override void Seed(MySqlIdentityDbContext context)
        {
            // create entity here if needed during creation      
            base.Seed(context);
        }
    }
}
