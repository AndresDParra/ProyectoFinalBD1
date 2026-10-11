# Proyecto Final — Bases de Datos I

## Ejecusion del metodo main en C# 
primero se abre una terminal desde vs code
luego se escribe: cd ProyectoBD
finalmente se ejecuta: dotnet run
para verificar que todo funcione debe mostrar "hello world" en la terminal

## Información académica

Este repositorio corresponde al proyecto final de la asignatura **Bases de Datos I** de la **Universidad del Quindío**, perteneciente al programa de Ingeniería de Sistemas y Computación.

### Integrantes

- **Mateo Gómez Marulanda**
- **Andres David Parra**
- **Juan David Castillo Hernandez**

## Descripción general

El proyecto contiene el modelo inicial de una aplicación de escritorio orientada a la gestión de usuarios, proveedores, vendedores y productos. La implementación está desarrollada en C# sobre .NET y organiza las clases del dominio en la carpeta `Prueba/model`.

El modelo representa principalmente:

- La información común de los usuarios del sistema.
- Los diferentes tipos de usuario.
- Los productos disponibles.
- Las categorías de productos.
- La relación entre proveedores y productos ofrecidos.
- La relación inicial entre vendedores y productos destinados a la venta.

La carpeta `Prueba/repository` está destinada a contener los repositorios que conectarán las clases del modelo con una base de datos MySQL. En el estado actual del proyecto, esta capa únicamente cuenta con un archivo descriptivo y todavía no contiene clases de acceso a datos.

## Estructura del proyecto

```text
ProyectoFinalBD1/
├── Prueba/
│   ├── model/
│   │   ├── Usuario.cs
│   │   ├── Cliente.cs
│   │   ├── Proveedor.cs
│   │   ├── Vendedor.cs
│   │   ├── Administrador.cs
│   │   └── Producto/
│   │       ├── Producto.cs
│   │       ├── CategoriaProducto.cs
│   │       └── ProductosOfertados.cs
│   ├── repository/
│   │   └── repository.txt
│   ├── ProgramTest.cs
│   └── VerificacionCSharp.csproj
├── ProyectoFinal_2026_II.md
└── README.md
```

### Carpeta `model`

Contiene las clases que representan los objetos principales del dominio de la aplicación. Estas clases definen atributos, constructores, relaciones y algunas reglas básicas de comportamiento.

### Carpeta `model/Producto`

Agrupa las clases relacionadas con los productos, sus categorías y las ofertas realizadas por los proveedores.

### Carpeta `repository`

Está reservada para la capa de persistencia. Su objetivo es definir la forma en que los objetos del modelo se almacenarán, consultarán, actualizarán y eliminarán en MySQL.

### Proyecto C#

El archivo `VerificacionCSharp.csproj` configura una aplicación ejecutable con:

- SDK de Microsoft.
- .NET 10 como framework objetivo.
- `ImplicitUsings` habilitado.
- Nullable Reference Types habilitado.

## Modelo de clases

### Clase `Usuario`

Archivo: `Prueba/model/Usuario.cs`

`Usuario` es una clase abstracta que concentra la información común a los diferentes tipos de usuario del sistema. Al ser abstracta, no se puede instanciar directamente; sirve como clase base para las clases especializadas.

Sus atributos son:

- `Nombre`
- `Direccion`
- `Telefono`
- `Cedula`
- `Correo`
- `FechaNacimiento`
- `Genero`

El constructor tiene acceso `protected`, por lo que únicamente puede ser utilizado por las clases que heredan de `Usuario`. Sus propiedades permiten la lectura pública y restringen la modificación mediante `protected set`.

En consecuencia, `Usuario` funciona como la base común de la jerarquía de usuarios y evita repetir estos mismos atributos en cada clase derivada.

### Clase `Cliente`

Archivo: `Prueba/model/Cliente.cs`

`Cliente` hereda de `Usuario` y agrega el identificador:

```csharp
IdCliente
```

El constructor recibe el identificador del cliente junto con los datos generales heredados. Actualmente la clase representa los datos básicos del cliente, pero todavía no contiene métodos para compras, carritos, pedidos o consultas de productos.

### Clase `Proveedor`

Archivo: `Prueba/model/Proveedor.cs`

`Proveedor` hereda de `Usuario` y representa a un proveedor de productos. Además de los datos generales, contiene:

- `IdProveedor`
- `NombreEmpresa`
- `Ciudad`
- Una colección privada de objetos `ProductosOfertados`

La colección privada representa los productos que el proveedor ofrece y el precio asignado a cada uno.

#### Método `ofrecerProducto`

El método `ofrecerProducto(Producto producto, decimal precio)` permite registrar un producto ofrecido por el proveedor.

Antes de agregar la oferta, aplica las siguientes validaciones:

1. El producto no puede ser nulo.
2. El precio debe ser mayor que cero.
3. El proveedor no puede ofrecer dos veces el mismo producto.

Cuando las validaciones se cumplen, se crea un objeto `ProductosOfertados` con el identificador del proveedor, el producto y el precio definido.

#### Método `eliminarProductoOfertado`

El método `eliminarProductoOfertado(int idProducto)` elimina de la colección privada las ofertas cuyo producto tenga el identificador indicado. La operación solamente se ejecuta cuando el identificador es positivo.

### Clase `Vendedor`

Archivo: `Prueba/model/Vendedor.cs`

`Vendedor` hereda de `Usuario` y agrega:

- `IdVendedor`
- `productosVenta`

`productosVenta` es una lista de objetos `Producto` que representa los productos que el vendedor tiene destinados a la venta.

La relación entre vendedor y producto se encuentra en una etapa inicial. Actualmente no se almacenan datos adicionales de esa relación, como inventario, precio de venta, cantidad disponible o proveedor seleccionado.

### Clase `Adminsitrador`

Archivo: `Prueba/model/Administrador.cs`

La clase representa al administrador del sistema y hereda de `Usuario`. Agrega el atributo:

```csharp
IdAdministrador
```

De acuerdo con la responsabilidad descrita en el código, el administrador corresponde al rol con mayor nivel de gestión dentro del sistema. Sin embargo, en el estado actual la clase solamente contiene los datos heredados, el identificador y el constructor; todavía no incluye métodos para administrar usuarios, productos o reportes.

> **Nota:** el nombre actual de la clase está escrito como `Adminsitrador`. La documentación conserva el nombre existente en el código.

### Clase `Producto`

Archivo: `Prueba/model/Producto/Producto.cs`

`Producto` representa una unidad que puede ser ofrecida por un proveedor y destinada a la venta por un vendedor.

Sus atributos son:

- `IdProducto`
- `Nombre`
- `Descripcion`
- `Categoria`

El atributo `Categoria` utiliza el tipo enumerado `CategoriaProducto`. El precio no se encuentra en esta clase, porque el diseño actual permite que un mismo producto sea ofrecido por diferentes proveedores con precios distintos.

### Enumeración `CategoriaProducto`

Archivo: `Prueba/model/Producto/CategoriaProducto.cs`

`CategoriaProducto` define las categorías disponibles para clasificar los productos:

- `Electronica`
- `Ropa`
- `Hogar`
- `Deportes`
- `Juguetes`
- `Belleza`
- `Alimentos`
- `Libros`
- `Musica`
- `Peliculas`

Actualmente las categorías están implementadas como un `enum` de C#. Esto significa que se manejan como valores fijos del código y no como una entidad independiente con atributos propios.

### Clase `ProductosOfertados`

Archivo: `Prueba/model/Producto/ProductosOfertados.cs`

`ProductosOfertados` actúa como clase intermedia entre `Proveedor` y `Producto`.

La clase es necesaria porque la relación entre proveedores y productos es de muchos a muchos:

- Un proveedor puede ofrecer varios productos.
- Un producto puede ser ofrecido por varios proveedores.
- Cada proveedor puede establecer un precio diferente para el mismo producto.

Sus atributos son:

- `ProveedorId`
- `Producto`
- `Precio`

En una futura representación relacional, esta clase correspondería conceptualmente a una tabla intermedia que tendría, como mínimo, los identificadores del proveedor y del producto, además del precio de la oferta.

## Relaciones entre las clases

### Herencia de usuarios

La jerarquía de herencia se representa de la siguiente manera:

```text
                         Usuario
                    (clase abstracta)
                              |
          ---------------------------------------------
          |                    |          |            |
       Cliente             Proveedor   Vendedor   Adminsitrador
```

Todas las clases derivadas reutilizan los atributos generales definidos en `Usuario` y agregan sus propios identificadores o comportamientos.

### Relación entre `Proveedor`, `ProductosOfertados` y `Producto`

La relación se representa así:

```text
Proveedor 1 ─────── N ProductosOfertados N ─────── 1 Producto
```

Conceptualmente, esto representa una relación de muchos a muchos entre proveedores y productos:

```text
Proveedor N ───────────────── N Producto
```

`ProductosOfertados` permite resolver esta relación y almacenar el precio específico que cada proveedor asigna a cada producto.

El flujo principal implementado es:

```text
Proveedor
   └── productosOfertados
          └── ProductosOfertados
                 └── Producto
```

### Relación entre `Vendedor` y `Producto`

La clase `Vendedor` mantiene una lista de productos:

```text
Vendedor 1 ───────────────── N Producto
```

Esta relación representa los productos que el vendedor tiene disponibles para vender. En el código actual todavía no existe una clase intermedia para almacenar información específica de la relación, como cantidades, precios de venta o inventario.

### Relación entre `Producto` y `CategoriaProducto`

Cada objeto `Producto` contiene un valor de `CategoriaProducto`:

```text
Producto N ───────────────── 1 CategoriaProducto
```

La categoría se encuentra implementada como enumeración y no como una clase independiente.

## Capa `repository`

Archivo actual: `Prueba/repository/repository.txt`

La carpeta `repository` está destinada a contener los repositorios correspondientes a las clases que serán almacenadas en la base de datos. Estos repositorios deberán servir como enlace entre:

```text
Objetos del modelo C#  ↔  Base de datos MySQL
```

En el estado actual del proyecto todavía no existen clases de repositorio ni operaciones de persistencia implementadas. Por lo tanto, aún no se encuentran definidos:

- La conexión con MySQL.
- Las cadenas de conexión.
- Los comandos SQL.
- Las operaciones CRUD.
- El mapeo entre registros de la base de datos y objetos C#.
- Las consultas de proveedores y productos.
- El almacenamiento de las ofertas.
- Las transacciones de base de datos.

La estructura futura de esta capa deberá permitir que las clases del modelo permanezcan separadas de los detalles de acceso a datos. De esta forma, `model` representará las entidades y `repository` se encargará de comunicarse con MySQL.

## Resumen del estado actual

Actualmente se encuentran implementados:

- La clase base abstracta `Usuario`.
- Las clases derivadas `Cliente`, `Proveedor`, `Vendedor` y `Adminsitrador`.
- La clase `Producto`.
- La enumeración `CategoriaProducto`.
- La clase intermedia `ProductosOfertados`.
- La asociación entre proveedores y productos mediante ofertas.
- Las validaciones básicas al registrar una oferta.
- La colección inicial de productos asociados a un vendedor.

La parte de persistencia todavía se encuentra pendiente. En consecuencia, las relaciones descritas en este documento están modeladas actualmente como objetos y colecciones en memoria; aún no están conectadas con tablas reales de MySQL.