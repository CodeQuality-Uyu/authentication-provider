﻿// <auto-generated />
using System;
using CQ.AuthProvider.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CQ.AuthProvider.DataAccess.Migrations
{
    [DbContext(typeof(AuthEfCoreContext))]
    [Migration("20240327192256_DeleteAuthClientSystem")]
    partial class DeleteAuthClientSystem
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CQ.AuthProvider.BusinessLogic.Accounts.AccountEfCore", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

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

                    b.HasKey("Id");

                    b.ToTable("Accounts");

                    b.HasData(
                        new
                        {
                            Id = "d47025648273495ba69482fcc69da874",
                            CreatedAt = new DateTimeOffset(new DateTime(2024, 3, 27, 19, 22, 56, 542, DateTimeKind.Unspecified).AddTicks(5654), new TimeSpan(0, 0, 0, 0, 0)),
                            Email = "admin@gmail.com",
                            FirstName = "Admin",
                            FullName = "Admin Admin",
                            LastName = "Admin"
                        });
                });

            modelBuilder.Entity("CQ.AuthProvider.BusinessLogic.Authorizations.PermissionEfCore", b =>
                {
                    b.Property<string>("Id")
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

                    b.HasKey("Id");

                    b.ToTable("Permissions");

                    b.HasData(
                        new
                        {
                            Id = "886416f6b1e44dedac826662202010ea",
                            Description = "Crear permiso",
                            IsPublic = false,
                            Key = "create-permission",
                            Name = "Crear permiso"
                        },
                        new
                        {
                            Id = "654834a7cc234e098302d1bad925c3f9",
                            Description = "Crear muchos permisos",
                            IsPublic = false,
                            Key = "createbulk-permission",
                            Name = "Crear muchos permisos"
                        },
                        new
                        {
                            Id = "c13551d59b354bff9878d99eae94e04f",
                            Description = "Obtener un permiso",
                            IsPublic = false,
                            Key = "getbyid-permission",
                            Name = "Obtener un permiso"
                        },
                        new
                        {
                            Id = "8deb4e1b283f4d939d134c511dc65a6f",
                            Description = "Obtener todos los permisos",
                            IsPublic = false,
                            Key = "getall-permission",
                            Name = "Obtener permisos"
                        },
                        new
                        {
                            Id = "ab435b2ef92d4a96851e2fc0891fa492",
                            Description = "Obtener permisos privados",
                            IsPublic = false,
                            Key = "getallprivate-permission",
                            Name = "Obtener permisos privados"
                        },
                        new
                        {
                            Id = "d7c51933084c4facb88a9f27de33d01a",
                            Description = "Obtener permisos de un rol",
                            IsPublic = false,
                            Key = "getallbyroleid-permission",
                            Name = "Obtener permisos de un rol"
                        },
                        new
                        {
                            Id = "eec8923fe738473793f38f6ba24d42a1",
                            Description = "Actualizar un permiso",
                            IsPublic = false,
                            Key = "updatebyid-permission",
                            Name = "Actualizar un permiso"
                        },
                        new
                        {
                            Id = "db00d1698409477f9e27101136983582",
                            Description = "Crear rol",
                            IsPublic = false,
                            Key = "create-role",
                            Name = "Crear rol"
                        },
                        new
                        {
                            Id = "4560660aa9474b1b9c3047d21cdecab2",
                            Description = "Crear muchos roles",
                            IsPublic = false,
                            Key = "createbulk-role",
                            Name = "Crear muchos roles"
                        },
                        new
                        {
                            Id = "e8668a426cd546f3ac0b4ec7d1a67afd",
                            Description = "Obtener un rol",
                            IsPublic = false,
                            Key = "getbyid-role",
                            Name = "Obtener un rol"
                        },
                        new
                        {
                            Id = "8b9dea2caa3e4481b67cfbecfbbda6f6",
                            Description = "Obtener todos los roles",
                            IsPublic = false,
                            Key = "getall-role",
                            Name = "Obtener roles"
                        },
                        new
                        {
                            Id = "f8a2929a5ac44264b567121fc61fd8ca",
                            Description = "Obtener roles privados",
                            IsPublic = false,
                            Key = "getallprivate-role",
                            Name = "Obtener roles privados"
                        },
                        new
                        {
                            Id = "d887260f12c3419ea5ea656d842d8373",
                            Description = "Actualizar un rol",
                            IsPublic = false,
                            Key = "addpermission-role",
                            Name = "Actualizar un rol"
                        },
                        new
                        {
                            Id = "b9f9bc1b840f4b51b9f1fccb82c52944",
                            Description = "Crear client system",
                            IsPublic = false,
                            Key = "create-clientsystem",
                            Name = "Crear client system"
                        },
                        new
                        {
                            Id = "b63d682602a34e0d8feaffbee83a693d",
                            Description = "Crear cuenta para un usuario",
                            IsPublic = false,
                            Key = "createcredentialsfor-account",
                            Name = "Crear cuenta para un usuario"
                        },
                        new
                        {
                            Id = "21a73466810c4e99840e99e1a58e57df",
                            Description = "Joker",
                            IsPublic = false,
                            Key = "*",
                            Name = "Joker"
                        },
                        new
                        {
                            Id = "ac4e7b6f39a24b98964dcc04b1b2b24e",
                            Description = "Validar token",
                            IsPublic = false,
                            Key = "validatetoken-session",
                            Name = "Validar token"
                        });
                });

            modelBuilder.Entity("CQ.AuthProvider.BusinessLogic.Authorizations.RoleEfCore", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("bit");

                    b.Property<bool>("IsPublic")
                        .HasColumnType("bit");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Roles");

                    b.HasData(
                        new
                        {
                            Id = "2aeae9e48ce740e7bf0ab8647e05761e",
                            Description = "Admin",
                            IsDefault = false,
                            IsPublic = false,
                            Key = "admin",
                            Name = "Admin"
                        },
                        new
                        {
                            Id = "e6136901b87a47018f21407b72c013ca",
                            Description = "Client System",
                            IsDefault = false,
                            IsPublic = false,
                            Key = "clientSystem",
                            Name = "Client System"
                        });
                });

            modelBuilder.Entity("CQ.AuthProvider.BusinessLogic.Authorizations.RolePermission", b =>
                {
                    b.Property<string>("PermissionId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("PermissionId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("RolesPermissions");

                    b.HasData(
                        new
                        {
                            PermissionId = "886416f6b1e44dedac826662202010ea",
                            RoleId = "2aeae9e48ce740e7bf0ab8647e05761e"
                        },
                        new
                        {
                            PermissionId = "c13551d59b354bff9878d99eae94e04f",
                            RoleId = "2aeae9e48ce740e7bf0ab8647e05761e"
                        },
                        new
                        {
                            PermissionId = "8deb4e1b283f4d939d134c511dc65a6f",
                            RoleId = "2aeae9e48ce740e7bf0ab8647e05761e"
                        },
                        new
                        {
                            PermissionId = "ab435b2ef92d4a96851e2fc0891fa492",
                            RoleId = "2aeae9e48ce740e7bf0ab8647e05761e"
                        },
                        new
                        {
                            PermissionId = "d7c51933084c4facb88a9f27de33d01a",
                            RoleId = "2aeae9e48ce740e7bf0ab8647e05761e"
                        },
                        new
                        {
                            PermissionId = "eec8923fe738473793f38f6ba24d42a1",
                            RoleId = "2aeae9e48ce740e7bf0ab8647e05761e"
                        },
                        new
                        {
                            PermissionId = "db00d1698409477f9e27101136983582",
                            RoleId = "2aeae9e48ce740e7bf0ab8647e05761e"
                        },
                        new
                        {
                            PermissionId = "e8668a426cd546f3ac0b4ec7d1a67afd",
                            RoleId = "2aeae9e48ce740e7bf0ab8647e05761e"
                        },
                        new
                        {
                            PermissionId = "8b9dea2caa3e4481b67cfbecfbbda6f6",
                            RoleId = "2aeae9e48ce740e7bf0ab8647e05761e"
                        },
                        new
                        {
                            PermissionId = "f8a2929a5ac44264b567121fc61fd8ca",
                            RoleId = "2aeae9e48ce740e7bf0ab8647e05761e"
                        },
                        new
                        {
                            PermissionId = "d887260f12c3419ea5ea656d842d8373",
                            RoleId = "2aeae9e48ce740e7bf0ab8647e05761e"
                        },
                        new
                        {
                            PermissionId = "b9f9bc1b840f4b51b9f1fccb82c52944",
                            RoleId = "2aeae9e48ce740e7bf0ab8647e05761e"
                        },
                        new
                        {
                            PermissionId = "b63d682602a34e0d8feaffbee83a693d",
                            RoleId = "2aeae9e48ce740e7bf0ab8647e05761e"
                        },
                        new
                        {
                            PermissionId = "654834a7cc234e098302d1bad925c3f9",
                            RoleId = "e6136901b87a47018f21407b72c013ca"
                        },
                        new
                        {
                            PermissionId = "4560660aa9474b1b9c3047d21cdecab2",
                            RoleId = "e6136901b87a47018f21407b72c013ca"
                        },
                        new
                        {
                            PermissionId = "ac4e7b6f39a24b98964dcc04b1b2b24e",
                            RoleId = "e6136901b87a47018f21407b72c013ca"
                        });
                });

            modelBuilder.Entity("CQ.AuthProvider.BusinessLogic.ClientSystems.ClientSystemEfCore", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTimeOffset>("CreatedOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PrivateKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("ClientSystems");
                });

            modelBuilder.Entity("CQ.AuthProvider.BusinessLogic.ResetPasswords.ResetPasswordApplicationEfCore", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AccountId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.ToTable("ResetPasswordApplications");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.Contexts.AccountRole", b =>
                {
                    b.Property<string>("AccountId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("AccountId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AccountsRoles");

                    b.HasData(
                        new
                        {
                            AccountId = "d47025648273495ba69482fcc69da874",
                            RoleId = "2aeae9e48ce740e7bf0ab8647e05761e"
                        });
                });

            modelBuilder.Entity("CQ.AuthProvider.BusinessLogic.Authorizations.RolePermission", b =>
                {
                    b.HasOne("CQ.AuthProvider.BusinessLogic.Authorizations.PermissionEfCore", "Permission")
                        .WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CQ.AuthProvider.BusinessLogic.Authorizations.RoleEfCore", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Permission");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("CQ.AuthProvider.BusinessLogic.ClientSystems.ClientSystemEfCore", b =>
                {
                    b.HasOne("CQ.AuthProvider.BusinessLogic.Authorizations.RoleEfCore", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("CQ.AuthProvider.BusinessLogic.ResetPasswords.ResetPasswordApplicationEfCore", b =>
                {
                    b.HasOne("CQ.AuthProvider.BusinessLogic.Accounts.AccountEfCore", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("CQ.AuthProvider.DataAccess.Contexts.AccountRole", b =>
                {
                    b.HasOne("CQ.AuthProvider.BusinessLogic.Accounts.AccountEfCore", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CQ.AuthProvider.BusinessLogic.Authorizations.RoleEfCore", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Role");
                });
#pragma warning restore 612, 618
        }
    }
}