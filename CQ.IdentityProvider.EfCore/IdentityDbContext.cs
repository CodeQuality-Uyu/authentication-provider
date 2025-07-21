﻿using CQ.AuthProvider.BusinessLogic.Identities;
using CQ.AuthProvider.BusinessLogic.Utils;
using CQ.UnitOfWork.EfCore.Core;
using Microsoft.EntityFrameworkCore;

namespace CQ.IdentityProvider.EfCore;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : EfCoreContext(options),
    IIdentityProviderHealthService
{
    public DbSet<Identity> Identities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Identity>(entity =>
        {
            entity
            .HasData(
                new Identity
                {
                    Id = AuthConstants.SEED_ACCOUNT_ID,
                    Email = "seed@cq.com",
                    Password = "AQAAAAEAACcQAAAAEPsvS9UPGBepUkrx3vhkeyoOBVrQFUURtbldx6xuqpW79GVKXbChBf37/GRGw3N+0w=="
                });
        });
    }

    public string GetProvider()
    {
        return "EfCore";
    }

    public bool Ping()
    {
        return base.Ping();
    }
}
