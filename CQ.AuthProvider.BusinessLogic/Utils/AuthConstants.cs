namespace CQ.AuthProvider.BusinessLogic.Utils;
public static class AuthConstants
{
    #region Role
    public static readonly Guid TENANT_OWNER_ROLE_ID = Guid.Parse("cf4a209a-8dbd-4dac-85d9-ed899424b49e");
    public static readonly string TENANT_OWNER_ROLE_NAME = "Tenant owner";

    public static readonly Guid SEED_ROLE_ID = Guid.Parse("77f7ff91-a807-43ac-bc76-1b34c52c5345");

    public static readonly Guid AUTH_WEB_API_OWNER_ROLE_ID = Guid.Parse("780a89b1-9fd3-4cf6-b802-2882ebb3db92");
    public static readonly string AUTH_WEB_API_OWNER_ROLE_NAME = "Auth Web API owner";

    public static readonly Guid APP_OWNER_ROLE_ID = Guid.Parse("4579a206-b6c7-4d58-9d36-c3e0923041b5");
    public static readonly string APP_OWNER_ROLE_NAME = "App owner";
    
    public static readonly Guid CLIENT_OWNER_ROLE_ID = Guid.Parse("01e55142-6b8c-4e7e-9d71-1e459d07796d");
    public static readonly string CLIENT_OWNER_ROLE_NAME = "Client owner";
    #endregion Role

    public static readonly Guid AUTH_WEB_API_APP_ID = Guid.Parse("f4ad89eb-6a0b-427a-8aef-b6bc736884dc");
    public static readonly string AUTH_WEB_API_APP_NAME = "Auth Provider Web API";

    public static readonly Guid SEED_ACCOUNT_ID = Guid.Parse("0ee82ee9-f480-4b13-ad68-579dc83dfa0d");

    public static readonly Guid SEED_TENANT_ID = Guid.Parse("882a262c-e1a7-411d-a26e-40c61f3b810c");

    public static readonly Guid CREATE_CLIENT_APP_PERMISSION_ID = Guid.Parse("87013d07-c8ba-48f1-bb8c-510b7836fe1f");

    public static readonly Guid CREATE_CREDENTIALS_FOR_PERMISSION_ID = Guid.Parse("046c65a8-d3c1-41d7-bda2-a96d393cc18e");

    /// <summary>
    /// Vista global: quien lo tiene lee cuentas y apps de <b>cualquier</b> tenant, no solo del
    /// suyo. Es privado y lo tiene solo el rol Auth Web API owner.
    /// </summary>
    public static readonly Guid GET_ALL_CROSS_TENANT_ACCOUNT_PERMISSION_ID = Guid.Parse("00000000-0000-0000-0000-000000000010");
    public static readonly string GET_ALL_CROSS_TENANT_ACCOUNT_PERMISSION_KEY = "getallcrosstenant-account";
}
