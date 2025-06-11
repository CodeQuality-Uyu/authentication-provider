# Authentication Provider

El servidor de autenticacion es una Web Apdi RestFull desarrollada en .NET usando EfCore como ORM para conectarse a dos bases de datos SQL. Una base de datos (`AuthProvider`) guarda la informacion de las cuentas, tenants, apps, roles, permisos, etc. Luego esta la base de datos (`IdentityProvider`) que guarda las credenciales de las cuentas de forma independiente a las cuentas. Esta separacion de informacion permite mayor flexibilidad de configuracion para la data sensible de la no sensible.

Esta web api es multi tenant y multi app, lo que significa que multiples tenant podran gestionar multiples aplicaciones propias dentro del sistema, filtrando la data por tenant y por app.

Esto es util para un tenant que tiene multiples app que se quiere que utilicen el mismo mecanismo de autenticacion y poder cruzar las cuentas entre las apps del mismo tenant.

## Configuraciones

### AppSettings
En el `appsettings.json` se podra ver las variables que se utilizan en la web api.

#### Logging y ConnectionStrings
La seccion de `Logging` es para indicar que logs utilizar para ciertas secciones o tecnologias utilizadas. Luego la seccion `ConnectionStrings` es para indicar los dos connection strings a las dos vaces necesarias, `Auth` para conectarse a la base `AuthProvider`, que es donde se guarda informasion no sensible y luego el connection string `Identity` que es para conectarse a la base `IdentityProvider` que guarda informacion sensible.

#### Authentication
Luego tenemos la seccion `Authentication` que es la seccion para agilizar la hora de desarrollo local ya que simula un usuario autenticado sin pasar por el proceso de autenticacion y expiracion de la sesion, esta seccion no tiene que completarse o estar en un ambiente de produccion.

#### DatabaseEngine
La seccion `DatabaseEngine`, permite seleccionar el motor de la base de datos para las bases necesarias mencionadas anteriormente en la seccion `ConnectionStrings`. Con esta flexibilidad se puede tener motores de bases de datos distintos para las dos bases, el mismo motor o unificar las bases en una haciendo que ambos connection strings apunten a la misma base. Los valores permitidos son `Sql` y `Postgres`. Al tener esta configuracion en el archivo, permite la facil adaptacion sin tener que modificar codigo existente.

#### Blob
La seccion `Blob`, permite configurar el servicio de almacenamiento de objetos, esta pensado para que configure el servicio `S3` de `AWS`. En este tambien se puede encontrar una configuracion `Fake` para agilizar el desarrollo y no depender de una instancia real de este servicio. Los campos requeridos son `AccessToken` y `SecretToken`, independientemente si se usa con un servicio real o no por el momento. En caso de querer usar un blob fake, no es necesario poner un valor en `Region` pero si un valor en `ServiceUrl` y viceversa para la situacion que se quiere usar un servicio real.

#### Cors
La ultima seccion, `Cors`, sirve para configurar los cors de la plataforma sin necesidad de realizar modificaciones de codigo.

```JSON
{
  "Logging": {
    "LogLevel": {
      "Default": "", // General logging
      "Microsoft": "", // Microsoft libraries, including EF Core
      "Microsoft.EntityFrameworkCore": "" // Detailed EF Core logging
    }
  },
  "ConnectionStrings": {
    "Auth": "Server=localhost,1430; Database=AuthProvider; User ID=sa; Password=MySuperStrongPassword1(!); TrustServerCertificate=true;",
    "Identity": "Server=localhost,1430; Database=IdentityProvider; User ID=sa; Password=MySuperStrongPassword1(!); TrustServerCertificate=true;",
  },
  "Authentication": {
    "Fake": {
      "IsActive": true,
      "Id": "0ee82ee9-f480-4b13-ad68-579dc83dfa0d",
      "FirstName": "test",
      "LastName": "test",
      "FullName": "test",
      "Email": "test@gmailcom",
      "Token": "",
      "AppLoggedId": "f4ad89eb-6a0b-427a-8aef-b6bc736884dc",
      "Locale": "America/Uruguay",
      "TimeZone": "-3",
      "AppsIds": [ "f4ad89eb-6a0b-427a-8aef-b6bc736884dc" ],
      "TenantId": "882a262c-e1a7-411d-a26e-40c61f3b810c",
      "PermissionsKeys": [ "create-app","getall-tenant" ],
      "RolesIds": [ "77f7ff91-a807-43ac-bc76-1b34c52c5345" ]
    }
  },
  "DatabaseEngine": {
    "Auth":"Sql",
    "Identity: "Sql"
  },
  "Blob": {
    "Fake": {
      "IsActive": true,
      "ServiceUrl": "http://localhost:9000"
    },
    "AccessToken": "test",
    "SecretToken": "test",
    "Region": "us-east-1"
  },
  "Cors": {
    "Origins": ["*"]
  }
}
```

### Variables de ambiente
Las variables de ambiente pueden sustituir los valores del `appsettings` mencionadas anteriormente, solo se tiene que indicar con el formato de usar `__` (doble barra baja), cuando se adentra entre la navegacion. Por ejemplo si se quiere modificar los connection strings, se debe crear las variables de ambiente `ConnectionStrings__Auth` y `ConnectionStrings__IdentityProvider`. A su vez existe la siguiente variable de ambiente `ASPNETCORE_ENVIRONMENT` que indica el ambiente en el cual se esta ejecutando la web api. Los valores pueden ser desde `Local`, `Docker`, `Staging`, `Testing` y `Production`, pero no si existen mas ambientes pueden ser indicados.

```
ASPNETCORE_ENVIRONMENT: "Docker"
ConnectionStrings__Auth: "Server=sqlserver,1433; Database=AuthProvider; User ID=sa; Password=MySuperStrongPassword1(!); TrustServerCertificate=true;"
ConnectionStrings__Identity: "Server=sqlserver,1433; Database=IdentityProvider; User ID=sa; Password=MySuperStrongPassword1(!); TrustServerCertificate=true;"
Authentication__Fake__IsActive: false
DatabaseEngine__Auth: Sql
DatabaseEngine__Identity: Sql
Blob__Fake__IsActive: true
Blob__Fake__ServiceUrl: "http://localhost:9000"
Blob__AccessToken: test
Blob__SecretToken: test
```

## Uso de autenticacion mockeada
La autenticacion mockeada sirve para agilizar la hora de desarrollo para no tener que depender de cuentas con los roles y permisos que se desean testear. Tambien sirve para evitar pasar por el proceso de autenticacion cada vez que se necesita ya que esta no expira. Para utilizarlo, la flag `Authentication:IsFake` tiene que estar activa, eso hace que se utilice la informacion de la cuenta descripta abajo de la declaracion de esta flag, una vez activa, la request si desea utilizar esta informacion no debera de proveer un `Bearer token` en el header `Authorization` de la request, esta combinacion de condiciones hace que use la info `hardcodeada`. 

## Definicion de entidades

### Empresa (Tenant)
Son las empresas registradas en el sistema que quieren usar los servicios de esta webapi para proveer un medio de autenticacion independiente a sus aplicaciones

```C#
public sealed record class Tenant
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public Guid MiniLogoId { get; init; }

    public Guid CoverLogoId { get;init; }

    public string WebUrl { get; set; }

    public Account Owner { get; init; }
}
```

### Aplicacion (App)
Son las aplicaciones de los tenant que quieren proveerles un medio de autenticacion y autorizacion, pudiendo desligar este diseño de los diseños de las aplicaciones.

```C#
public sealed record class App
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public Guid CoverId { get; init; }

    public Tenant Tenant { get; init; }

    public bool IsDefault { get; init; }
}
```

### Roles de las cuentas (Role)
Los roles son agrupadores de permisos para poder autorizar a una cuenta en si puede o no acceder a un recurso controlado. Los roles no son un medio de verificacion de permisos, los permisos son este medio.
Los roles son unicos para cada aplicacion, esto quiere decir que si un tenant tiene dos aplicaciones, estos no pueden compartir los mismos roles ya que los permisos en la aplicacion van a ser diferentes.

```C#
public sealed record class Role
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public string Description { get; init; }

    public bool IsPublic { get; init; }

    public List<Permission> Permissions { get; init; }

    public App App { get; init; }

    public Tenant Tenant { get; init; }

    public bool IsDefault { get; init; }
}
```

### Permisos de las aplicaciones (Permission)
Los permisos son el medio para verificar si una cuenta puede o no acceder al recurso controlado. Estos son unicos por cada aplicacion, esto implica que un mismo permiso no puede aparecer en aplicaciones diferentes del mismo tenant.

```C#
public sealed record class Permission
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public string Description { get; init; }

    public bool IsPublic { get; init; }

    public string Key { get; init; }

    public App App { get; init; }

    public Tenant Tenant { get; init; }
}
```
### Invitaciones a personas (Invitation)
Las invitaciones son a futuras cuentas con un rol asignado a una app determinada del tenant.

```C#
public sealed record class Invitation()
{
    public const int EXPIRATION_MINUTES

    public Guid Id { get; init; }

    public string Email { get; init; }

    public int Code { get; init; }

    public Account Creator { get; init; }

    public Role Role { get; init; }

    public App App { get; init; }

    public Tenant Tenant { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime ExpiresAt { get; init; }
}
```
### Reseteo de contraseña (ResetPassword)
Son solicitudes para reiniciar la contraseña de una cuenta que no puede loguearse. Esta solicitud requiere que la persona pueda acceder a su email personal con el cual se registro para poder obtener un codigo unico y asi poder resetear su contraseña sin estar autenticado.

```C#
public sealed record class ResetPassword
{
    public const int TOLERANCE_IN_MINUTES = 15;

    public Guid Id { get; init; }

    public Account Account { get; init; }

    public int Code { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime ExpiresAt { get; init; }
}
```

### Cuenta de usuario (Account)
Los usuarios se pueden registrar en aplicaciones precargadas anteriormente en el sistema de un tenant.

```C#
public record class Account
{
    public Guid Id { get; init; }

    public string Email { get; init; }

    public string FirstName { get; init; }

    public string LastName { get; init; }

    public string FullName { get; init; }

    public string? ProfilePictureId { get; init; }

    public string Locale { get; init; }

    public string TimeZone { get; init; }

    public List<Role> Roles { get; init; }

    public List<App> Apps { get; init; }

    public Tenant Tenant { get; init; }
}
```

### Sesiones (Session)
Son las sesiones activas de las cuentas. Para que un usuario pueda loguearse a una app, este a parte de enviar sus credenciales debera de proveer el identificador de la app a la cual se quiere loguear y este debera ser de una app en donde la cuenta este registrado. En caso de que no se provea, se utilizara la aplicacion establecida como por defecto en la que pertenezca la cuenta.

```C#
public sealed record class Session
{
    public Guid Id { get; init; }

    public string Token { get; init; }

    public Account Account { get; init; }

    public App App { get; init; }
}
```

## Para ejecutar
Para correr la web api con docker ejecutar el siguiente comando en la ruta raiz del repo
```
docker-compose up --build
```

Una vez ejecutado con exito el comando, se debio de haber creado dos contenedores bajo una misma red, un contenedor para las bases de datos y otro para la web api. El ambiente configurado en el docker compose tiene seteado para que no sea tratado como produccion, lo que significa que al haberse ejecutado por primera vez, las migraciones de ambas bases de datos se debieron de ejecutar automatiamente. Esto implica que al parar la ejecucion de los contenedores y volverlos a iniciar, se va a consultar si quedan migraciones por aplicar, en caso de que si, las bases se van actualizar automaticamente. Esto facilita el mantenimiento de la base y la seguridad de que siempre se este trabajando con la ultima version de la base de datos.

Esta configuracion se puede desactivar si la variable de ambiente `ASPNETCORE_ENVIRONMENT` se setea con el valor `Production`. 

Por defecto tiene desactivada la opcion de que se simule la autenticacion teniendo la variable de ambiente `Authentication__Fake__IsActive` con el valor `false`. Esta variable solo deberia de ser `true` en ambientes locales, para simular la autenticacion lo que implica poder saltearse la autenticacion sin tener que depender de cuentas con los permisos o roles con los que se desea probar. Este mecanismo permite agilidad a la hora de desarrollo de la web api. Esta variable de ambiente no es necesario setearla si el ambiente es `Production`.

## Seed data
La web Api inicia con una cuenta semilla, un tenant semilla, la app de Auth Provier Web Api como default dentro del tenant semilla, 3 roles y los permisos para Auth Provider Web Api.

### Cuenta semilla
```json
{
  "email":"seed@cq.com",
  "password":"!12345678"
}
```
Esta cuenta permite realizar las siguientes operaciones:
- Obtener los roles creados dentro del tenant (`getall-role`)
- Crear una cuenta a otra persona en la plataforma dentro del tenant perteneciente a la cuenta semilla (`createcredentialsfor-account`)

### Tenant semilla
```json
{
  "name": "Semilla"
}
```
A este tenant se le debe de cambiar el nombre y ser transferido a la cuenta que va a ser el dueño de la app.

### App Auth Provider Web Api
Es la app que representa el servidor de autenticacion para poder realizar operaciones especificas solo para esta aplicacion. El servidor de autenticacion permite gestionar varias aplicaciones de los tenants asi pudiendo gestionar los permisos de acceso de los recursos de las aplicaciones de las cuentas creadas en el servidor.

```json
{
  "name": "Auth Provider Web Api"
}
```

### Roles semilla
Existen 3 roles, estos roles solo pueden ser usados en la app `Auth Provider Web Api`

- `Seed` (este rol solo sirve para la cuenta semilla, se eliminara automaticamente cuando se cree una cuenta)
  
- `Tenant Owner` (las cuentas con este rol pueden acceder a recursos para gestionar las aplicaciones dentro del tenant y el tenant usando la app Auth Provider Web Api)
  
- `Auth Provider Api Owner` (las cuentas con este rol pueden acceder a recursos exclusivos dentro de la app Auth Provider Web Api)

### Permisos
Los permisos son los recursos y opciones a los cuales se les quiere restringir el acceso a las cuentas dentro de una app. Los permisos pertenecen unicamente a una app.

## Inicios con el servidor de autenticacion

### Paso 1
Loguearse con la cuenta semilla

- Endpoint: `POST sessions/credentials`
- Body:
```json
{
    "email": "seed@cq.com",
    "password": "!12345678"
}
```

### Paso 2
Crear una cuenta para otra persona con el rol de `Auth Provider Web Api`

- Endpoint: `POST accounts/credentials/for`
- Body:
```JSON
{
    "roleId": "c1664759-22eb-44be-8faf-88f6042faa92", // Deberia de ser el id del rol Auth Provider Web Api
    "email": "test@gmail.com",
    "locale":"uru",
    "lastname":"test",
    "firstname":"test",
    "password":"!12345678",
    "timezone":"-3"
}
```
Una vez ejecutado, la `identificacion`, `cuenta`, `rol` y cualquier otra cosa relacionado a la semilla, se debio haber eliminado automaticamente, dejando esta nueva cuenta como el nuevo owner del `tenant` de la semilla. La nueva cuenta tambien tendra el rol `Tenant Owner` de haber declarado otro.

### Paso 3
Loguearse con la nueva cuenta con el rol de `Auth Provider Web Api` y `Tenant Owner`.

### Paso 4
Actualizar el nombre del tenant
- Endpoint: `PATCH me/tenants/name`
- Body:
```JSON
{
    "newName": "CQ" //Nombre de la nueva empresa que va a gestionar Auth Provider Web Api
}
``` 

### Paso 5
Crear una nueva app
- Endpoint: `POST apps`
- Body:
```JSON
{
    "name": "somthing", // Nombre de la nueva app de la empresa
    "isDefault": false, // Para marcar que esta app es la por defecto en cualquier accion dentro del tenant. Solo puede existir una por defecto, en caso de que otra tenga esta flag activa, se le quitara a esa otra app
    "coverId": "cf4a209a-8dbd-4dac-85d9-ed899424b49e" // Identificador del blob
}
```

### Paso 6
Crear permisos a la nueva app para restringir el acceso a los recursos
- Endpoint: `POST permissions`
- Body:
```JSON
{
    "name": "Test",
    "description": "Test",
    "key": "getall-test", //Es la propiedad que se va a verificar si el cuenta tiene para poder acceder al recurso. En caso de que el permiso sea para un endpoint en particular, deberia de seguir el formato accion-controller, donde la accion es el nombre del metodo en el controller todo en minuscula y junto (en caso de que se use la palabra async, se debera de evitar) y el controller el nombre del controller. En caso de que se quiera crear un permiso para algun recurso particular independientemente a un endpoint, el formato puede ser libre
    "isPublic": false // Una flag que permite ser visible o no al permiso para aquellos que tengan el permiso de poder ver permisos privados
}
```

### Paso 7
Crear roles a la nueva app usando los permisos de la app
- Endpoint: `POST roles`
- Body:
```JSON
{
    "name": "Test",
    "description": "Test Test",
    "isDefault": true, //Una flag permite al rol ser usado para la creacion de cuentas dentro de una app en caso de que no se provea un id del rol en la creacion de la cuenta
    "isPublic": true, // Una flag que permite ser visible o no al rol para aquellos que tengan el permiso de poder ver roles privados
    "permissionsKeys": [ // Las keys de los permisos que se quiere que el rol agrupe
        "createcredentialsfor-account" 
    ]
}
```


