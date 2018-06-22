using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Threading.Tasks;

namespace Europe.CentralBank.CashServer.Models {
    public class cash {
        [Key]
        public int id { get; set; }

        public int amount { get; set; }

        public DateTimeOffset created_at { get; set; }

        public bool digital { get; set; }

        public virtual List<cash_invalidation> invalidates { get; set; }

        public virtual List<cash_invalidation> invalidated_by { get; set; }
    }

    public class cash_invalidation {
        public int cash_id { get; set; }

        public virtual cash cash { get; set; }

        public int invalidated_cash_id { get; set; }

        public virtual cash invalidated_cash { get; set; }
    }

    public class ApplicationDbContext : DbContext {
        public virtual DbSet<cash> cashs { get; set; }

        public virtual DbSet<cash_invalidation> invalidations { get; set; }
        
        /* Connection String is for migration purposes only */
        public ApplicationDbContext() : base("Data Source=localhost;Initial Catalog=ecbcash_test;User Id=ecbcash_test;Password=ecbcash_test") {

        }
        
        public ApplicationDbContext(string cstr) : base(cstr) {
            Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Properties<string>()
                .Configure(c => c.HasMaxLength(250).HasColumnType("nvarchar"));

            modelBuilder
                .Entity<cash_invalidation>()
                .HasKey(a => new { a.cash_id, a.invalidated_cash_id});

            modelBuilder
                .Entity<cash>()
                .HasMany(a => a.invalidates)
                .WithRequired()
                .HasForeignKey(a => a.cash_id);

            modelBuilder
                .Entity<cash>()
                .HasMany(a => a.invalidated_by)
                .WithRequired()
                .HasForeignKey(a => a.invalidated_cash_id);
        }
    }
}
