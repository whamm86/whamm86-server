﻿// <auto-generated />
using AasxServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AasxServerStandardBib.Migrations
{
    [DbContext(typeof(AasContext))]
    partial class AasContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("AasxServer.AASXSet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AASX")
                        .HasColumnType("TEXT");

                    b.Property<long>("AASXNum")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AASXNum");

                    b.ToTable("AASXSets");
                });

            modelBuilder.Entity("AasxServer.AasSet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("AASXNum")
                        .HasColumnType("INTEGER");

                    b.Property<string>("AasId")
                        .HasColumnType("TEXT");

                    b.Property<long>("AasNum")
                        .HasColumnType("INTEGER");

                    b.Property<string>("AssetId")
                        .HasColumnType("TEXT");

                    b.Property<string>("AssetKind")
                        .HasColumnType("TEXT");

                    b.Property<string>("Idshort")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AasNum");

                    b.ToTable("AasSets");
                });

            modelBuilder.Entity("AasxServer.DbConfigSet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("AASXCount")
                        .HasColumnType("INTEGER");

                    b.Property<long>("AasCount")
                        .HasColumnType("INTEGER");

                    b.Property<long>("SMECount")
                        .HasColumnType("INTEGER");

                    b.Property<long>("SubmodelCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("DbConfigSets");
                });

            modelBuilder.Entity("AasxServer.SMESet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("FValue")
                        .HasColumnType("REAL");

                    b.Property<long>("IValue")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Idshort")
                        .HasColumnType("TEXT");

                    b.Property<long>("ParentSMENum")
                        .HasColumnType("INTEGER");

                    b.Property<long>("SMENum")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SMEType")
                        .HasColumnType("TEXT");

                    b.Property<string>("SValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("SemanticId")
                        .HasColumnType("TEXT");

                    b.Property<long>("SubmodelNum")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ValueType")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("SMENum");

                    b.HasIndex("SubmodelNum");

                    b.ToTable("SMESets");
                });

            modelBuilder.Entity("AasxServer.SubmodelSet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("AASXNum")
                        .HasColumnType("INTEGER");

                    b.Property<long>("AasNum")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Idshort")
                        .HasColumnType("TEXT");

                    b.Property<string>("SemanticId")
                        .HasColumnType("TEXT");

                    b.Property<string>("SubmodelId")
                        .HasColumnType("TEXT");

                    b.Property<long>("SubmodelNum")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("SubmodelNum");

                    b.ToTable("SubmodelSets");
                });
#pragma warning restore 612, 618
        }
    }
}
