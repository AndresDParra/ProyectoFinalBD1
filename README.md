# Proyecto Final - Bases de Datos I

Proyecto académico de la Universidad del Quindío para modelar una aplicación de gestión de usuarios, proveedores, vendedores y productos utilizando C#, .NET y MySQL.

## Integrantes

- Mateo Gómez Marulanda
- Andres David Parra
- Juan David Castillo Hernandez

## Estado actual

El repositorio se encuentra en una etapa inicial de persistencia. Actualmente contiene:

- Modelos de dominio para usuarios, proveedores, vendedores y productos.
- Reglas básicas en memoria para las ofertas de productos.
- Interfaces DAO para definir operaciones de acceso a datos.
- Implementaciones DAO con SQL parametrizado para MySQL.
- Una clase centralizada para construir y probar la conexión a la base de datos.
- Un script SQL inicial con las tablas `categoria`, `producto` y `proveedor`.

La aplicación todavía es un ejecutable de consola. [Program.cs](ProyectoBD/Program.cs) únicamente imprime `Hello, World!`; por tanto, los DAO no se ejecutan desde un flujo de aplicación y la conexión no se prueba automáticamente.

Todavía no están implementados:

- API HTTP o controladores.
- Inicio de sesión, almacenamiento seguro de contraseñas y emisión de tokens.
- Roles y autorización de operaciones.
- Servicios de aplicación que coordinen los DAO.
- Reportes.
- Interfaz gráfica o frontend web.
- Pruebas automatizadas.

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
            └── Data/               # Conexión y acceso directo a MySQL
                ├── Conexion.cs
                ├── AdministradorDAO.cs
                ├── ProductoDAO.cs
                ├── ProductoOfertadoDAO.cs
                ├── ProveedorDAO.cs
                ├── VendedorDAO.cs
                └── DAOInterface/
                    ├── IAdministradorDAO.cs
                    ├── IProductoDAO.cs
                    ├── IProductoOfertadoDAO.cs
                    ├── IProveedorDAO.cs
                    └── IVendedorDAO.cs
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

`Administrador` contiene `IdAdministrador`. El modelo lo considera el usuario con mayor nivel de gestión, pero esta autorización todavía no está implementada en servicios ni en una API.

### Proveedor

`Proveedor` contiene `IdProveedor`, `NombreEmpresa` y `Ciudad`. También mantiene una colección privada de `ProductoOfertado` y expone estas reglas en memoria:

- `OfrecerProducto(Producto, decimal)` exige un producto válido, un precio mayor que cero y evita duplicar el mismo producto para el proveedor.
- `EliminarProductoOfertado(int)` elimina una oferta por el identificador del producto.

Estas operaciones modifican la colección del objeto. La persistencia de la oferta se realiza por separado mediante `ProductoOfertadoDAO`.

### Vendedor

`Vendedor` contiene `IdVendedor` y una lista `productosVenta`. La relación entre vendedor, inventario, cantidades y precio de venta todavía no está persistida.

### Producto y categorías

`Producto` contiene:

- `IdProducto`.
- `Nombre`.
- `Descripcion`.
- `Categoria`.

`CategoriaProducto` es un `enum` con las categorías `Electronica`, `Ropa`, `Hogar`, `Deportes`, `Juguetes`, `Belleza`, `Alimentos`, `Libros`, `Musica` y `Peliculas`.

`ProductoOfertado` representa la relación entre un proveedor y un producto, junto con el precio que ofrece ese proveedor. Conceptualmente es una relación muchos a muchos, aunque el esquema SQL actual todavía no la representa de forma completa.

## Acceso a datos

Las interfaces de `Data/DAOInterface` definen los contratos de acceso a datos. Las implementaciones ejecutan SQL parametrizado usando `MySql.Data`.

| DAO | Operaciones disponibles | Observaciones |
| --- | --- | --- |
| `AdministradorDAO` | Crear, consultar por ID y actualizar | No implementa eliminar. Solo permite crear el primer administrador. |
| `ProveedorDAO` | Crear, consultar por ID, actualizar, eliminar y comprobar existencia | Solo persiste el identificador, el nombre de empresa y la ciudad. |
| `VendedorDAO` | Crear, consultar por ID, actualizar, eliminar y comprobar existencia | La actualización actual asigna el mismo ID al mismo ID y no modifica otros datos. |
| `ProductoDAO` | Crear, consultar por ID, actualizar, eliminar y comprobar existencia | La creación actual no envía `Prproveedor`, columna obligatoria del SQL. |
| `ProductoOfertadoDAO` | Crear, actualizar precio, consultar por proveedor y eliminar | La consulta devuelve solo la primera oferta del proveedor. |

`Conexion` centraliza la creación de conexiones MySQL y expone `ProbarAsync` para comprobar la conectividad. La cadena de conexión se obtiene en este orden:

1. `MYSQL_CONNECTION_STRING`, si está definida.
2. Variables individuales de entorno:
   - `MYSQL_HOST` - valor predeterminado: `localhost`.
   - `MYSQL_PORT` - opcional; MySQL usa `3306` por defecto.
   - `MYSQL_DATABASE` - valor predeterminado: `proyecto`.
   - `MYSQL_USER` - valor predeterminado: `root`.
   - `MYSQL_PASSWORD` - valor predeterminado: cadena vacía.

No se deben incluir contraseñas reales en el repositorio. Para otros entornos se recomienda configurar estas variables fuera del código fuente.

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

### `producto`

- `Prid`: clave primaria autoincremental.
- `Prnombre`: nombre.
- `Prdecripcion`: descripción; el nombre de la columna conserva un error tipográfico del esquema original.
- `Prproveedor`: proveedor obligatorio y clave foránea.
- `Prcategoria`: categoría obligatoria y clave foránea.

El script no crea actualmente las tablas `administrador`, `vendedor` ni `productoofertado`, aunque los DAO contienen SQL que las utiliza. Tampoco incluye datos iniciales para categorías. Por eso, los DAO relacionados no pueden ejecutarse correctamente contra una base creada únicamente con este script.

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

La aplicación actual no carga ni ejecuta automáticamente `BDproyecto.sql`, y tampoco llama a `Conexion.ProbarAsync`; esos pasos deben realizarse antes de probar los DAO directamente.

## Arquitectura prevista

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

## Problemas conocidos

Antes de considerar funcional la persistencia, es necesario resolver como mínimo:

1. Crear en `BDproyecto.sql` las tablas que utilizan `AdministradorDAO`, `VendedorDAO` y `ProductoOfertadoDAO`.
2. Definir si un producto tiene un único proveedor (`Prproveedor`) o varios proveedores mediante `productoofertado`; actualmente el modelo y el SQL son inconsistentes.
3. Corregir el alta de productos para suministrar un proveedor válido o modificar el esquema relacional.
4. Agregar los códigos iniciales de `categoria` y fijar su correspondencia con `CategoriaProducto`.
5. Completar el mapeo de los datos de `Usuario`, ya que los DAO actuales devuelven varios campos con valores vacíos.
6. Corregir las actualizaciones que actualmente no modifican información real.
7. Añadir consultas que devuelvan colecciones cuando un proveedor tenga varias ofertas.
8. Incorporar transacciones donde una operación modifique varias tablas relacionadas.