﻿using AutoMapper;
using CQ.AuthProvider.BusinessLogic.Accounts;
using CQ.UnitOfWork.Abstractions;
using CQ.Utility;
using System.Linq;

namespace CQ.AuthProvider.BusinessLogic.Authorizations
{
    internal sealed class RoleMongoService : RoleService<RoleMongo>
    {
        private readonly IRepository<RoleMongo> _roleRepository;
        private readonly IPermissionInternalService<PermissionMongo> _permissionService;

        public RoleMongoService(
            IRepository<RoleMongo> roleRepository,
            IPermissionInternalService<PermissionMongo> permissionService,
            IMapper mapper) 
            : base(mapper)
        {
            this._roleRepository = roleRepository;
            this._permissionService = permissionService;
        }

        #region AddPermissionsByIdAsync
        protected override async Task AddPermissionsByIdAsync(RoleMongo role, List<PermissionKey> permissions)
        {
            var permissionsMapped = base._mapper.Map<List<string>>(permissions);

            role.Permissions = role.Permissions.Concat(permissionsMapped).ToList();

            await this._roleRepository.UpdateAsync(role).ConfigureAwait(false);
        }

        protected override async Task<RoleMongo> GetByIdAsync(string id)
        {
            var role = await this._roleRepository.GetByIdAsync(id).ConfigureAwait(false);

            return role;
        }
        #endregion

        public override async Task<RoleMongo> GetByKeyAsync(RoleKey key)
        {
            var role = await this._roleRepository.GetByPropAsync(key.ToString(), nameof(Role.Key)).ConfigureAwait(false);

            return role;
        }

        public override async Task<Role> GetDefaultAsync()
        {
            var role = await this._roleRepository.GetAsync(r => r.IsDefault).ConfigureAwait(false);

            return base._mapper.Map<Role>(role);
        }

        protected override async Task CreateAsync(CreateRole newRole)
        {
            await this._permissionService.AssertByKeysAsync(newRole.PermissionKeys).ConfigureAwait(false);

            var role = new RoleMongo(
                newRole.Name,
                newRole.Description,
                newRole.Key,
                newRole.PermissionKeys,
                newRole.IsPublic);

            await this._roleRepository.CreateAsync(role).ConfigureAwait(false);
        }

        protected override async Task RemoveDefaultAsync()
        {
            var roleDefault = await this._roleRepository.GetOrDefaultAsync(r => r.IsDefault).ConfigureAwait(false);

            if (Guard.IsNull(roleDefault))
                return;

            roleDefault.IsDefault = false;
            await this._roleRepository.UpdateAsync(roleDefault).ConfigureAwait(false);
        }
        protected override async Task<bool> ExistByKeyAsync(RoleKey key)
        {
            var roleValue = key.ToString();

            var existExist = await this._roleRepository.ExistAsync(r => r.Key == roleValue).ConfigureAwait(false);

            return existExist;
        }

        protected override async Task<List<Role>> GetAllAsync(Account accountLogged, bool isPrivate = false)
        {
            var roles = await this._roleRepository.GetAllAsync(r => r.IsPublic != isPrivate).ConfigureAwait(false);

            return base._mapper.Map<List<Role>>(roles);
        }

        protected override async Task<bool> HasPermissionAsync(List<string> roles, string permissionKey)
        {
            var hasPermission = await this._roleRepository.ExistAsync(r => roles.Contains(r.Key) && r.Permissions.Contains(permissionKey)).ConfigureAwait(false);

            return hasPermission;
        }

        #region CreateBulk
        protected override async Task<List<Role>> GetAllByRoleKeyAsync(List<string> roles)
        {
            var rolesSaved = await this._roleRepository.GetAllAsync(r => roles.Contains(r.Key)).ConfigureAwait(false);

            return base._mapper.Map<List<Role>>(rolesSaved);
        }

        protected override async Task CreateBulkAsync(List<CreateRole> roles)
        {
            var permissionKeys = roles
                .SelectMany(r => r.PermissionKeys)
                .ToList();

            await this._permissionService.AssertByKeysAsync(permissionKeys).ConfigureAwait(false);

            var rolesToSave = roles
                .Select(r =>
                    new RoleMongo(
                        r.Name,
                        r.Description,
                        r.Key,
                        r.PermissionKeys,
                        r.IsPublic,
                        r.IsDefault))
                .ToList();

            await this._roleRepository.CreateBulkAsync(rolesToSave).ConfigureAwait(false);
        }
        #endregion
    }
}