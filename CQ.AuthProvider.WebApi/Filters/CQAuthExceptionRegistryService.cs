using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.AuthProvider.BusinessLogic.EmailVerifications.Exceptions;
using CQ.AuthProvider.BusinessLogic.Roles;
using CQ.AuthProvider.BusinessLogic.Roles.Exceptions;
using CQ.AuthProvider.BusinessLogic.Sessions.Exceptions;
using CQ.Exceptions;
using System.Net;

namespace CQ.AuthProvider.WebApi.Filters;

internal sealed class CQAuthExceptionRegistryService
    : ExceptionStoreService
{
    protected override void RegisterBusinessExceptions()
    {
        #region Specific exceptions
        #region Role controller
        #region Create
        
            AddOriginExceptions(new("Role", "Create"))
            .AddException<PermissionNotFoundException>(
            HttpStatusCode.Conflict,
            "ResourceNotFound",
            (exception, context) => $"The following permissions are incorrect '{string.Join(',', exception.PermissionKeys)}'"
            );
        #endregion

        #region Add permission
        
            AddOriginExceptions(new("Role", "AddPermission"))
            .AddException<PermissionNotFoundException>(
            HttpStatusCode.Conflict,
            "ResourceNotFound",
            (exception, context) => $"The following permissions are incorrect '{string.Join(',', exception.PermissionKeys)}'"
            )
            .AddException<PermissionsDuplicatedException>(
            HttpStatusCode.Conflict,
            "PermissionsDuplicated",
            (exception, context) => $"The following permissions are duplicated '{string.Join(',', exception.Keys)}"
            );
        #endregion
        #endregion

        #region Auth controller
        
            AddOriginExceptions(
            new("Auth", "CreateCredentials"))
            .AddException<ResourceDuplicatedException>(
                HttpStatusCode.Conflict,
                "DuplicatedEmail",
                "Exist another account with email provided"
                )
            .AddException<SpecificResourceNotFoundException<Role>>(
            HttpStatusCode.Conflict,
            "InvalidRole",
            (exception, context) => "The role provided does not exist"
            )
            .AddException<InvalidCredentialsException>(
            HttpStatusCode.InternalServerError,
            "InvalidSession",
            (exception, context) => $"Operation failed due to an error in creating a session"
            )
            .AddException<AuthDisabledException>(
            HttpStatusCode.InternalServerError,
            "InvalidSession",
            (exception, context) => $"Operation due to an error in creating a session"
            );
        #endregion
        #endregion

        #region Generic exceptions
        
            AddGenericException<InvalidCredentialsException>(
            HttpStatusCode.BadRequest,
            "InvalidCredentials",
            (exception, context) => $"The credentials provided are incorrect"
            )

            .AddGenericException<AuthDisabledException>(
            HttpStatusCode.BadRequest,
            "AccountDisabled",
            (exception, context) => $"The account is disabled",
            (exception, context) => $"The account with '{exception.Email}' is disabled"
            )

            .AddGenericException<EmailNotVerifiedException>(
            HttpStatusCode.Forbidden,
            "EmailNotVerified",
            (exception, context) => $"The email is not verified",
            (exception, context) => $"The account with '{exception.Email}' has not verified its email"
            )

            .AddGenericException<EmailAlreadyVerifiedException>(
            HttpStatusCode.Conflict,
            "EmailAlreadyVerified",
            (exception, context) => $"The email is already verified",
            (exception, context) => $"The account with '{exception.Email}' has already verified its email"
            );
        #endregion
    }
}
