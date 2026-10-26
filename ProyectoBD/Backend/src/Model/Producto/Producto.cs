/// <summary>
/// representa la unidad ofrecida por un proveedor a un vendedor en el sistema, la cual puede ser adquirida por un cliente
/// los productos cuentan con diferentes categorias definidas en un enum
/// </summary>

public class Producto
{
    public Producto(
        int idProducto, // primary key del producto en la base de datos
        string nombre,
        string descripcion,
        CategoriaProducto categoria // por ahora es un enum
    )
    {
        if (idProducto <= 0)
        {
            throw new ArgumentException("El ID del producto debe ser un número positivo.", nameof(idProducto));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre del producto no puede estar vacío o ser nulo.");
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            throw new ArgumentException("La descripción del producto no puede estar vacía o ser nula.");
        }

        CategoriaProducto categoriaValida = Enum.IsDefined(typeof(CategoriaProducto), categoria)
            ? categoria
            : throw new ArgumentException("La categoría del producto no es válida.");

        IdProducto = idProducto;
        Nombre = nombre;
        Descripcion = descripcion;
        Categoria = categoria;
    }

    public int IdProducto { get; }
    public string Nombre { get; private set; }
    public string Descripcion { get; private set; }
    public CategoriaProducto Categoria { get; private set; }
}