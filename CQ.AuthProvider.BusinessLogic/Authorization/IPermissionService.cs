﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.AuthProvider.BusinessLogic
{
    public interface IPermissionService
    {
        Task<IList<MiniPermission>> GetAllAsync();

        Task CreateAsync(CreatePermission permission);
    }
}