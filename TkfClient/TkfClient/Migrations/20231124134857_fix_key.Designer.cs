﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TkfClient;

#nullable disable

namespace TkfClient.Migrations
{
    [DbContext(typeof(AppContext))]
    [Migration("20231124134857_fix_key")]
    partial class fix_key
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TkfClient.Models.CandleSync", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text");

                    b.Property<decimal>("Close")
                        .HasColumnType("numeric");

                    b.Property<decimal>("High")
                        .HasColumnType("numeric");

                    b.Property<string>("Isin")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Low")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Open")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("Time")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Uid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("Volume")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("Candle");
                });
#pragma warning restore 612, 618
        }
    }
}
