﻿using CQ.AuthProvider.BusinessLogic.Abstractions.Permissions;
using CQ.AuthProvider.BusinessLogic.Abstractions.Roles;

namespace CQ.AuthProvider.DataAccess.Mongo.Roles;

public sealed record class RoleMongo
{
    public string Id { get; init; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public List<string> Permissions { get; set; } = [];

    public bool IsPublic { get; set; }

    public bool IsDefault { get; set; }

    /// <summary>
    /// For MongoDriver
    /// </summary>
    public RoleMongo()
    {
    }

    /// <summary>
    /// For new Role
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="key"></param>
    /// <param name="permissions"></param>
    /// <param name="isPublic"></param>
    /// <param name="isDefault"></param>
    public RoleMongo(
        string id,
        string name,
        string description,
        List<Permission> permissions,
        bool isPublic,
        bool isDefault)
    {
        Id = id;
        Name = name;
        Description = description;
        Permissions = permissions.ConvertAll(p => p.Key);
        IsPublic = isPublic;
        IsDefault = isDefault;
    }
}
