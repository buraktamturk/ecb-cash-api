using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {

        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(string)))
            {
                property.AsProperty().Builder
                    .HasMaxLength(256, ConfigurationSource.Convention);
            }

            modelBuilder
                .Entity<cash_invalidation>()
                .HasKey(a => new { a.cash_id, a.invalidated_cash_id});

            modelBuilder
                .Entity<cash>()
                .HasMany(a => a.invalidates)
                .WithOne(a => a.invalidated_cash)
                .HasForeignKey(a => a.cash_id);

            modelBuilder
                .Entity<cash>()
                .HasMany(a => a.invalidated_by)
                .WithOne()
                .HasForeignKey(a => a.invalidated_cash_id);
        }
    }
}
