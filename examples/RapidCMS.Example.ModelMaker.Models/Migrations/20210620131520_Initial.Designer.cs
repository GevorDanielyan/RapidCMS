﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RapidCMS.ModelMaker;

namespace RapidCMS.Example.ModelMaker.Migrations
{
    [DbContext(typeof(ModelMakerDbContext))]
    [Migration("20210620131520_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.7")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BlogCategory", b =>
                {
                    b.Property<int>("BlogBlogCategoriesId")
                        .HasColumnType("int");

                    b.Property<int>("BlogCategoriesId")
                        .HasColumnType("int");

                    b.HasKey("BlogBlogCategoriesId", "BlogCategoriesId");

                    b.HasIndex("BlogCategoriesId");

                    b.ToTable("BlogCategory");
                });

            modelBuilder.Entity("ManytoManyManyAManytoManyManyB", b =>
                {
                    b.Property<int>("AsId")
                        .HasColumnType("int");

                    b.Property<int>("BsId")
                        .HasColumnType("int");

                    b.HasKey("AsId", "BsId");

                    b.HasIndex("BsId");

                    b.ToTable("ManytoManyManyAManytoManyManyB");
                });

            modelBuilder.Entity("RapidCMS.ModelMaker.Blog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsPublished")
                        .HasColumnType("bit");

                    b.Property<int?>("MainCategoryId")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<DateTime>("PublishDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(127)
                        .HasColumnType("nvarchar(127)");

                    b.HasKey("Id");

                    b.HasIndex("MainCategoryId");

                    b.ToTable("Blogs");
                });

            modelBuilder.Entity("RapidCMS.ModelMaker.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("RapidCMS.ModelMaker.ManytoManyManyA", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ManytoManyManyAs");
                });

            modelBuilder.Entity("RapidCMS.ModelMaker.ManytoManyManyB", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ManytoManyManyBs");
                });

            modelBuilder.Entity("RapidCMS.ModelMaker.OnetoManyMany", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("OnetoManyManys");
                });

            modelBuilder.Entity("RapidCMS.ModelMaker.OnetoManyOne", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("OneId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OneId");

                    b.ToTable("OnetoManyOnes");
                });

            modelBuilder.Entity("RapidCMS.ModelMaker.OnetoOneOneA", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("BId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BId")
                        .IsUnique()
                        .HasFilter("[BId] IS NOT NULL");

                    b.ToTable("OnetoOneOneAs");
                });

            modelBuilder.Entity("RapidCMS.ModelMaker.OnetoOneOneB", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("AId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("OnetoOneOneBs");
                });

            modelBuilder.Entity("BlogCategory", b =>
                {
                    b.HasOne("RapidCMS.ModelMaker.Blog", null)
                        .WithMany()
                        .HasForeignKey("BlogBlogCategoriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RapidCMS.ModelMaker.Category", null)
                        .WithMany()
                        .HasForeignKey("BlogCategoriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ManytoManyManyAManytoManyManyB", b =>
                {
                    b.HasOne("RapidCMS.ModelMaker.ManytoManyManyA", null)
                        .WithMany()
                        .HasForeignKey("AsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RapidCMS.ModelMaker.ManytoManyManyB", null)
                        .WithMany()
                        .HasForeignKey("BsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RapidCMS.ModelMaker.Blog", b =>
                {
                    b.HasOne("RapidCMS.ModelMaker.Category", "MainCategory")
                        .WithMany("BlogMainCategory")
                        .HasForeignKey("MainCategoryId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("MainCategory");
                });

            modelBuilder.Entity("RapidCMS.ModelMaker.OnetoManyOne", b =>
                {
                    b.HasOne("RapidCMS.ModelMaker.OnetoManyMany", "One")
                        .WithMany("Many")
                        .HasForeignKey("OneId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("One");
                });

            modelBuilder.Entity("RapidCMS.ModelMaker.OnetoOneOneA", b =>
                {
                    b.HasOne("RapidCMS.ModelMaker.OnetoOneOneB", "B")
                        .WithOne("A")
                        .HasForeignKey("RapidCMS.ModelMaker.OnetoOneOneA", "BId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("B");
                });

            modelBuilder.Entity("RapidCMS.ModelMaker.Category", b =>
                {
                    b.Navigation("BlogMainCategory");
                });

            modelBuilder.Entity("RapidCMS.ModelMaker.OnetoManyMany", b =>
                {
                    b.Navigation("Many");
                });

            modelBuilder.Entity("RapidCMS.ModelMaker.OnetoOneOneB", b =>
                {
                    b.Navigation("A");
                });
#pragma warning restore 612, 618
        }
    }
}