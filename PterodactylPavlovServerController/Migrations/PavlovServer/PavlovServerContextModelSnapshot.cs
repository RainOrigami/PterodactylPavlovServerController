﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PterodactylPavlovServerController.Contexts;

#nullable disable

namespace PterodactylPavlovServerController.Migrations.PavlovServer
{
    [DbContext(typeof(PavlovServerContext))]
    partial class PavlovServerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("PterodactylPavlovServerDomain.Models.AuditActionModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Server")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("Time")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("User")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("AuditActions");
                });

            modelBuilder.Entity("PterodactylPavlovServerDomain.Models.MapInMapRotationModel", b =>
                {
                    b.Property<string>("MapLabel")
                        .HasColumnType("varchar(64)");

                    b.Property<string>("GameMode")
                        .HasColumnType("varchar(64)");

                    b.Property<string>("ServerId")
                        .HasColumnType("varchar(64)");

                    b.Property<string>("RotationName")
                        .HasColumnType("varchar(64)");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.HasKey("MapLabel", "GameMode", "ServerId", "RotationName", "Order");

                    b.HasIndex("ServerId", "RotationName");

                    b.ToTable("MapsInRotation", (string)null);
                });

            modelBuilder.Entity("PterodactylPavlovServerDomain.Models.MapRotationModel", b =>
                {
                    b.Property<string>("ServerId")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("ServerId", "Name");

                    b.ToTable("MapRotations");
                });

            modelBuilder.Entity("PterodactylPavlovServerDomain.Models.PersistentPavlovPlayerModel", b =>
                {
                    b.Property<ulong>("UniqueId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("ServerId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("BanReason")
                        .HasColumnType("longtext");

                    b.Property<string>("Comments")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("EFPCash")
                        .HasColumnType("int");

                    b.Property<DateTime>("LastSeen")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("TotalMoneyEarned")
                        .HasColumnType("bigint unsigned");

                    b.Property<TimeSpan>("TotalTime")
                        .HasColumnType("time(6)");

                    b.Property<DateTime?>("UnbanAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("UniqueId", "ServerId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("PterodactylPavlovServerDomain.Models.ServerMapModel", b =>
                {
                    b.Property<string>("MapLabel")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("GameMode")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("MapLabel", "GameMode");

                    b.ToTable("Maps");
                });

            modelBuilder.Entity("PterodactylPavlovServerDomain.Models.ServerSettings", b =>
                {
                    b.Property<string>("ServerId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("SettingName")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("SettingValue")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("ServerId", "SettingName");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("PterodactylPavlovServerDomain.Models.WarmupRoundLoadoutModel", b =>
                {
                    b.Property<string>("ServerId")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("Attachment")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Gun")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Item")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("ServerId", "Name");

                    b.ToTable("WarmupLoadouts");
                });

            modelBuilder.Entity("PterodactylPavlovServerDomain.Models.MapInMapRotationModel", b =>
                {
                    b.HasOne("PterodactylPavlovServerDomain.Models.ServerMapModel", "Map")
                        .WithMany("MapsInRotation")
                        .HasForeignKey("MapLabel", "GameMode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PterodactylPavlovServerDomain.Models.MapRotationModel", "Rotation")
                        .WithMany("MapsInRotation")
                        .HasForeignKey("ServerId", "RotationName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Map");

                    b.Navigation("Rotation");
                });

            modelBuilder.Entity("PterodactylPavlovServerDomain.Models.MapRotationModel", b =>
                {
                    b.Navigation("MapsInRotation");
                });

            modelBuilder.Entity("PterodactylPavlovServerDomain.Models.ServerMapModel", b =>
                {
                    b.Navigation("MapsInRotation");
                });
#pragma warning restore 612, 618
        }
    }
}
