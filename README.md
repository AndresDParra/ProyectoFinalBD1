# Proyecto Final - Bases de Datos I

Proyecto académico de la Universidad del Quindío para modelar una aplicación de gestión de usuarios, proveedores, vendedores y productos utilizando C#, .NET y MySQL.

## Integrantes

- Mateo Gómez Marulanda
- Andres David Parra
- Juan David Castillo Hernandez

## Estado actual

El proyecto se encuentra en una etapa de persistencia y servicios de aplicación. Actualmente contiene:

- Modelos de dominio para usuarios, administradores, proveedores, vendedores y productos.
- Un enum para clasificar productos en diez categorías.
- Interfaces DAO y sus implementaciones para acceder a MySQL con consultas parametrizadas.
- Operaciones CRUD para proveedores, vendedores y productos.
- Operaciones para crear y consultar administradores, incluyendo la regla de que solo puede existir un administrador inicial.
- Gestión de productos ofertados por proveedores, con precio por proveedor.
- Servicios que validan existencia, evitan duplicados y coordinan operaciones entre DAO.
- Una clase centralizada para construir conexiones MySQL y comprobar la conectividad.
- Un script SQL inicial para `categoria`, `producto` y `proveedor`.

La aplicación todavía es un ejecutable de consola. [Program.cs](ProyectoBD/Program.cs) únicamente imprime `Hello, World!`; por tanto, los servicios, DAO y la conexión no se ejecutan automáticamente desde un flujo de aplicación.

## Estructura actual

```text
ProyectoFinalBD1/
├── BDproyecto.sql                  # Esquema inicial de MySQL
├── README.md
├── He gawn                       
└── ProyectoBD/
    ├── Program.cs                 # Punto de entrada actual: aplicación de consola
    ├── ProyectoBD.csproj          # Proyecto .NET 10 y paquete MySql.Data
    └── Backend/
        └── src/
            ├── Model/             # Entidades, relaciones y reglas de dominio
            │   ├── Usuario.cs
            │   ├── Administrador.cs
            │   ├── Proveedor.cs
            │   ├── Vendedor.cs
            │   └── Producto/
            │       ├── Producto.cs
            │       ├── ProductoOfertado.cs
            │       └── CategoriaProducto.cs
            ├── Data/              # Conexión y acceso directo a MySQL
            │   ├── Conexion.cs
            │   ├── AdministradorDAO.cs
            │   ├── ProductoDAO.cs
            │   ├── ProductoOfertadoDAO.cs
            │   ├── ProveedorDAO.cs
            │   ├── VendedorDAO.cs
            │   └── DAOInterface/
            │       ├── IAdministradorDAO.cs
            │       ├── IProductoDAO.cs
            │       ├── IProductoOfertadoDAO.cs
            │       ├── IProveedorDAO.cs
            │       └── IVendedorDAO.cs
            └── Service/            # Casos de uso y validaciones sobre los DAO
                ├── AdministradorService.cs
                ├── ProductoService.cs
                ├── ProveedorService.cs
                └── VendedorService.cs
```

Las carpetas `bin/` y `obj/` son artefactos generados por .NET y no forman parte de la arquitectura de la aplicación.

## Modelo de dominio

### Usuario

`Usuario` es una clase abstracta con la información común de los usuarios:

- Nombre.
- Dirección.
- Teléfono.
- Cédula.
- Correo.
- Fecha de nacimiento.
- Género.

Las clases `Administrador`, `Proveedor` y `Vendedor` heredan de `Usuario` y agregan sus identificadores o datos específicos. No existe actualmente una clase `Cliente` en el código fuente.

### Administrador

`Administrador` contiene `IdAdministrador`. `AdministradorService` permite actualizarlo si existe y ofrece operaciones para crear, consultar, actualizar y eliminar vendedores y proveedores, además de consultar productos. La clase representa el rol con mayor nivel de gestión, pero no existe todavía un mecanismo de autenticación o autorización que obligue a usar este servicio únicamente desde una sesión de administrador.

### Proveedor

`Proveedor` contiene `IdProveedor`, `NombreEmpresa` y `Ciudad`, además de los datos comunes heredados de `Usuario`. La relación con los productos se persiste mediante `ProductoOfertadoDAO`; la clase de dominio no expone actualmente métodos públicos para modificar su colección de ofertas.

`ProveedorService` recibe el ID del proveedor autenticado al construirse y comprueba que exista. Después permite:

- Crear una oferta para un producto existente.
- Consultar todas las ofertas del proveedor autenticado.
- Consultar una oferta por ID de producto.
- Actualizar el precio de una oferta.
- Eliminar una oferta.

El precio actualizado debe ser mayor que cero. El servicio usa siempre el proveedor autenticado, por lo que sus métodos no reciben un proveedor arbitrario para modificar sus ofertas.

### Vendedor

`Vendedor` contiene `IdVendedor` y una lista `productosVenta`. El DAO permite crear, consultar, actualizar, eliminar y comprobar la existencia de vendedores. La lista de productos es únicamente una colección del modelo: no hay todavía persistencia de inventario, cantidades, precio de venta ni operaciones implementadas en `VendedorService`.

### Producto y categorías

`Producto` contiene:

- `IdProducto`.
- `Nombre`.
- `Descripcion`.
- `Categoria`.

`CategoriaProducto` es un `enum` con las categorías `Electronica`, `Ropa`, `Hogar`, `Deportes`, `Juguetes`, `Belleza`, `Alimentos`, `Libros`, `Musica` y `Peliculas`.

`ProductoOfertado` representa la relación entre un proveedor y un producto, junto con el precio que ofrece ese proveedor. El DAO consulta todas las ofertas de un proveedor, pero la consulta por producto devuelve solo una oferta (`LIMIT 1`) cuando existen varias. El modelo representa una relación muchos a muchos, aunque el esquema SQL incluido todavía no contiene la tabla puente `productoofertado`.

## Servicios de aplicación

Los servicios actuales son clases coordinadoras que reciben DAO por constructor y aplican validaciones antes de acceder a MySQL. Todavía no están registrados en un contenedor de inyección de dependencias ni son invocados por `Program.cs`.

### `AdministradorService`

- Actualiza un administrador existente.
- Crea, consulta, actualiza y elimina vendedores.
- Crea, consulta, actualiza y elimina proveedores.
- Consulta productos por ID.
- Devuelve `false` cuando una operación de creación, actualización o eliminación no puede continuar porque el registro no existe o ya existe.
- Lanza `KeyNotFoundException` al consultar un vendedor, proveedor o producto inexistente.

### `ProductoService`

- Crea productos y rechaza IDs duplicados con `InvalidOperationException`.
- Consulta productos por ID.
- Elimina un producto solo si existe y no está siendo ofertado por ningún proveedor.
- Lanza `KeyNotFoundException` cuando se consulta o elimina un producto inexistente.

### `ProveedorService`

Además de restringir las operaciones de creación, actualización y eliminación al proveedor recibido en el constructor, valida que el producto consultado exista antes de operar. La consulta por ID de producto delega en el DAO y puede devolver una oferta de cualquier proveedor, porque actualmente no incluye el ID del proveedor autenticado en su filtro. La creación de la oferta delega la validación de precio positivo al DAO al actualizar; actualmente la creación no aplica esa misma validación antes de insertar.

### `VendedorService`

El servicio define dependencias hacia los DAO de productos, ofertas y proveedores, pero sus métodos de consulta todavía no están implementados. Los comentarios internos describen consultas futuras por proveedor, producto, rango de precios, categoría, nombre y ciudad.

## Acceso a datos

Las interfaces de `Data/DAOInterface` definen los contratos de acceso a datos. Las implementaciones ejecutan SQL parametrizado usando `MySql.Data`.

| DAO | Operaciones disponibles | Observaciones |
| --- | --- | --- |
| `AdministradorDAO` | Crear, consultar por ID, actualizar y comprobar existencia | La creación abre una transacción serializable y solo permite insertar el primer registro de la tabla. La consulta y actualización todavía mapean únicamente el ID; la actualización no modifica los demás datos. |
| `ProveedorDAO` | Crear, consultar por ID, actualizar, eliminar y comprobar existencia | La creación intenta persistir datos de usuario, empresa y ciudad. La consulta devuelve el ID, el nombre y la ciudad; los demás datos de usuario se devuelven vacíos. |
| `VendedorDAO` | Crear, consultar por ID, actualizar, eliminar y comprobar existencia | La consulta devuelve únicamente el ID. La actualización actual asigna el mismo ID al mismo ID y no modifica los demás datos. |
| `ProductoDAO` | Crear, consultar por ID, actualizar, eliminar y comprobar existencia | Usa el enum `CategoriaProducto` convertido a entero. La creación y actualización no envían `Prproveedor`, aunque esa columna aparece como obligatoria en el SQL actual. |
| `ProductoOfertadoDAO` | Crear, actualizar precio, comprobar existencia por producto, consultar por proveedor, consultar por producto y eliminar | La consulta por proveedor devuelve una lista ordenada de ofertas. La consulta por producto devuelve una sola oferta total mediante `LIMIT 1`. |

Las interfaces `IAdministradorDAO`, `IProveedorDAO`, `IVendedorDAO`, `IProductoDAO` e `IProductoOfertadoDAO` definen estos contratos y permiten que los servicios dependan de abstracciones en lugar de implementar SQL directamente.

`Conexion` centraliza la creación de conexiones MySQL y expone `ProbarAsync(CancellationToken)` para comprobar la conectividad de forma asíncrona. Cada DAO solicita una nueva conexión con `Conexion.Obtener()`. La cadena de conexión se obtiene en este orden:

1. `MYSQL_CONNECTION_STRING`, si está definida.
2. Variables individuales de entorno:
   - `MYSQL_HOST` - valor predeterminado: `localhost`.
   - `MYSQL_PORT` - opcional; MySQL usa `3306` por defecto.
   - `MYSQL_DATABASE` - valor predeterminado: `proyecto`.
   - `MYSQL_USER` - valor predeterminado: `root`.
   - `MYSQL_PASSWORD` - valor predeterminado: cadena vacía.

La conexión usa `SslMode=Preferred` y un tiempo de espera de cinco segundos. No se deben incluir contraseñas reales en el repositorio. Para otros entornos se recomienda configurar estas variables fuera del código fuente.

## Base de datos

El archivo [BDproyecto.sql](BDproyecto.sql) es un volcado de MySQL 8.0.46 para la base de datos `proyecto`. Actualmente crea estas tablas:

### `categoria`

- `Cacodigo`: clave primaria entera.
- `Catipo`: nombre de la categoría.

### `proveedor`

- `Pcedula`: clave primaria y referencia utilizada por los productos.
- `Pnombre`: nombre del proveedor.
- `Ptelefono`: teléfono opcional.
- `Pciudad`: ciudad.

El script actual solo contiene estas cuatro columnas. Sin embargo, `ProveedorDAO.Crear` intenta insertar también `PnombreUsuario`, `Pdireccion`, `PcedulaUsuario`, `Pcorreo`, `PfechaNacimiento` y `Pgenero`, por lo que esa operación requiere ajustar el esquema o el DAO.

### `producto`

- `Prid`: clave primaria autoincremental.
- `Prnombre`: nombre.
- `Prdecripcion`: descripción; el nombre de la columna conserva un error tipográfico del esquema original.
- `Prproveedor`: proveedor obligatorio y clave foránea.
- `Prcategoria`: categoría obligatoria y clave foránea.

El script no crea actualmente las tablas `administrador`, `vendedor` ni `productoofertado`, aunque los DAO contienen SQL que las utiliza. Tampoco incluye datos iniciales para categorías. Además, `ProductoDAO` no envía el valor de `Prproveedor` al crear o actualizar un producto. Por eso, los DAO y servicios no pueden ejecutarse correctamente contra una base creada únicamente con este script sin completar primero el esquema y ajustar el mapeo.

## Requisitos

- .NET SDK 10.
- MySQL Server 8.0 o compatible.
- Una base de datos llamada `proyecto`.
- Paquete NuGet `MySql.Data` 9.4.0, restaurado automáticamente por el proyecto.

La versión objetivo y la dependencia principal están declaradas en [ProyectoBD.csproj](ProyectoBD/ProyectoBD.csproj).

## Configuración y ejecución

Desde la raíz del repositorio:

```bash
cd ProyectoBD
dotnet restore
dotnet build
dotnet run
```

La ejecución actual muestra:

```text
Hello, World!
```

El proyecto compila como aplicación de consola para .NET 10, pero no ofrece todavía un menú ni un flujo interactivo. Para usar los servicios de forma programática es necesario construir sus dependencias (`AdministradorDAO`, `ProveedorDAO`, `VendedorDAO`, `ProductoDAO` y `ProductoOfertadoDAO`) y llamar explícitamente a sus métodos.

Para configurar MySQL en Linux, por ejemplo:

```bash
export MYSQL_HOST=localhost
export MYSQL_PORT=3306
export MYSQL_DATABASE=proyecto
export MYSQL_USER=tu_usuario
export MYSQL_PASSWORD='tu contraseña'
```

Después se puede importar el esquema desde MySQL:

```bash
mysql -u tu_usuario -p proyecto < BDproyecto.sql
```

La aplicación actual no carga ni ejecuta automáticamente `BDproyecto.sql`, y tampoco llama a `Conexion.ProbarAsync`; esos pasos deben realizarse antes de probar los DAO o servicios directamente. La importación del script tampoco es suficiente para habilitar todas las operaciones, porque faltan tablas y columnas requeridas por el código.

## Arquitectura actual y evolución prevista

La arquitectura implementada actualmente está organizada en tres responsabilidades principales:

- `Model`: entidades y enum del dominio (`Usuario`, `Administrador`, `Proveedor`, `Vendedor`, `Producto` y `ProductoOfertado`).
- `Data`: contratos DAO, SQL parametrizado y construcción de conexiones MySQL.
- `Service`: casos de uso y validaciones que coordinan los DAO para administradores, proveedores y productos.

El punto de entrada todavía no conecta estas capas con una interfaz de usuario. La siguiente evolución prevista es exponer los servicios mediante una API web.

Para integrar un frontend web en el futuro, la aplicación debería evolucionar hacia una API ASP.NET Core con capas separadas:

```text
Frontend web (React, Vue, Angular u otro)
                    |
              HTTP / JSON
                    |
API / Controllers
                    |
Application / Services / DTOs
                    |
Domain / Entities
                    |
Infrastructure / Repositories / MySQL
```

Una estructura futura recomendada sería:

```text
Backend/src/
├── Api/
│   ├── Controllers/
│   └── Middleware/
├── Application/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Services/
├── Domain/
│   ├── Entities/
│   ├── Enums/
│   └── Exceptions/
└── Infrastructure/
    ├── Authentication/
    ├── Data/
    └── Reports/
```

Las responsabilidades previstas son:

- `Domain`: entidades y reglas propias del negocio; no debe contener SQL.
- `Infrastructure/Data`: repositorios o DAO y detalles de MySQL.
- `Application/Services`: casos de uso, validaciones y coordinación entre repositorios.
- `Api/Controllers`: entrada HTTP y respuestas para el frontend.
- `Infrastructure/Authentication`: hash de contraseñas, validación de credenciales y tokens.
- `Infrastructure/Reports`: generación de PDF, Excel o CSV a partir de datos obtenidos por servicios.

La autorización por rol debe estar en servicios y políticas de la API, no dentro de las entidades ni de los DAO. Las contraseñas nunca deben almacenarse en texto plano; deben guardarse como hashes utilizando una solución segura.

## Problemas conocidos y próximos pasos

Para considerar funcional la persistencia completa y habilitar un flujo de aplicación, es necesario resolver como mínimo:

1. Crear en `BDproyecto.sql` las tablas que utilizan `AdministradorDAO`, `VendedorDAO` y `ProductoOfertadoDAO`.
2. Definir si un producto tiene un único proveedor (`Prproveedor`) o varios proveedores mediante `productoofertado`; actualmente el modelo y el SQL son inconsistentes.
3. Corregir el alta de productos para suministrar un proveedor válido o modificar el esquema relacional.
4. Agregar los códigos iniciales de `categoria` y fijar su correspondencia con `CategoriaProducto`.
5. Completar el mapeo de los datos de `Usuario`, ya que los DAO actuales devuelven varios campos con valores vacíos.
6. Corregir las actualizaciones que actualmente no modifican información real.
7. Cambiar la consulta por producto de `ProductoOfertadoDAO` para devolver todas las ofertas cuando existan varios proveedores, si ese es el comportamiento requerido.
8. Validar en `ProveedorService.CrearProductoOfertado` que el precio inicial sea mayor que cero, igual que en la actualización.
9. Implementar las consultas de `VendedorService` por proveedor, producto, rango de precios, categoría, nombre y ciudad.
10. Completar la autorización por rol, las transacciones entre tablas relacionadas y las pruebas automatizadas.
