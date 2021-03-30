﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RedditPosts.Data;

namespace RedditPosts.Migrations.RedditPostMigrations
{
    [DbContext(typeof(RedditPostContext))]
    [Migration("20210330052054_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.13");

            modelBuilder.Entity("RedditPosts.Models.RedditPost", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Author")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsNsfw")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsSaved")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Number")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Subreddit")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<string>("UrlContent")
                        .HasColumnType("TEXT");

                    b.Property<string>("UrlPost")
                        .HasColumnType("TEXT");

                    b.Property<string>("UrlThumbnail")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("RedditPost");
                });
#pragma warning restore 612, 618
        }
    }
}