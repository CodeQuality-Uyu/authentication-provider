﻿using CQ.AuthProvider.BusinessLogic.Authorizations;
using CQ.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.AuthProvider.BusinessLogic.ClientSystems
{
    public sealed record class ClientSystem
    {
        public string Id { get; init; } = null!;

        public string Name { get; init; } = null!;

        public string PrivateKey { get; init; } = null!;

        public RoleKey Role { get; init; } = null!;

        public List<PermissionKey> Permissions { get; init; } = null!;

        public DateTimeOffset CreatedOn { get; init; }

        public bool HasPermission(PermissionKey permission)
        {
            return this.Permissions.Contains(permission);   
        }
    }
}