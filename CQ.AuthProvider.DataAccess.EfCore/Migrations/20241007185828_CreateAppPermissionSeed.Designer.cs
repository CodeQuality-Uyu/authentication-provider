﻿// <auto-generated />
using System;
using CQ.AuthProvider.DataAccess.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CQ.AuthProvider.DataAccess.EfCore.Migrations
{
    [DbContext(typeof(AuthDbContext))]
    [Migration("20241007185828_CreateAppPermissionSeed")]
    partial class CreateAppPermissionSeed
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Accounts.AccountApp", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AccountId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AppId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("AppId");

                    b.ToTable("AccountsApps");

                    b.HasData(
                        new
                        {
                            Id = "ef03980ea2a54e4bba92e022fbd33d9b",
                            AccountId = "5a0d9e179991499e80db0a15fda4df79",
                            AppId = "d31184dabbc6435eaec86694650c2679"
                        });
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Accounts.AccountEfCore", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Locale")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProfilePictureId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenantId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("TimeZone")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.ToTable("Accounts");

                    b.HasData(
                        new
                        {
                            Id = "5a0d9e179991499e80db0a15fda4df79",
                            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "seed@cq.com",
                            FirstName = "Seed",
                            FullName = "Seed Seed",
                            LastName = "Seed",
                            Locale = "Uruguay",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4",
                            TimeZone = "-3"
                        });
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Accounts.AccountRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AccountId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("RoleId");

                    b.ToTable("AccountsRoles");

                    b.HasData(
                        new
                        {
                            Id = "1f191c90510d456d84bda9e17fe24f50",
                            AccountId = "5a0d9e179991499e80db0a15fda4df79",
                            RoleId = "0415b39e83cd4fbdb33c5004a0b65294"
                        });
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Apps.AppEfCore", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.ToTable("Apps");

                    b.HasData(
                        new
                        {
                            Id = "d31184dabbc6435eaec86694650c2679",
                            IsDefault = true,
                            Name = "Auth Provider WEB API",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        });
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Invitations.InvitationEfCore", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AppId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatorId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("AppId");

                    b.HasIndex("CreatorId");

                    b.HasIndex("RoleId");

                    b.HasIndex("TenantId");

                    b.ToTable("Invitations");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Permissions.PermissionEfCore", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AppId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsPublic")
                        .HasColumnType("bit");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("AppId");

                    b.HasIndex("TenantId");

                    b.ToTable("Permissions");

                    b.HasData(
                        new
                        {
                            Id = "39d20cb8c4d541c6944aeeb678261cea",
                            AppId = "d31184dabbc6435eaec86694650c2679",
                            Description = "Can create permissions",
                            IsPublic = true,
                            Key = "create-permission",
                            Name = "Create permission",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        },
                        new
                        {
                            Id = "d40ad347c7f943e399069eef409b4fa6",
                            AppId = "d31184dabbc6435eaec86694650c2679",
                            Description = "Can read permissions",
                            IsPublic = true,
                            Key = "getall-permission",
                            Name = "Can read permissions",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        },
                        new
                        {
                            Id = "8b1c2d303f3b45a1aa3ae6af46c8652b",
                            AppId = "d31184dabbc6435eaec86694650c2679",
                            Description = "Can create roles",
                            IsPublic = true,
                            Key = "create-role",
                            Name = "Can create role",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        },
                        new
                        {
                            Id = "aca002cfbf3a47899ff4c16e6be2c029",
                            AppId = "d31184dabbc6435eaec86694650c2679",
                            Description = "Can read roles",
                            IsPublic = true,
                            Key = "getall-role",
                            Name = "Can read roles",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        },
                        new
                        {
                            Id = "80ca0e41ea5046519f351a99b03b294e",
                            AppId = "d31184dabbc6435eaec86694650c2679",
                            Description = "Can read invitations of tenant",
                            IsPublic = true,
                            Key = "getall-invitation",
                            Name = "Can read invitations of tenant",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        },
                        new
                        {
                            Id = "f3ba5c2342a248d89eee2977456d54cd",
                            AppId = "d31184dabbc6435eaec86694650c2679",
                            Description = "Can create invitations",
                            IsPublic = true,
                            Key = "create-invitation",
                            Name = "Can create invitations",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        },
                        new
                        {
                            Id = "9203d8a99d4e4a3b9b47f7db0e81353e",
                            AppId = "d31184dabbc6435eaec86694650c2679",
                            Description = "Can create tenant",
                            IsPublic = true,
                            Key = "create-tenant",
                            Name = "Can create tenant",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        },
                        new
                        {
                            Id = "7d21bd25e0b74951b06772ca348e81fa",
                            AppId = "d31184dabbc6435eaec86694650c2679",
                            Description = "Can update tenant name",
                            IsPublic = true,
                            Key = "updatetenantname-me",
                            Name = "Can update tenant name",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        },
                        new
                        {
                            Id = "91cc2fb3a90e4f4aa01c02a363ae44c3",
                            AppId = "d31184dabbc6435eaec86694650c2679",
                            Description = "Can update tenant owner",
                            IsPublic = true,
                            Key = "transfertenant-me",
                            Name = "Can update tenant owner",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        },
                        new
                        {
                            Id = "7e9af6ea241342c5bb97c634a36c2de2",
                            AppId = "d31184dabbc6435eaec86694650c2679",
                            Description = "Can create app",
                            IsPublic = true,
                            Key = "create-app",
                            Name = "Can create app",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        },
                        new
                        {
                            Id = "9b079f7461554950bbd981f929568322",
                            AppId = "d31184dabbc6435eaec86694650c2679",
                            Description = "Can read all tenants",
                            IsPublic = true,
                            Key = "getall-tenants",
                            Name = "Can read all tenants",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        },
                        new
                        {
                            Id = "33d7733f42214f6785e10a480c45a007",
                            AppId = "d31184dabbc6435eaec86694650c2679",
                            Description = "Can read all accounts",
                            IsPublic = true,
                            Key = "getall-accounts",
                            Name = "Can read all accounts",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        });
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.ResetPasswords.ResetPasswordEfCore", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AccountId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.ToTable("ResetPasswords");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Roles.RoleEfCore", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AppId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("bit");

                    b.Property<bool>("IsPublic")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("AppId");

                    b.HasIndex("TenantId");

                    b.ToTable("Roles");

                    b.HasData(
                        new
                        {
                            Id = "5c2260fc58864b75a4cad5c0e7dd57cb",
                            AppId = "d31184dabbc6435eaec86694650c2679",
                            Description = "Tenant Owner",
                            IsDefault = false,
                            IsPublic = true,
                            Name = "Tenant Owner",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        },
                        new
                        {
                            Id = "dfa136595e304b98ad7b55d782c6a12c",
                            AppId = "d31184dabbc6435eaec86694650c2679",
                            Description = "Owner of Auth Web Api App",
                            IsDefault = false,
                            IsPublic = true,
                            Name = "Auth Web Api Owner",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        },
                        new
                        {
                            Id = "0415b39e83cd4fbdb33c5004a0b65294",
                            AppId = "d31184dabbc6435eaec86694650c2679",
                            Description = "Should be deleted once deployed",
                            IsDefault = false,
                            IsPublic = false,
                            Name = "Seed",
                            TenantId = "b22fcf202bd84a97936ccf2949e00da4"
                        });
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Roles.RolePermission", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("PermissionId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("PermissionId");

                    b.HasIndex("RoleId");

                    b.ToTable("RolesPermissions");

                    b.HasData(
                        new
                        {
                            Id = "ea84e710483e42eea573260151916d36",
                            PermissionId = "f3ba5c2342a248d89eee2977456d54cd",
                            RoleId = "0415b39e83cd4fbdb33c5004a0b65294"
                        },
                        new
                        {
                            Id = "2ea1bb330e3e489cbf3402daacef9905",
                            PermissionId = "aca002cfbf3a47899ff4c16e6be2c029",
                            RoleId = "0415b39e83cd4fbdb33c5004a0b65294"
                        },
                        new
                        {
                            Id = "c07081abf2054ec496bee67b44a2ee2a",
                            PermissionId = "39d20cb8c4d541c6944aeeb678261cea",
                            RoleId = "5c2260fc58864b75a4cad5c0e7dd57cb"
                        },
                        new
                        {
                            Id = "d76afb762df349caadc39b7373ea98ed",
                            PermissionId = "d40ad347c7f943e399069eef409b4fa6",
                            RoleId = "5c2260fc58864b75a4cad5c0e7dd57cb"
                        },
                        new
                        {
                            Id = "be097c9f1b4e4b3088172bcb0c75372b",
                            PermissionId = "8b1c2d303f3b45a1aa3ae6af46c8652b",
                            RoleId = "5c2260fc58864b75a4cad5c0e7dd57cb"
                        },
                        new
                        {
                            Id = "6e3f476ec4354b27af25e025034ee97e",
                            PermissionId = "aca002cfbf3a47899ff4c16e6be2c029",
                            RoleId = "5c2260fc58864b75a4cad5c0e7dd57cb"
                        },
                        new
                        {
                            Id = "c867b020844a4a6fa495b013bc903b3a",
                            PermissionId = "f3ba5c2342a248d89eee2977456d54cd",
                            RoleId = "5c2260fc58864b75a4cad5c0e7dd57cb"
                        },
                        new
                        {
                            Id = "d26570a4df1a41fea0bf0006f1b87721",
                            PermissionId = "80ca0e41ea5046519f351a99b03b294e",
                            RoleId = "5c2260fc58864b75a4cad5c0e7dd57cb"
                        },
                        new
                        {
                            Id = "8c52753c02324daeb56fb4557c2eaf46",
                            PermissionId = "9203d8a99d4e4a3b9b47f7db0e81353e",
                            RoleId = "5c2260fc58864b75a4cad5c0e7dd57cb"
                        },
                        new
                        {
                            Id = "60307119a6f8403faaf53606eceefedc",
                            PermissionId = "7d21bd25e0b74951b06772ca348e81fa",
                            RoleId = "5c2260fc58864b75a4cad5c0e7dd57cb"
                        },
                        new
                        {
                            Id = "20ba3bbf9e87433199a49bc01c928014",
                            PermissionId = "91cc2fb3a90e4f4aa01c02a363ae44c3",
                            RoleId = "5c2260fc58864b75a4cad5c0e7dd57cb"
                        },
                        new
                        {
                            Id = "f368580391cc459c964ce099cebb9b02",
                            PermissionId = "7e9af6ea241342c5bb97c634a36c2de2",
                            RoleId = "5c2260fc58864b75a4cad5c0e7dd57cb"
                        },
                        new
                        {
                            Id = "89c5ad347a8f41c0864a4a37f7be5224",
                            PermissionId = "9b079f7461554950bbd981f929568322",
                            RoleId = "dfa136595e304b98ad7b55d782c6a12c"
                        },
                        new
                        {
                            Id = "16ef3304b62240b2bd86b4287f14bea3",
                            PermissionId = "33d7733f42214f6785e10a480c45a007",
                            RoleId = "dfa136595e304b98ad7b55d782c6a12c"
                        });
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Sessions.SessionEfCore", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AccountId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AppId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("AppId");

                    b.ToTable("Sessions");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Tenants.TenantEfCore", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OwnerId")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId")
                        .IsUnique();

                    b.ToTable("Tenants");

                    b.HasData(
                        new
                        {
                            Id = "b22fcf202bd84a97936ccf2949e00da4",
                            Name = "Seed Tenant",
                            OwnerId = "5a0d9e179991499e80db0a15fda4df79"
                        });
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Accounts.AccountApp", b =>
                {
                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Accounts.AccountEfCore", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Apps.AppEfCore", "App")
                        .WithMany()
                        .HasForeignKey("AppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("App");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Accounts.AccountEfCore", b =>
                {
                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Tenants.TenantEfCore", "Tenant")
                        .WithMany("Accounts")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Accounts.AccountRole", b =>
                {
                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Accounts.AccountEfCore", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Roles.RoleEfCore", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Apps.AppEfCore", b =>
                {
                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Tenants.TenantEfCore", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Invitations.InvitationEfCore", b =>
                {
                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Apps.AppEfCore", "App")
                        .WithMany()
                        .HasForeignKey("AppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Accounts.AccountEfCore", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Roles.RoleEfCore", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Tenants.TenantEfCore", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("App");

                    b.Navigation("Creator");

                    b.Navigation("Role");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Permissions.PermissionEfCore", b =>
                {
                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Apps.AppEfCore", "App")
                        .WithMany("Permissions")
                        .HasForeignKey("AppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Tenants.TenantEfCore", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("App");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.ResetPasswords.ResetPasswordEfCore", b =>
                {
                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Accounts.AccountEfCore", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Roles.RoleEfCore", b =>
                {
                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Apps.AppEfCore", "App")
                        .WithMany("Roles")
                        .HasForeignKey("AppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Tenants.TenantEfCore", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("App");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Roles.RolePermission", b =>
                {
                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Permissions.PermissionEfCore", "Permission")
                        .WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Roles.RoleEfCore", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Permission");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Sessions.SessionEfCore", b =>
                {
                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Accounts.AccountEfCore", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Apps.AppEfCore", "App")
                        .WithMany()
                        .HasForeignKey("AppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("App");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Tenants.TenantEfCore", b =>
                {
                    b.HasOne("CQ.AuthProvider.DataAccess.EfCore.Accounts.AccountEfCore", "Owner")
                        .WithOne()
                        .HasForeignKey("CQ.AuthProvider.DataAccess.EfCore.Tenants.TenantEfCore", "OwnerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Apps.AppEfCore", b =>
                {
                    b.Navigation("Permissions");

                    b.Navigation("Roles");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.EfCore.Tenants.TenantEfCore", b =>
                {
                    b.Navigation("Accounts");
                });
#pragma warning restore 612, 618
        }
    }
}