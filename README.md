# Proyecto Final - Bases de Datos I

Proyecto académico de la Universidad del Quindío: sistema de información para gestionar proveedores, productos, categorías y usuarios, desarrollado con C#, .NET 10 y MySQL.

## Integrantes

- Mateo Gómez Marulanda
- Andres David Parra
- Juan David Castillo Hernandez

## Contexto del proyecto

Las diversas empresas de carácter comercial requieren de sistemas de información que les permita determinar los proveedores a los que se pueden realizar los pedidos de sus productos. Los proveedores pueden estar en cualquier ciudad de Colombia y, es necesario almacenar los datos de estos. Así mismo, es fundamental poder identificar los productos, con sus datos, que cada proveedor puede enviar. Es importante determinar que, un Proveedor puede proveer varios productos y, que un Producto puede ser adquirido de varios proveedores. Los productos se pueden clasificar en Categorías, lo cual podrá identificar las familias de Productos. De las Categorías, también es importante almacenar sus datos básicos.

La aplicación deberá contar con un sistema básico de seguridad que consiste en un login de usuarios. Habrán tres tipos de usuarios: El Administrador (solo 1), los usuarios tradicionales y los usuarios esporádicos. Solo el Administrador puede crear nuevos usuarios. Los usuarios tradicionales pueden ejecutar los CRUD para la gestión de datos. Los usuarios esporádicos solo podrán ejecutar las consultas. También, se contará con una Bitácora que almacenará los datos del ingreso y la salida de los usuarios del sistema (fechas y horas de entrada y salida).

### Consultas y reportes mínimos que debe tener la aplicación

**Consultas**

- Se deben crear tres consultas con JOIN que junten al menos, datos de 3 tablas.
- Se deben construir y mostrar los datos arrojados por dos consultas que posean subconsultas y que relacionen dos tablas.

**Reportes** (archivo en .pdf o en una herramienta de reportes mostrado dentro de un formulario en la aplicación)

- Se deben tener en reportes (TRES diferentes) los datos filtrados por el usuario, de productos y proveedores.
- Se deben tener dos reportes que relacionen los Productos con sus Categorías y que los pueda filtrar el usuario.

### Cómo se traduce el enunciado al proyecto

| Concepto del enunciado | En el proyecto |
| --- | --- |
| Administrador (solo 1) | `RolUsuario.Administrador` (id 1) |
| Usuario tradicional | `RolUsuario.Proveedor` (id 2): gestiona sus productos ofertados |
| Usuario esporádico | `RolUsuario.Vendedor` (id 3): solo consulta |
| Proveedor con empresa y ciudad | `Usuario` con rol Proveedor; `nombreempresa` y `ciudad` |
| Proveedor ofrece varios productos y un producto tiene varios proveedores | Tabla puente `productosofertados` (relación muchos a muchos con precio y tiempo de entrega) |
| Categorías de productos | Tabla `categoria` y enum `CategoriaProducto` |
| Login | `AutentificacionService` con contraseñas protegidas con bcrypt |
| Bitácora de ingreso y salida | Tabla `bitacora` + `BitacoraDAO`; el login registra la entrada y `Logout` la salida (ver [Bitácora](#bitácora-de-entrada-y-salida)) |

## Estado actual

El backend está implementado como una biblioteca de clases dentro de un proyecto de consola. Contiene:

- Modelos de dominio para usuarios (con rol), productos, categorías (enum), productos ofertados y bitácora.
- DAO que acceden a MySQL con SQL parametrizado (`MySql.Data`).
- Servicios con las reglas de negocio, la autenticación (login con bcrypt) y la autorización por rol.
- Un script SQL (`BDproyecto.sql`) con el esquema y datos iniciales.

**El proyecto todavía no tiene interfaz gráfica.** [Program.cs](ProyectoBD/Program.cs) sin argumentos imprime `Hello, World!` y la ayuda del **modo de demostración**, que sirve para ver las consultas en la consola y generar los reportes en PDF (ver [Modo de demostración](#modo-de-demostración)). El resto de los servicios se pueden usar desde código.

## Estructura de carpetas

```text
ProyectoFinalBD1/
├── BDproyecto.sql                      # Esquema y datos iniciales de MySQL
├── README.md
├── He gawn                             # Archivo suelto sin uso en el código
└── ProyectoBD/
    ├── Program.cs                      # Punto de entrada: "Hello, World!" y ayuda, o modo de demostración
    ├── ProyectoBD.csproj               # .NET 10; paquetes MySql.Data, BCrypt.Net-Next y QuestPDF
    └── Backend/
        └── src/
            ├── Demo/
            │   └── Demostracion.cs     # Modo de demostración: consultas en consola y reportes en PDF
            ├── Model/                  # Entidades y reglas de dominio
            │   ├── Usuario.cs
            │   ├── RolUsuario.cs
            │   ├── RegistroBitacora.cs
            │   ├── Producto/
            │   │   ├── Producto.cs
            │   │   ├── ProductoOfertado.cs
            │   │   └── CategoriaProducto.cs
            │   └── Consulta/           # Resultados de las consultas combinadas
            │       ├── OfertaDetalle.cs
            │       ├── ProductoCategoria.cs
            │       ├── ResumenCategoria.cs
            │       └── PortafolioProveedor.cs
            ├── Data/                   # Acceso a MySQL
            │   ├── Conexion.cs
            │   ├── UsuarioDAO.cs
            │   ├── ProductoDAO.cs
            │   ├── ProductoOfertadoDAO.cs
            │   ├── ConsultaDAO.cs      # Consultas con JOIN y subconsultas
            │   ├── BitacoraDAO.cs      # Bitácora de entrada y salida
            │   ├── DAOInterface/       # IUsuarioDAO, IProductoDAO, IProductoOfertadoDAO, IConsultaDAO, IBitacoraDAO
            │   └── exception/          # Excepciones propias de los DAO
            └── Service/                # Casos de uso y reglas de negocio
                ├── AutentifiacionService.cs     # clase AutentificacionService
                ├── AutorizacionService.cs
                ├── UsuarioService.cs
                ├── ProductoService.cs
                ├── ProductoOfertadoService.cs   # contiene la clase ProdcutoOfertadoService
                ├── ConsultaService.cs
                ├── BitacoraService.cs
                ├── Report/             # Reportes en PDF (QuestPDF)
                │   ├── ReporteService.cs
                │   ├── GeneradorPdf.cs
                │   └── ColumnaReporte.cs
                └── exception/          # Excepciones de las reglas de negocio
```

Las carpetas `bin/` y `obj/` son artefactos generados por .NET y están excluidas por [.gitignore](.gitignore).

> El archivo `ProductoOfertadoService.cs` declara la clase `ProdcutoOfertadoService`, y `AutentifiacionService.cs` declara `AutentificacionService`. Los nombres de archivo y de clase no coinciden.

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
- **`Data`**: cada DAO implementa una interfaz y solo lee o escribe datos. `UsuarioDAO`, `BitacoraDAO` y `ConsultaDAO` envuelven los errores de MySQL en `ErrorDeBaseDeDatos`; `ProductoDAO` y `ProductoOfertadoDAO` dejan pasar `MySqlException`.
- **`Service`**: decide qué se permite. Recibe los DAO por constructor (como interfaces), de modo que se pueden sustituir por dobles en las pruebas.

### Flujo de una operación

```text
Program.cs
  └── Demostracion (modo consola) o código que arme los servicios
        └── Service
              ├── AutorizacionService -> IUsuarioDAO -> MySQL (rol del solicitante)
              └── DAO del dominio     -> Conexion    -> MySQL
                    └── Model / records (datos de entrada y de salida)
```

- `Demostracion` es el único punto del proyecto que arma los servicios: crea los DAO, inyecta `AutorizacionService` y `ConsultaService`, y llama a los métodos. No tiene reglas de negocio.
- `Conexion` es la única clase que construye la cadena de conexión; cada DAO pide una conexión nueva con `Conexion.Obtener()` y la abre con `Open()` (o `OpenAsync()` en `ProbarAsync`).
- Los servicios reciben interfaces (`IUsuarioDAO`, `IProductoDAO`, `IProductoOfertadoDAO`, `IConsultaDAO`, `IBitacoraDAO`), no las clases concretas. `ReporteService`, además, recibe `GeneradorPdf` y `AutorizacionService`.
- Los `Model` y los `record` de `Model/Consulta/` son la entrada y la salida de los servicios. Los reportes no devuelven modelos: devuelven el PDF como `byte[]`, y quien llama decide si lo guarda en disco.

### Roles y permisos

| Rol | Id | Qué puede hacer |
| --- | --- | --- |
| Administrador | 1 | Crear, actualizar y eliminar usuarios (`UsuarioService`); crear y eliminar productos (`ProductoService`); consultar la bitácora (`BitacoraService`) y generar el reporte de bitácora (`ReporteService`) |
| Proveedor (tradicional) | 2 | Crear y eliminar productos (`ProductoService`); crear, consultar, actualizar el precio y eliminar sus productos ofertados (`ProdcutoOfertadoService`) |
| Vendedor (esporádico) | 3 | Consultas |

`AutorizacionService.ValidarRol(idSolicitante, rol)` consulta el rol del usuario en la base de datos y lanza `UnauthorizedAccessException` si no coincide. Las operaciones de escritura de `UsuarioService` exigen Administrador, y las de `ProdcutoOfertadoService` exigen Proveedor.

Reglas del administrador implementadas en `UsuarioService`:

- No se puede crear un usuario con rol Administrador, para mantener un solo administrador.
- No se puede actualizar ni eliminar a un administrador.
- No se puede crear un usuario con un id, correo o cédula ya existentes.

## Modelos

- `Usuario`: entidad con id, datos personales y rol. Valida en el constructor que el id sea positivo y que nombre, dirección, teléfono, cédula, correo, fecha de nacimiento y género no estén vacíos. Si el rol es `Proveedor`, exige `NombreEmpresa` y `Ciudad`; para Administrador y Vendedor exige que ambos sean `null`. Expone `IdTipoUsuario` y `TipoUsuario` a partir del rol. El constructor es `protected` y `UsuarioDAO` lo invoca por reflexión (`Activator.CreateInstance`) para reconstruir la entidad desde MySQL.
- `RolUsuario`: enum con `Administrador = 1`, `Proveedor = 2` y `Vendedor = 3`; estos valores son los que se guardan en `usuario.rolusuario`.
- `Producto`: id positivo, nombre y descripción no vacíos, y una categoría que exista en `CategoriaProducto`.
- `ProductoOfertado`: relación entre proveedor y producto. Valida id de proveedor positivo, precio mayor que cero, tiempo de entrega no negativo y producto no nulo. El precio vive aquí, no en `Producto`, porque cada proveedor pone el suyo.
- `CategoriaProducto`: enum de las 10 categorías, de `Electronica` (0) a `Peliculas` (9). `ProductoDAO` lo traduce a la columna `producto.categoria` sumando 1.
- `RegistroBitacora`: `record` de una sesión (`IdBitacora`, `IdUsuario`, `NombreUsuario`, `FechaEntrada`, `FechaSalida`). `SalidaRegistrada` indica si `FechaSalida` tiene valor; `IdUsuario` y `NombreUsuario` son `null` si el usuario fue eliminado.
- `OfertaDetalle`, `ProductoCategoria`, `ResumenCategoria` y `PortafolioProveedor`: `record` de solo transporte para los resultados de `ConsultaDAO`. No validan nada y son también la entrada de los reportes.
- `ColumnaReporte`: `record` con el título y el ancho relativo de una columna del PDF.

## Servicios

### `AutentificacionService`
- `Login(correo, contrasena)`: verifica el hash bcrypt, **registra la entrada en la bitácora** y devuelve el `Usuario`. Lanza `UnauthorizedAccessException` si las credenciales son inválidas, y no registra nada en ese caso. Ejecuta siempre una verificación, exista o no el correo, para igualar tiempos de respuesta. Si no se puede registrar la entrada, el inicio de sesión falla.
- `Logout(idUsuario)`: **registra la salida**, cerrando la sesión abierta más reciente del usuario. Devuelve `false` si no tenía ninguna abierta.
- `CambiarContrasena(idUsuario, actual, nueva)`: exige la contraseña actual.
- `ValidarContrasenaNueva`: mínimo 8 caracteres.

### `UsuarioService`
- Administrador: `CrearUsuario`, `ActualizarUsuario`, `EliminarUsuario`.
- Consultas abiertas: `ObtenerUsuarioPorId`, `ObtenerUsuarioPorCorreo`, `ObtenerUsuarioPorCedula`, `ObtenerRolUsuarioPorId`, `ObtenerUsuarioPorIdTipo` (acepta el número o el nombre del rol), `ObtenerProveedorPorCiudad`, `ObtenerProveedorPorNombreEmpresa`.
- Las contraseñas se guardan solo como hash bcrypt en la columna `usuario.contrasena`; la entidad `Usuario` nunca contiene la contraseña.

### `ProductoService`
- `CrearProducto(idSolicitante, producto)` (rechaza ids duplicados) y `EliminarProducto(idSolicitante, idProducto)` (solo si ningún proveedor lo ofrece). Ambos exigen rol Proveedor o Administrador (`AutorizacionService.ValidarRol`).
- `ConsultarProducto` está abierto a cualquier usuario.

### `ProdcutoOfertadoService`
- `CrearProductoOfertado`, `EliminarProductoOfertado`, `ActualizarPrecioProductoOfertado` y `ConsultarProductoOfertadoPorProveedor`, todos para el proveedor solicitante.
- `ConsultarProducOfertadoPorIdProducto`: devuelve la lista de ofertas de un producto (puede haber varios proveedores).

### `BitacoraService`
- `ConsultarBitacora(idSolicitante, idUsuario?, desde?, hasta?)`: lista las sesiones (de la más reciente a la más antigua) con filtros opcionales por usuario y por rango de fechas de entrada. **Solo el Administrador** puede ejecutarla.

### `ReporteService`
Genera los reportes en PDF con [QuestPDF](https://www.questpdf.com) (licencia Community). Cualquier usuario puede generar los cinco reportes de productos y proveedores; el **reporte de bitácora es solo del Administrador** (ver [Reporte administrativo](#reporte-administrativo-bitácora-de-acceso)). Todos los filtros son opcionales: un filtro vacío no se aplica. Cada método devuelve el PDF como `byte[]`, para que quien lo pida decida si lo guarda en un archivo o lo muestra.

El flujo es el mismo para todos los reportes: `ReporteService` valida los filtros, pide los datos a `ConsultaDAO`, los convierte a texto y se los pasa a `GeneradorPdf`, que es la única clase que conoce QuestPDF y dibuja el PDF (título, fecha de generación, filtros aplicados, total de registros, tabla con el encabezado repetido en cada página, número de página y un aviso cuando no hay resultados). Cada columna se declara con `ColumnaReporte(título, ancho relativo)`; agregar un reporte consiste en dar formato a las filas y definir sus columnas en `ReporteService`.

#### Qué reportes hay y por qué

El enunciado pide cinco reportes: **tres de productos y proveedores** y **dos que relacionen productos con sus categorías**, todos filtrados por el usuario. Se eligieron de modo que cada uno responda una pregunta distinta del negocio y que, en conjunto, recorran las cuatro tablas del dominio (`usuario` como proveedor, `producto`, `categoria` y `productosofertados`).

**Regla de diseño: una consulta, una sola fuente.** Los reportes no tienen SQL propio: reutilizan las consultas de `ConsultaDAO` (las mismas de las pantallas de consulta) a las que se les agregan filtros opcionales. Así un cambio en una consulta se refleja en su reporte, y no hay dos versiones del mismo SQL que mantener. Solo el reporte 4 necesitó una consulta nueva (`ConsultarProductosPorCategoria`), porque ningún DAO tenía un listado de productos.

Se descartaron para los reportes los métodos de `UsuarioDAO`, `ProductoDAO` y `ProductoOfertadoDAO`: piden un id obligatorio o filtran por un único criterio, devuelven pocos datos (por ejemplo, el proveedor solo como número) y no traen los nombres de categoría y proveedor que necesita un reporte legible. Las consultas de `ConsultaDAO` ya cruzan esas tablas.

| # | Reporte | Grupo del enunciado | Fuente | Estado |
| --- | --- | --- | --- | --- |
| 1 | Proveedores y su portafolio | Productos y proveedores | `ConsultarPortafolioProveedores` | Hecho |
| 2 | Ofertas de los proveedores | Productos y proveedores | `ConsultarCatalogoOfertas` | Hecho |
| 3 | Productos y su mejor proveedor | Productos y proveedores | `ConsultarMejorOfertaPorProducto` | Hecho |
| 4 | Productos por categoría | Productos con categorías | `ConsultarProductosPorCategoria` (nueva) | Hecho |
| 5 | Resumen por categoría | Productos con categorías | `ConsultarResumenPorCategoria` | Hecho |

Métodos de `ReporteService`: `GenerarReporteProveedoresPortafolio` (1), `GenerarReporteOfertasPorProveedor` (2), `GenerarReporteMejorProveedorPorProducto` (3), `GenerarReporteProductosPorCategoria` (4) y `GenerarReporteResumenPorCategoria` (5). Todos devuelven `byte[]` y sus parámetros son los filtros opcionales de cada reporte. El reporte administrativo, `GenerarReporteBitacora`, se explica aparte.

**Reporte 1: Proveedores y su portafolio** (`GenerarReporteProveedoresPortafolio`)
- **Finalidad:** el enunciado exige almacenar los datos de los proveedores, que pueden estar en cualquier ciudad, y que un proveedor pueda proveer varios productos. Este reporte muestra cada proveedor con su empresa, ciudad y qué tan amplia es su oferta.
- **Tablas y cómo:** `usuario` (solo rol Proveedor), `productosofertados` y `producto`, unidas con `LEFT JOIN` para que también aparezcan los proveedores que todavía no ofrecen nada. Una fila por proveedor con la cantidad de productos y de categorías distintas, y el precio y el tiempo de entrega promedio.
- **Filtros:** ciudad y empresa.

**Reporte 2: Ofertas de los proveedores** (`GenerarReporteOfertasPorProveedor`)
- **Finalidad:** responde a "qué productos puede enviar cada proveedor" y refleja que un producto puede adquirirse de varios proveedores. Es el detalle que usaría quien debe elegir a quién hacer un pedido.
- **Tablas y cómo:** `productosofertados`, `producto`, `categoria` y `usuario`. Una fila por oferta con proveedor, empresa, ciudad, producto, categoría, precio y tiempo de entrega, ordenadas por proveedor.
- **Filtros:** proveedor (id), ciudad, empresa y nombre del producto (coincidencia parcial).
- **Fuente:** `ConsultarCatalogoOfertas`, ordenada por precio para la consulta y reordenada por proveedor en el reporte.

**Reporte 3: Productos y su mejor proveedor** (`GenerarReporteMejorProveedorPorProducto`)
- **Finalidad:** el objetivo del sistema es "determinar los proveedores a los que se pueden realizar los pedidos". Este reporte indica, para cada producto, el proveedor que lo ofrece al menor precio.
- **Tablas y cómo:** las mismas cuatro del reporte 2, más una subconsulta que calcula el precio mínimo de cada producto. Si dos proveedores empatan, salen ambos.
- **Filtros:** nombre del producto (coincidencia parcial) y ciudad. Con ciudad, el usuario elige cómo se interpreta (parámetro `soloProveedoresDeLaCiudad`, `true` por defecto):
  - **Mejor dentro de la ciudad** (`true`): la mejor oferta se calcula entre los proveedores de esa ciudad, es decir, "el más barato de Pereira" para cada producto que se ofrezca allí.
  - **Mejor global** (`false`): la mejor oferta se calcula entre todos los proveedores y la ciudad solo filtra el resultado, es decir, "los productos cuyo proveedor más barato es de Pereira". Puede no salir ningún producto.
  - Sin ciudad, las dos formas dan lo mismo. El PDF indica en el encabezado cuál se usó.

**Reporte 4: Productos por categoría** (`GenerarReporteProductosPorCategoria`)
- **Finalidad:** el enunciado dice que las categorías identifican las "familias de productos". Este reporte lista los productos agrupados por su familia.
- **Tablas y cómo:** `producto` y `categoria`. Una fila por producto con su categoría y su descripción. Es la única consulta nueva (`ConsultarProductosPorCategoria`), porque los DAO no tenían un listado de productos. También se expone como consulta en `ConsultaService`.
- **Filtros:** categoría y nombre del producto.

**Reporte 5: Resumen por categoría** (`GenerarReporteResumenPorCategoria`)
- **Finalidad:** el enunciado también pide almacenar los datos básicos de las categorías. Este reporte da una vista de cada familia: cuántos productos tiene, cuántas ofertas y en qué rango de precios.
- **Tablas y cómo:** `categoria`, `producto` y `productosofertados`, con `LEFT JOIN`: aparecen todas las categorías, aunque no tengan productos u ofertas.
- **Filtros:** categoría.

#### Reporte administrativo: bitácora de acceso

Además de los cinco reportes que pide el enunciado, hay un sexto que **solo puede generar un usuario con rol Administrador**: `GenerarReporteBitacora(idSolicitante, idUsuario?, desde?, hasta?)`.

- **Finalidad:** el enunciado pide una bitácora con la entrada y la salida de los usuarios al sistema. Este reporte la entrega en PDF para que el Administrador la revise o la archive: una fila por sesión con el usuario, la fecha y hora de entrada, la de salida y la duración. Las sesiones sin salida se muestran como "No registrada" y las de usuarios eliminados como "(usuario eliminado)".
- **Permiso:** lo primero que hace el método es llamar a `AutorizacionService.ValidarRol(idSolicitante, RolUsuario.Administrador)`. Si el solicitante es Proveedor, Vendedor o no existe, lanza `UnauthorizedAccessException` y **no consulta la bitácora ni genera nada**; esa validación va antes que la de los filtros. Es el mismo permiso que ya tiene `BitacoraService`, porque es información de auditoría.
- **Datos:** `BitacoraDAO.Consultar` (LEFT JOIN de `bitacora` y `usuario`), de la sesión más reciente a la más antigua. No tiene SQL propio: sigue la regla de una consulta, una fuente.
- **Filtros (opcionales):** usuario (id) y rango de fechas sobre la fecha de entrada. Las fechas llevan hora: para incluir un día completo, `hasta` debe ser el final de ese día.
- **Requisito:** la tabla `bitacora` debe existir en la base; está definida en [BDproyecto.sql](BDproyecto.sql) (ver [Bitácora de entrada y salida](#bitácora-de-entrada-y-salida)).
- **Constructor:** `ReporteService` recibe un `IConsultaDAO`, un `GeneradorPdf`, un `AutorizacionService` y un `IBitacoraDAO`.

### `ConsultaService`
Consultas de solo lectura que combinan varias tablas. Las puede ejecutar cualquier usuario, sin validación de rol; el DAO es `ConsultaDAO`.

| Método | Tipo | Tablas | Qué devuelve |
| --- | --- | --- | --- |
| `ConsultarCatalogoOfertas` | JOIN | `productosofertados`, `producto`, `categoria`, `usuario` | `OfertaDetalle`: oferta con su producto, categoría y proveedor. Filtros opcionales: categoría, ciudad y rango de precio |
| `ConsultarResumenPorCategoria` | JOIN | `categoria`, `producto`, `productosofertados` | `ResumenCategoria`: productos, ofertas y precio mínimo, máximo y promedio por categoría (incluye las categorías sin ofertas) |
| `ConsultarPortafolioProveedores` | JOIN | `usuario`, `productosofertados`, `producto` | `PortafolioProveedor`: productos y categorías que ofrece cada proveedor, con precio y entrega promedio (incluye a los que no ofrecen nada) |
| `ConsultarProductosPorCategoria` | JOIN | `producto`, `categoria` | `ProductoCategoria`: productos con el nombre de su categoría. Filtros opcionales: categoría y nombre del producto |
| `ConsultarMejorOfertaPorProducto` | Subconsulta | `productosofertados`, `producto`, `categoria`, `usuario` | `OfertaDetalle`: la oferta de menor precio de cada producto (todas, si hay empate) |
| `ConsultarProductosSinOferta` | Subconsulta (`NOT EXISTS`) | `producto`, `productosofertados` | `Producto`: los que ningún proveedor ofrece, que son los únicos que se pueden eliminar |

## Acceso a datos

| DAO | Operaciones |
| --- | --- |
| `UsuarioDAO` | Consultar por id, correo, cédula, rol, tipo, ciudad y empresa; crear, actualizar, eliminar, comprobar existencia; leer y actualizar el hash de contraseña |
| `ProductoDAO` | Crear, consultar por id, actualizar, eliminar y comprobar existencia |
| `ProductoOfertadoDAO` | Crear (valida proveedor y producto en una transacción), actualizar precio, consultar por proveedor o por producto, comprobar existencia y eliminar |
| `BitacoraDAO` | Registrar entrada, registrar salida y consultar la bitácora con filtros opcionales |
| `ConsultaDAO` | Las seis consultas de `ConsultaService` (cuatro con JOIN y dos con subconsulta), y es la fuente de los reportes (reutilizan sus consultas con filtros opcionales); no pertenecen a una sola entidad, por eso están separadas |

Detalles de implementación:
- **Categorías:** `categoria.Cacodigo` empieza en 1 y el enum `CategoriaProducto` en 0, por lo que `ProductoDAO` suma 1 al guardar y resta 1 al leer.
- **Proveedores:** son filas de `usuario` con `rolusuario = 2`; `nombreempresa` y `ciudad` solo se usan con ese rol.
- **Ids generados:** `UsuarioDAO` y `ProductoDAO` tienen una sobrecarga `Crear(..., out int idGenerado)` que ignora el id del objeto, inserta sin esa columna y devuelve el `AUTO_INCREMENT` de MySQL.
- **Productos nuevos:** `Producto` exige un id mayor que cero en su constructor, pero `ProductoDAO.Crear` no envía ese id a la base; el id real lo asigna `AUTO_INCREMENT`.
- **Errores de MySQL:** solo `UsuarioDAO`, `BitacoraDAO` y `ConsultaDAO` los envuelven en `ErrorDeBaseDeDatos`; `ProductoDAO` y `ProductoOfertadoDAO` no lo hacen.
- **Excepciones:** `Data/exception/` (acceso a datos) y `Service/exception/` (reglas de negocio, por ejemplo correo o cédula duplicados).

### Conexión

`Conexion` crea las conexiones y expone `ProbarAsync()` para comprobarla. La cadena se arma así:

1. `MYSQL_CONNECTION_STRING`, si está definida.
2. Si no, las variables `MYSQL_HOST` (`localhost`), `MYSQL_PORT` (opcional), `MYSQL_DATABASE` (`proyecto`), `MYSQL_USER` (`root`) y `MYSQL_PASSWORD` (vacía).

Usa `SslMode=Preferred` y un tiempo de espera de 5 segundos. La cadena se lee una sola vez al primer uso. No incluyas contraseñas reales en el repositorio.

## Base de datos

[BDproyecto.sql](BDproyecto.sql) es un volcado de MySQL (`mysqldump`) de la base `proyecto` (MySQL 8.0 o superior, por su collation `utf8mb4_0900_ai_ci`). **No crea ni selecciona la base**: hay que importarlo en una base `proyecto` ya existente. Desactiva las comprobaciones de claves foráneas al inicio (`FOREIGN_KEY_CHECKS=0`) y luego crea estas tablas:

| Tabla | Contenido |
| --- | --- |
| `bitacora` | `idbitacora`, `idusuario`, `fechaentrada`, `fechasalida` (sin datos iniciales) |
| `categoria` | `Cacodigo` y `Catipo` (10 categorías iniciales) |
| `producto` | `Prid` (autoincremental), `nombre`, `descripcion` y `categoria` (20 productos iniciales) |
| `productosofertados` | `proveedorid`, `producto`, `precio` y `tiempoentrega` (sin datos iniciales; sin llave primaria) |
| `rolusuario` | Roles: 1 Administrador, 2 proveedor, 3 vendedor |
| `usuario` | `idusuario` autoincremental, datos personales, `rolusuario`, `nombreempresa` y `ciudad`. Solo `rolusuario` es obligatorio; las demás columnas admiten `NULL`. No tiene columna `contrasena` ni restricciones `UNIQUE` |

> El script **borra y recrea** las tablas (`DROP TABLE IF EXISTS`), así que no lo ejecutes sobre datos que quieras conservar.

> **El script no coincide con los DAO en dos nombres.** `ConsultaDAO` lee `categoria.nombre`, pero la tabla define `categoria.Catipo`; y `UsuarioDAO` lee y escribe `usuario.contrasena`, pero esa columna no existe en la tabla. Contra este script, las consultas que unen `categoria` y el login fallan con `ErrorDeBaseDeDatos`. El resto de la documentación describe el código tal como está.

### Bitácora de entrada y salida

Cada sesión es una fila de la tabla `bitacora`: se crea al iniciar sesión y se completa con `Logout`.

| Columna | Descripción |
| --- | --- |
| `idbitacora` | Clave primaria autoincremental |
| `idusuario` | Usuario que entró; pasa a `NULL` si el usuario se elimina, para conservar el historial |
| `fechaentrada` | Fecha y hora del inicio de sesión |
| `fechasalida` | Fecha y hora del cierre de sesión; `NULL` mientras la sesión sigue abierta |

Si el programa se cierra sin llamar a `Logout`, `fechasalida` queda en `NULL` y la sesión se muestra como "salida no registrada" (`RegistroBitacora.SalidaRegistrada`). Hoy nada llama a `Logout` automáticamente: solo se cierra la sesión cuando el código lo invoca.

La tabla ya está en `BDproyecto.sql`; su clave foránea `idusuario` referencia a `usuario` con `ON DELETE SET NULL`, para que el historial sobreviva al borrado del usuario.

### Usuarios para pruebas

`BDproyecto.sql` inserta tres usuarios, uno por rol, con estos datos:

| Id | Nombre | Rol | Correo | Cédula | Empresa / ciudad |
| --- | --- | --- | --- | --- | --- |
| 1 | `admin` | Administrador | `s@email.com` | `12345` | `cocacola` / `circasia` |
| 2 | `porvee` | Proveedor | `d@email.com` | `2345` | `exito` / `armenia` |
| 3 | `vendedor` | Vendedor | `v@email.com` | `3456` | `D1` / `armenia` |

> **No sirven para iniciar sesión con este script:** la tabla `usuario` no tiene la columna `contrasena` y los `INSERT` no incluyen ningún hash (ver [Base de datos](#base-de-datos)). El login falla al leer esa columna. Los tres ids sí sirven para probar los permisos de los servicios, porque `idSolicitante` se pasa directamente y `AutorizacionService` consulta el rol en la base.

Qué se puede probar pasando el id de cada uno como `idSolicitante`:

| Usuario | Pruebas útiles |
| --- | --- |
| Administrador (id 1) | Crear, actualizar y eliminar usuarios; crear y eliminar productos. No puede crear otro administrador ni modificar o eliminar al existente |
| Proveedor (id 2) | Crear y eliminar productos; crear, consultar, actualizar el precio y eliminar sus productos ofertados. No puede crear usuarios |
| Vendedor (id 3) | Solo consultas. Las operaciones de escritura deben fallar con `UnauthorizedAccessException` |

Ejemplo de validación de permisos desde código:

```csharp
var autorizacion = new AutorizacionService(new UsuarioDAO());
autorizacion.ValidarRol(1, RolUsuario.Administrador); // pasa
autorizacion.ValidarRol(3, RolUsuario.Administrador); // UnauthorizedAccessException
```

Notas:

- El administrador y el vendedor tienen valores en `nombreempresa` y `ciudad` dentro de la tabla, pero el sistema los ignora porque solo se usan con el rol Proveedor.
- Volver a ejecutar `BDproyecto.sql` borra y recrea las tablas, así que los usuarios que se hayan creado después se pierden y estos tres vuelven a su estado inicial.

## Requisitos

- .NET SDK 10.
- MySQL Server 8.0 o superior.
- Paquetes NuGet `MySql.Data` 9.4.0, `BCrypt.Net-Next` 4.2.0 y `QuestPDF` 2026.9.1, restaurados automáticamente.

## Cómo ejecutar

1. **Preparar la base de datos.** El script no crea ni selecciona la base, así que debe existir `proyecto` antes de importarlo (pide la contraseña de MySQL):

   ```bash
   mysql -u tu_usuario -p -e "CREATE DATABASE IF NOT EXISTS proyecto DEFAULT CHARACTER SET utf8mb4;"
   mysql -u tu_usuario -p proyecto < BDproyecto.sql
   ```

   Ten en cuenta que el script actual no define `usuario.contrasena` ni `categoria.nombre` (ver [Base de datos](#base-de-datos)).

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

   Sin argumentos imprime `Hello, World!` y la ayuda del modo de demostración. No hay menú ni flujo interactivo: para usar los servicios hay que crear los DAO y servicios desde código, por ejemplo:

   ```csharp
   var usuarioDao = new UsuarioDAO();
   var autorizacion = new AutorizacionService(usuarioDao);
   var usuarioService = new UsuarioService(autorizacion, usuarioDao);
   Usuario admin = usuarioService.ObtenerUsuarioPorId(1);
   ```

El modo de demostración sí comprueba la conexión con `Conexion.ProbarAsync()` antes de empezar; en el resto del código, un error de configuración solo aparece cuando un DAO abre su primera conexión.

### Modo de demostración

Mientras no exista la interfaz gráfica, `Program.cs` permite ver el backend funcionando. Desde la carpeta `ProyectoBD`:

```bash
dotnet run -- consultas            # imprime en la consola las consultas con JOIN y con subconsultas
dotnet run -- reportes [carpeta]   # genera los reportes en PDF (por defecto en la carpeta temporal del sistema)
```

- **`consultas`** imprime las cinco consultas que pide el enunciado (tres con JOIN y dos con subconsulta) más el listado de productos por categoría, cada una con las tablas que usa y su número de filas.
- **`reportes`** genera 12 PDF: los cinco reportes, varios de ellos con y sin filtros, para comprobar que los filtros cambian el resultado, y el reporte de bitácora pedido como Administrador (usuario 1 del script). Además comprueba que el Vendedor del script (usuario 3) sea rechazado al pedir ese reporte. Si un reporte falla (por ejemplo, por una tabla o una columna faltante), lo indica, sigue con los demás y termina con código 1. Imprime la carpeta y el nombre de cada archivo; ábrelos con el visor de PDF del sistema (por ejemplo `xdg-open <carpeta>`).
- Si la conexión falla o faltan las tablas, muestran un mensaje claro y terminan con código 1.
- **Datos de ejemplo:** `BDproyecto.sql` no trae ofertas, así que sin cargarlas la mayoría de las consultas y reportes salen vacíos (el programa avisa). Para probar, inserta ofertas en tu base local, sin modificar el `.sql`. La lógica de este modo está en [Demostracion.cs](ProyectoBD/Backend/src/Demo/Demostracion.cs); no tiene lógica de negocio y se puede borrar cuando exista la interfaz.

## Pruebas

El proyecto **no tiene pruebas automatizadas**: el equipo retiró la carpeta de pruebas porque su implementación resultó complicada, y se podrá retomar más adelante. `ProyectoBD.csproj` sigue excluyendo `Backend/tests/**` de la compilación, por si se vuelve a crear.

## Pendiente respecto al enunciado

Funcionalidades que el enunciado pide y todavía no están (o no están completas):

1. **Sesión.** `Login` devuelve el usuario y `Logout` cierra la sesión en la bitácora, pero no hay un objeto de sesión; la interfaz deberá recordar al usuario autenticado y llamar a `Logout` al salir.
2. **Gestión de categorías.** Existe la tabla y el enum, pero no hay modelo, DAO ni servicio para crear, consultar, actualizar o eliminar categorías y sus datos básicos.
3. **Actualizar productos.** `ProductoService` no tiene operación de actualización; `ProductoDAO.Actualizar` existe pero ningún servicio la expone.
4. **Más búsquedas para el usuario esporádico.** `ConsultaService` ya filtra ofertas por categoría, ciudad y rango de precio, y `ConsultarProductosPorCategoria` busca productos por categoría y por nombre. Faltaría, a lo sumo, exponer los filtros nuevos del catálogo (proveedor, empresa y nombre del producto), que hoy solo usa el reporte 2.
5. **Proveedores de cualquier ciudad de Colombia.** La ciudad es texto libre, sin validación ni catálogo de ciudades.
6. **Un solo administrador a nivel de base de datos.** Hoy solo lo impide el servicio; la tabla no lo garantiza.
7. **`productosofertados` sin llave primaria** `(proveedorid, producto)`: la base permite ofertas repetidas.
8. **Interfaz de usuario.** Menú de consola o API web con login, y un `Program.cs` que arme los servicios.
9. **Pruebas automatizadas** (descartadas por ahora; ver la sección anterior).
10. **Mostrar los resultados de las consultas en la interfaz.** El modo de demostración las imprime en consola, pero no hay pantalla de la aplicación que las muestre. Además, `productosofertados` no trae datos iniciales, así que las consultas devuelven vacío hasta que se carguen ofertas.
11. **Mostrar los reportes en la interfaz.** Los cinco reportes están generados y verificados, y el modo de demostración los guarda como PDF, pero `ReporteService` solo devuelve los bytes: falta la interfaz que los guarde o los muestre, con los filtros que elija el usuario.

Mejoras técnicas que se dejaron anotadas: longitudes de columna cortas sin validar en el modelo (`correo` 25, `telefono` 10, `nombreempresa` 20), manejo de errores de MySQL únicamente en `UsuarioDAO`, `BitacoraDAO` y `ConsultaDAO`, y renombrar `ProdcutoOfertadoService` y `AutentificacionService` para que coincidan con sus archivos.
