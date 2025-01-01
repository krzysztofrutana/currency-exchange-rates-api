﻿// <auto-generated />
using System;
using CurrencyExchangeRates.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CurrencyExchangeRates.Data.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CurrencyExchangeRates.Data.Entities.Currency", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AdditionalDataJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Code")
                        .HasMaxLength(3)
                        .HasColumnType("nvarchar(3)");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique()
                        .HasFilter("[Code] IS NOT NULL");

                    b.ToTable("Currencies");
                });

            modelBuilder.Entity("CurrencyExchangeRates.Data.Entities.CurrencyExchangeRate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("AvarageRate")
                        .HasPrecision(20, 10)
                        .HasColumnType("decimal(20,10)");

                    b.Property<int>("CurrencyId")
                        .HasColumnType("int");

                    b.Property<DateOnly>("EffectiveDate")
                        .HasColumnType("date");

                    b.Property<DateOnly>("ForDate")
                        .HasColumnType("date");

                    b.Property<decimal?>("PurchaseRate")
                        .HasPrecision(20, 10)
                        .HasColumnType("decimal(20,10)");

                    b.Property<decimal?>("SaleRate")
                        .HasPrecision(20, 10)
                        .HasColumnType("decimal(20,10)");

                    b.Property<string>("Source")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("CurrencyId");

                    b.HasIndex("EffectiveDate");

                    b.HasIndex("ForDate");

                    b.ToTable("CurrencyExchangeRates");
                });

            modelBuilder.Entity("CurrencyExchangeRates.Data.Entities.CurrencyExchangeRate", b =>
                {
                    b.HasOne("CurrencyExchangeRates.Data.Entities.Currency", "Currency")
                        .WithMany("CurrencyExchangeRates")
                        .HasForeignKey("CurrencyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Currency");
                });

            modelBuilder.Entity("CurrencyExchangeRates.Data.Entities.Currency", b =>
                {
                    b.Navigation("CurrencyExchangeRates");
                });
#pragma warning restore 612, 618
        }
    }
}
