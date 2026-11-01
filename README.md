# Proyecto Final - Bases de Datos I

Proyecto académico de la Universidad del Quindío: sistema de información para gestionar proveedores, productos, categorías y usuarios, desarrollado con C#, .NET 10 y MySQL.

## Integrantes

- Mateo Gómez Marulanda
- Andres David Parra
- Juan David Castillo Hernandez

## Contexto del proyecto

Las diversas empresas de carácter comercial requieren de sistemas de información que les permita determinar los proveedores a los que se pueden realizar los pedidos de sus productos. Los proveedores pueden estar en cualquier ciudad de Colombia y, es necesario almacenar los datos de estos. Así mismo, es fundamental poder identificar los productos, con sus datos, que cada proveedor puede enviar. Es importante determinar que, un Proveedor puede proveer varios productos y, que un Producto puede ser adquirido de varios proveedores. Los productos se pueden clasificar en Categorías, lo cual podrá identificar las familias de Productos. De las Categorías, también es importante almacenar sus datos básicos.

La aplicación deberá contar con un sistema básico de seguridad que consiste en un login de usuarios. Habrán tres tipos de usuarios: El Administrador (solo 1), los usuarios tradicionales y los usuarios esporádicos. Solo el Administrador puede crear nuevos usuarios. Los usuarios tradicionales pueden ejecutar los CRUD para la gestión de datos. Los usuarios esporádicos solo podrán ejecutar las consultas. También, se contará con una Bitácora que almacenará los datos del ingreso y la salida de los usuarios del sistema (fechas y horas de entrada y salida).

### Cómo se traduce el enunciado al proyecto

| Concepto del enunciado | En el proyecto |
| --- | --- |
| Administrador (solo 1) | `RolUsuario.Administrador` (id 1) |
| Usuario tradicional | `RolUsuario.Proveedor` (id 2): gestiona sus productos ofertados |
| Usuario esporádico | `RolUsuario.Vendedor` (id 3): solo consulta |
| Proveedor con empresa y ciudad | `Usuario` con rol Proveedor; `nombreempresa` y `ciudad` |
| Proveedor ofrece varios productos y un producto tiene varios proveedores | Tabla puente `productosofertados` (relación muchos a muchos con precio y tiempo de entrega) |
| Categorías de productos | Tabla `categoria` y enum `CategoriaProducto` |
| Login | `AutenticacionService` con contraseñas protegidas con bcrypt |
| Bitácora de ingreso y salida | **No implementada** (ver [Pendiente](#pendiente-respecto-al-enunciado)) |

## Estado actual

El backend está implementado como una biblioteca de clases dentro de un proyecto de consola. Contiene:

- Modelos de dominio para usuarios (con rol), productos y productos ofertados.
- DAO que acceden a MySQL con SQL parametrizado (`MySql.Data`).
- Servicios con las reglas de negocio, la autenticación (login con bcrypt) y la autorización por rol.
- Un script SQL (`BDproyecto.sql`) con el esquema y datos iniciales.

**El proyecto todavía no tiene interfaz.** [Program.cs](ProyectoBD/Program.cs) solo imprime `Hello, World!`; los servicios se pueden usar desde código pero ningún flujo los invoca.

## Estructura de carpetas

```text
ProyectoFinalBD1/
├── BDproyecto.sql                      # Esquema y datos iniciales de MySQL
├── README.md
├── He gawn                             # Archivo suelto sin uso en el código
└── ProyectoBD/
    ├── Program.cs                      # Punto de entrada actual (solo "Hello, World!")
    ├── ProyectoBD.csproj               # .NET 10; paquetes MySql.Data y BCrypt.Net-Next
    └── Backend/
        └── src/
            ├── Model/                  # Entidades y reglas de dominio
            │   ├── Usuario.cs
            │   ├── RolUsuario.cs
            │   └── Producto/
            │       ├── Producto.cs
            │       ├── ProductoOfertado.cs
            │       └── CategoriaProducto.cs
            ├── Data/                   # Acceso a MySQL
            │   ├── Conexion.cs
            │   ├── UsuarioDAO.cs
            │   ├── ProductoDAO.cs
            │   ├── ProductoOfertadoDAO.cs
            │   ├── DAOInterface/       # IUsuarioDAO, IProductoDAO, IProductoOfertadoDAO
            │   └── exception/          # Excepciones propias de los DAO
            └── Service/                # Casos de uso y reglas de negocio
                ├── AutentifiacionService.cs     # clase AutenticacionService
                ├── AutorizacionService.cs
                ├── UsuarioService.cs
                ├── ProductoService.cs
                ├── ProductoOfertadoService.cs   # contiene la clase ProveedorService
                └── exception/          # Excepciones de las reglas de negocio
```

Las carpetas `bin/` y `obj/` son artefactos generados por .NET y están excluidas por [.gitignore](.gitignore).

> El archivo `ProductoOfertadoService.cs` declara la clase `ProveedorService`, y `AutentifiacionService.cs` declara `AutenticacionService`. Los nombres de archivo y de clase no coinciden.

## Arquitectura

El código se organiza en tres capas, y cada una solo conoce a la inferior:

```text
Service   -> reglas de negocio, autenticación y autorización por rol
   |
Data      -> DAO: SQL parametrizado sobre MySQL (interfaces en DAOInterface/)
   |
MySQL     -> esquema definido en BDproyecto.sql
```

- **`Model`**: entidades que validan sus propios datos en el constructor. No contienen SQL.
- **`Data`**: cada DAO implementa una interfaz y solo lee o escribe datos. Los errores de MySQL se envuelven en `ErrorDeBaseDeDatos` (en `UsuarioDAO`).
- **`Service`**: decide qué se permite. Recibe los DAO por constructor (como interfaces), de modo que se pueden sustituir por dobles en las pruebas.

### Roles y permisos

| Rol | Id | Qué puede hacer |
| --- | --- | --- |
| Administrador | 1 | Crear, actualizar y eliminar usuarios (`UsuarioService`); crear y eliminar productos (`ProductoService`) |
| Proveedor (tradicional) | 2 | Crear y eliminar productos (`ProductoService`); crear, consultar, actualizar el precio y eliminar sus productos ofertados (`ProveedorService`) |
| Vendedor (esporádico) | 3 | Consultas |

`AutorizacionService.ValidarRol(idSolicitante, rol)` consulta el rol del usuario en la base de datos y lanza `UnauthorizedAccessException` si no coincide. Las operaciones de escritura de `UsuarioService` exigen Administrador, y las de `ProveedorService` exigen Proveedor.

Reglas del administrador implementadas en `UsuarioService`:

- No se puede crear un usuario con rol Administrador, para mantener un solo administrador.
- No se puede actualizar ni eliminar a un administrador.
- No se puede crear un usuario con un id, correo o cédula ya existentes.

## Servicios

### `AutenticacionService`
- `Login(correo, contrasena)`: verifica el hash bcrypt y devuelve el `Usuario`. Lanza `UnauthorizedAccessException` si las credenciales son inválidas. Ejecuta siempre una verificación, exista o no el correo, para igualar tiempos de respuesta.
- `CambiarContrasena(idUsuario, actual, nueva)`: exige la contraseña actual.
- `ValidarContrasenaNueva`: mínimo 8 caracteres.

### `UsuarioService`
- Administrador: `CrearUsuario`, `ActualizarUsuario`, `EliminarUsuario`.
- Consultas abiertas: `ObtenerUsuarioPorId`, `ObtenerUsuarioPorCorreo`, `ObtenerUsuarioPorCedula`, `ObtenerRolUsuarioPorId`, `ObtenerUsuarioPorIdTipo` (acepta el número o el nombre del rol), `ObtenerProveedorPorCiudad`, `ObtenerProveedorPorNombreEmpresa`.
- Las contraseñas se guardan solo como hash bcrypt en la columna `usuario.contrasena`; la entidad `Usuario` nunca contiene la contraseña.

### `ProductoService`
- `CrearProducto(idSolicitante, producto)` (rechaza ids duplicados) y `EliminarProducto(idSolicitante, idProducto)` (solo si ningún proveedor lo ofrece). Ambos exigen rol Proveedor o Administrador (`AutorizacionService.ValidarRol`).
- `ConsultarProducto` está abierto a cualquier usuario.

### `ProveedorService`
- `CrearProductoOfertado`, `EliminarProductoOfertado`, `ActualizarPrecioProductoOfertado` y `ConsultarProductoOfertadoPorProveedor`, todos para el proveedor solicitante.
- `ConsultarProducOfertadoPorIdProducto`: devuelve la lista de ofertas de un producto (puede haber varios proveedores).

## Acceso a datos

| DAO | Operaciones |
| --- | --- |
| `UsuarioDAO` | Consultar por id, correo, cédula, rol, tipo, ciudad y empresa; crear, actualizar, eliminar, comprobar existencia; leer y actualizar el hash de contraseña |
| `ProductoDAO` | Crear, consultar por id, actualizar, eliminar y comprobar existencia |
| `ProductoOfertadoDAO` | Crear (valida proveedor y producto en una transacción), actualizar precio, consultar por proveedor o por producto, comprobar existencia y eliminar |

Detalles de implementación:
- **Categorías:** `categoria.Cacodigo` empieza en 1 y el enum `CategoriaProducto` en 0, por lo que `ProductoDAO` suma 1 al guardar y resta 1 al leer.
- **Proveedores:** son filas de `usuario` con `rolusuario = 2`; `nombreempresa` y `ciudad` solo se usan con ese rol.
- **Excepciones:** `Data/exception/` (acceso a datos) y `Service/exception/` (reglas de negocio, por ejemplo correo o cédula duplicados).

### Conexión

`Conexion` crea las conexiones y expone `ProbarAsync()` para comprobarla. La cadena se arma así:

1. `MYSQL_CONNECTION_STRING`, si está definida.
2. Si no, las variables `MYSQL_HOST` (`localhost`), `MYSQL_PORT` (opcional), `MYSQL_DATABASE` (`proyecto`), `MYSQL_USER` (`root`) y `MYSQL_PASSWORD` (vacía).

Usa `SslMode=Preferred` y un tiempo de espera de 5 segundos. La cadena se lee una sola vez al primer uso. No incluyas contraseñas reales en el repositorio.

## Base de datos

[BDproyecto.sql](BDproyecto.sql) crea la base `proyecto` (MySQL 8.0 o superior, por su collation `utf8mb4_0900_ai_ci`) con estas tablas:

| Tabla | Contenido |
| --- | --- |
| `rolusuario` | Roles: 1 Administrador, 2 proveedor, 3 vendedor |
| `categoria` | `Cacodigo`, `nombre` (10 categorías iniciales) |
| `usuario` | Datos personales, `rolusuario`, `nombreempresa`, `ciudad` y `contrasena` (hash). `correo` y `cedula` son únicos; `nombre`, `cedula`, `correo`, `fechanacimiento` y `contrasena` son obligatorios |
| `producto` | `Prid`, `nombre`, `descripcion`, `categoria` (20 productos iniciales) |
| `productosofertados` | `proveedorid`, `producto`, `precio`, `tiempoentrega` (sin datos iniciales) |

> El script **borra y recrea** las tablas (`DROP TABLE IF EXISTS`), así que no lo ejecutes sobre datos que quieras conservar.

### Usuarios iniciales (solo para desarrollo)

| Rol | Correo | Contraseña |
| --- | --- | --- |
| Administrador | `s@email.com` | `Admin12345` |
| Proveedor | `d@email.com` | `Proveedor12345` |
| Vendedor | `v@email.com` | `Vendedor12345` |

Los hashes están versionados en el script, así que cualquiera con acceso al repositorio puede deducir estas contraseñas. **Deben cambiarse** en cualquier entorno real.

## Requisitos

- .NET SDK 10.
- MySQL Server 8.0 o superior.
- Paquetes NuGet `MySql.Data` 9.4.0 y `BCrypt.Net-Next` 4.2.0, restaurados automáticamente.

## Cómo ejecutar

1. **Crear la base de datos** (pide la contraseña de MySQL):

   ```bash
   mysql -u tu_usuario -p < BDproyecto.sql
   ```

2. **Configurar la conexión** si tu MySQL no usa los valores por defecto (`localhost`, usuario `root`, sin contraseña):

   ```bash
   export MYSQL_HOST=localhost
   export MYSQL_PORT=3306
   export MYSQL_DATABASE=proyecto
   export MYSQL_USER=tu_usuario
   export MYSQL_PASSWORD='tu contraseña'
   ```

3. **Compilar y ejecutar**:

   ```bash
   cd ProyectoBD
   dotnet restore
   dotnet build
   dotnet run
   ```

   Por ahora solo imprime `Hello, World!`: el proyecto compila, pero no hay menú ni flujo interactivo. Para usar los servicios hay que crear los DAO y servicios desde código, por ejemplo:

   ```csharp
   var usuarioDao = new UsuarioDAO();
   var autenticacion = new AutenticacionService(usuarioDao);
   Usuario admin = autenticacion.Login("s@email.com", "Admin12345");
   ```

`Program.cs` no llama a `Conexion.ProbarAsync()`, por lo que un error de configuración solo aparece cuando un DAO abre su primera conexión.

## Pruebas

La carpeta de pruebas (xUnit) fue retirada del repositorio y está pendiente de reconstruirse. El plan es usar:

- Pruebas de servicios con DAO falsos (sin base de datos).
- Pruebas de integración de los DAO contra una base separada cuyo nombre termine en `_test` (por ejemplo `proyecto_test`), cargando `BDproyecto.sql` sin sus sentencias `CREATE DATABASE` y `USE` para no tocar la base de desarrollo.

`ProyectoBD.csproj` excluye `Backend/tests/**` de la compilación del proyecto principal.

## Pendiente respecto al enunciado

Funcionalidades que el enunciado pide y todavía no están (o no están completas):

1. **Bitácora de ingreso y salida.** No existe tabla ni código. Debe guardar usuario, fecha y hora de entrada y de salida, y registrarse en el login y el logout.
2. **Sesión y cierre de sesión.** `AutenticacionService.Login` devuelve el usuario, pero no hay sesión ni `Logout`. Lo necesita la bitácora.
3. **Gestión de categorías.** Existe la tabla y el enum, pero no hay modelo, DAO ni servicio para crear, consultar, actualizar o eliminar categorías y sus datos básicos.
4. **Actualizar productos.** `ProductoService` no tiene operación de actualización; `ProductoDAO.Actualizar` existe pero ningún servicio la expone.
5. **Consultas para el usuario esporádico.** Faltan búsquedas de productos por categoría, nombre y rango de precio, y de ofertas por proveedor o ciudad.
6. **Proveedores de cualquier ciudad de Colombia.** La ciudad es texto libre, sin validación ni catálogo de ciudades.
7. **Un solo administrador a nivel de base de datos.** Hoy solo lo impide el servicio; la tabla no lo garantiza.
8. **`productosofertados` sin llave primaria** `(proveedorid, producto)`: la base permite ofertas repetidas.
9. **Interfaz de usuario.** Menú de consola o API web con login, y un `Program.cs` que arme los servicios.
10. **Pruebas automatizadas** (ver la sección anterior).

Mejoras técnicas que se dejaron anotadas: longitudes de columna cortas sin validar en el modelo (`correo` 25, `telefono` 10, `nombreempresa` 20), manejo de errores de MySQL solo en `UsuarioDAO`, y renombrar `ProveedorService` y `AutenticacionService` para que coincidan con sus archivos.
