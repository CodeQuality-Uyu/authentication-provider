﻿using CQ.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.AuthProvider.BusinessLogic.Abstractions.Roles
{
    public sealed record class RoleKey
    {
        public static readonly RoleKey Admin = new("admin");
        public static readonly RoleKey ClientSystem = new("clientSystem");
        public static readonly RoleKey AuthProviderClientSystem = new("authProviderClientSystem");

        private readonly string Value;

        public RoleKey(string value)
        {
            Value = Guard.Encode(value, nameof(value));
        }

        public override string ToString()
        {
            return Value;
        }
    }
}