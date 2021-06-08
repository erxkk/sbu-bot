﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SbuBot.Models;

namespace SbuBot.Migrations
{
    [DbContext(typeof(SbuDbContext))]
    [Migration("20210606154839_StableDependencies")]
    partial class StableDependencies
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.6")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("SbuBot.Models.SbuColorRole", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<ulong>("DiscordId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<ulong?>("OwnerId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("DiscordId")
                        .IsUnique();

                    b.HasIndex("OwnerId")
                        .IsUnique();

                    b.ToTable("ColorRoles");
                });

            modelBuilder.Entity("SbuBot.Models.SbuMember", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<ulong>("DiscordId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("InheritanceCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("DiscordId")
                        .IsUnique();

                    b.ToTable("Members");
                });

            modelBuilder.Entity("SbuBot.Models.SbuReminder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<long>("CreatedAt")
                        .HasColumnType("bigint");

                    b.Property<long>("DueAt")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsDispatched")
                        .HasColumnType("boolean");

                    b.Property<string>("Message")
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)");

                    b.Property<ulong>("MessageId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<ulong?>("OwnerId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Reminders");
                });

            modelBuilder.Entity("SbuBot.Models.SbuTag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("character varying(2048)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<ulong?>("OwnerId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("OwnerId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("SbuBot.Models.SbuColorRole", b =>
                {
                    b.HasOne("SbuBot.Models.SbuMember", "Owner")
                        .WithOne("ColorRole")
                        .HasForeignKey("SbuBot.Models.SbuColorRole", "OwnerId")
                        .HasPrincipalKey("SbuBot.Models.SbuMember", "DiscordId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("SbuBot.Models.SbuReminder", b =>
                {
                    b.HasOne("SbuBot.Models.SbuMember", "Owner")
                        .WithMany("Reminders")
                        .HasForeignKey("OwnerId")
                        .HasPrincipalKey("DiscordId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("SbuBot.Models.SbuTag", b =>
                {
                    b.HasOne("SbuBot.Models.SbuMember", "Owner")
                        .WithMany("Tags")
                        .HasForeignKey("OwnerId")
                        .HasPrincipalKey("DiscordId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("SbuBot.Models.SbuMember", b =>
                {
                    b.Navigation("ColorRole");

                    b.Navigation("Reminders");

                    b.Navigation("Tags");
                });
#pragma warning restore 612, 618
        }
    }
}