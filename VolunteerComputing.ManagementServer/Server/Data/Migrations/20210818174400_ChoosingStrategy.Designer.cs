﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VolunteerComputing.ManagementServer.Server.Data;

namespace VolunteerComputing.ManagementServer.Server.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20210818174400_ChoosingStrategy")]
    partial class ChoosingStrategy
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.8")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("IdentityServer4.EntityFramework.Entities.DeviceFlowCodes", b =>
                {
                    b.Property<string>("UserCode")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasMaxLength(50000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("DeviceCode")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime?>("Expiration")
                        .IsRequired()
                        .HasColumnType("datetime2");

                    b.Property<string>("SessionId")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("SubjectId")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("UserCode");

                    b.HasIndex("DeviceCode")
                        .IsUnique();

                    b.HasIndex("Expiration");

                    b.ToTable("DeviceCodes");
                });

            modelBuilder.Entity("IdentityServer4.EntityFramework.Entities.PersistedGrant", b =>
                {
                    b.Property<string>("Key")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime?>("ConsumedTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasMaxLength(50000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime?>("Expiration")
                        .HasColumnType("datetime2");

                    b.Property<string>("SessionId")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("SubjectId")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Key");

                    b.HasIndex("Expiration");

                    b.HasIndex("SubjectId", "ClientId", "Type");

                    b.HasIndex("SubjectId", "SessionId", "Type");

                    b.ToTable("PersistedGrants");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("VolunteerComputing.ManagementServer.Server.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.BundleResult", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("BundleId")
                        .HasColumnType("int");

                    b.Property<byte[]>("DataHash")
                        .HasColumnType("varbinary(max)");

                    b.HasKey("Id");

                    b.HasIndex("BundleId");

                    b.ToTable("BundleResults");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.ComputeTask", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ExeFilename")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LinuxCpuProgram")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LinuxGpuProgram")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ProjectId")
                        .HasColumnType("int");

                    b.Property<string>("WindowsCpuProgram")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WindowsGpuProgram")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("ComputeTask");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.DeviceData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<double>("BaseCpuEnergyConsumption")
                        .HasColumnType("float");

                    b.Property<double>("BaseGpuEnergyConsumption")
                        .HasColumnType("float");

                    b.Property<string>("ConnectionId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("CpuAvailable")
                        .HasColumnType("bit");

                    b.Property<int>("CpuWorksOnBundle")
                        .HasColumnType("int");

                    b.Property<bool>("GpuAvailable")
                        .HasColumnType("bit");

                    b.Property<int>("GpuWorksOnBundle")
                        .HasColumnType("int");

                    b.Property<bool>("IsWindows")
                        .HasColumnType("bit");

                    b.Property<string>("TaskServerId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.DeviceStat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("ComputeTaskId")
                        .HasColumnType("int");

                    b.Property<int>("Count")
                        .HasColumnType("int");

                    b.Property<int?>("DeviceDataId")
                        .HasColumnType("int");

                    b.Property<double>("EnergySum")
                        .HasColumnType("float");

                    b.Property<bool>("IsCpu")
                        .HasColumnType("bit");

                    b.Property<double>("TimeSum")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("ComputeTaskId");

                    b.HasIndex("DeviceDataId");

                    b.ToTable("DeviceStats");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.Packet", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Aggregated")
                        .HasColumnType("bit");

                    b.Property<int?>("BundleId")
                        .HasColumnType("int");

                    b.Property<int?>("BundleResultId")
                        .HasColumnType("int");

                    b.Property<string>("Data")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DeviceWorkedOnItId")
                        .HasColumnType("int");

                    b.Property<int?>("TypeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BundleId");

                    b.HasIndex("BundleResultId");

                    b.HasIndex("DeviceWorkedOnItId");

                    b.HasIndex("TypeId");

                    b.ToTable("Packets");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.PacketBundle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("ComputeTaskId")
                        .HasColumnType("int");

                    b.Property<int>("TimesSent")
                        .HasColumnType("int");

                    b.Property<int>("UntilCheck")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ComputeTaskId");

                    b.ToTable("Bundles");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.PacketType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Aggregable")
                        .HasColumnType("bit");

                    b.Property<int?>("ProjectId")
                        .HasColumnType("int");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("PacketTypes");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.PacketTypeToComputeTask", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("ComputeTaskId")
                        .HasColumnType("int");

                    b.Property<bool>("IsInput")
                        .HasColumnType("bit");

                    b.Property<int?>("PacketTypeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ComputeTaskId");

                    b.HasIndex("PacketTypeId");

                    b.ToTable("PacketTypeToComputeTasks");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<double>("ChanceToUseNewDevice")
                        .HasColumnType("float");

                    b.Property<int>("ChoosingStrategy")
                        .HasColumnType("int");

                    b.Property<int>("MinAgreeingClients")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.Result", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("FileId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("Result");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("VolunteerComputing.ManagementServer.Server.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("VolunteerComputing.ManagementServer.Server.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VolunteerComputing.ManagementServer.Server.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("VolunteerComputing.ManagementServer.Server.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.BundleResult", b =>
                {
                    b.HasOne("VolunteerComputing.Shared.Models.PacketBundle", "Bundle")
                        .WithMany("BundleResults")
                        .HasForeignKey("BundleId");

                    b.Navigation("Bundle");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.ComputeTask", b =>
                {
                    b.HasOne("VolunteerComputing.Shared.Models.Project", "Project")
                        .WithMany("ComputeTasks")
                        .HasForeignKey("ProjectId");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.DeviceStat", b =>
                {
                    b.HasOne("VolunteerComputing.Shared.Models.ComputeTask", "ComputeTask")
                        .WithMany("DeviceStats")
                        .HasForeignKey("ComputeTaskId");

                    b.HasOne("VolunteerComputing.Shared.Models.DeviceData", "DeviceData")
                        .WithMany("DeviceStats")
                        .HasForeignKey("DeviceDataId");

                    b.Navigation("ComputeTask");

                    b.Navigation("DeviceData");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.Packet", b =>
                {
                    b.HasOne("VolunteerComputing.Shared.Models.PacketBundle", "Bundle")
                        .WithMany("Packets")
                        .HasForeignKey("BundleId");

                    b.HasOne("VolunteerComputing.Shared.Models.BundleResult", "BundleResult")
                        .WithMany("Packets")
                        .HasForeignKey("BundleResultId");

                    b.HasOne("VolunteerComputing.Shared.Models.DeviceData", "DeviceWorkedOnIt")
                        .WithMany()
                        .HasForeignKey("DeviceWorkedOnItId");

                    b.HasOne("VolunteerComputing.Shared.Models.PacketType", "Type")
                        .WithMany("Packets")
                        .HasForeignKey("TypeId");

                    b.Navigation("Bundle");

                    b.Navigation("BundleResult");

                    b.Navigation("DeviceWorkedOnIt");

                    b.Navigation("Type");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.PacketBundle", b =>
                {
                    b.HasOne("VolunteerComputing.Shared.Models.ComputeTask", "ComputeTask")
                        .WithMany()
                        .HasForeignKey("ComputeTaskId");

                    b.Navigation("ComputeTask");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.PacketType", b =>
                {
                    b.HasOne("VolunteerComputing.Shared.Models.Project", "Project")
                        .WithMany("PacketTypes")
                        .HasForeignKey("ProjectId");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.PacketTypeToComputeTask", b =>
                {
                    b.HasOne("VolunteerComputing.Shared.Models.ComputeTask", "ComputeTask")
                        .WithMany("PacketTypes")
                        .HasForeignKey("ComputeTaskId");

                    b.HasOne("VolunteerComputing.Shared.Models.PacketType", "PacketType")
                        .WithMany("ComputeTasks")
                        .HasForeignKey("PacketTypeId");

                    b.Navigation("ComputeTask");

                    b.Navigation("PacketType");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.Result", b =>
                {
                    b.HasOne("VolunteerComputing.Shared.Models.Project", "Project")
                        .WithMany("Results")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.BundleResult", b =>
                {
                    b.Navigation("Packets");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.ComputeTask", b =>
                {
                    b.Navigation("DeviceStats");

                    b.Navigation("PacketTypes");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.DeviceData", b =>
                {
                    b.Navigation("DeviceStats");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.PacketBundle", b =>
                {
                    b.Navigation("BundleResults");

                    b.Navigation("Packets");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.PacketType", b =>
                {
                    b.Navigation("ComputeTasks");

                    b.Navigation("Packets");
                });

            modelBuilder.Entity("VolunteerComputing.Shared.Models.Project", b =>
                {
                    b.Navigation("ComputeTasks");

                    b.Navigation("PacketTypes");

                    b.Navigation("Results");
                });
#pragma warning restore 612, 618
        }
    }
}
