﻿// <auto-generated />
using System;
using Europe.CentralBank.CashServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Europe.CentralBank.CashServer.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-preview3-35497")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Europe.CentralBank.CashServer.Models.cash", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("amount");

                    b.Property<DateTimeOffset>("created_at");

                    b.Property<bool>("digital");

                    b.HasKey("id");

                    b.ToTable("cashs");
                });

            modelBuilder.Entity("Europe.CentralBank.CashServer.Models.cash_invalidation", b =>
                {
                    b.Property<int>("cash_id");

                    b.Property<int>("invalidated_cash_id");

                    b.Property<int?>("cashid");

                    b.HasKey("cash_id", "invalidated_cash_id");

                    b.HasIndex("cashid");

                    b.HasIndex("invalidated_cash_id");

                    b.ToTable("invalidations");
                });

            modelBuilder.Entity("Europe.CentralBank.CashServer.Models.cash_invalidation", b =>
                {
                    b.HasOne("Europe.CentralBank.CashServer.Models.cash", "invalidated_cash")
                        .WithMany("invalidates")
                        .HasForeignKey("cash_id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Europe.CentralBank.CashServer.Models.cash", "cash")
                        .WithMany()
                        .HasForeignKey("cashid");

                    b.HasOne("Europe.CentralBank.CashServer.Models.cash")
                        .WithMany("invalidated_by")
                        .HasForeignKey("invalidated_cash_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
