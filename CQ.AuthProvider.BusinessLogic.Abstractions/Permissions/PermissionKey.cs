﻿using CQ.Utility;

namespace CQ.AuthProvider.BusinessLogic.Abstractions.Permissions;

public sealed record class PermissionKey
{
    #region Permission
    public static readonly PermissionKey CreatePermission = new("create-permission");
    public static readonly PermissionKey CreateBulkPermission = new("createbulk-permission");
    public static readonly PermissionKey GetByIdPermission = new("getbyid-permission");
    public static readonly PermissionKey GetAllPermissions = new("getall-permission");
    public static readonly PermissionKey GetAllPrivatePermissions = new("getallprivate-permission");
    public static readonly PermissionKey GetAllPermissionsByRoleId = new("getallbyroleid-permission");
    public static readonly PermissionKey UpdateByIdPermission = new("updatebyid-permission");
    #endregion

    #region Role
    public static readonly PermissionKey CreateRole = new("create-role");
    public static readonly PermissionKey CreateBulkRole = new("createbulk-role");
    public static readonly PermissionKey GetByIdRole = new("getbyid-role");
    public static readonly PermissionKey GetAllRoles = new("getall-role");
    public static readonly PermissionKey GetAllPrivateRoles = new("getallprivate-role");
    public static readonly PermissionKey AddPermissionToRole = new("addpermission-role");
    #endregion

    public static readonly PermissionKey CreateAccountFor = new("createcredentialsfor-account");

    public static readonly PermissionKey ValidateToken = new("validatetoken-session");

    public static readonly PermissionKey Joker = new("*");

    private readonly string Value;

    public PermissionKey(string value)
    {
        Value = Guard.Encode(value, nameof(value));
    }

    public override string ToString()
    {
        return Value;
    }
}