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
        IdProducto = idProducto;
        Nombre = nombre;
        Descripcion = descripcion;
        Categoria = categoria;
    }

    public int IdProducto { get; private set; }
    public string Nombre { get; private set; }
    public string Descripcion { get; private set; }
    public CategoriaProducto Categoria { get; private set; }
}