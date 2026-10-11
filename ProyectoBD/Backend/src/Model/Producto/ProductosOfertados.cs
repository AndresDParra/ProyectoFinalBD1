/// <summary>
/// esta clase busca ser la intermediaria entre el proveedor y el producto,
/// esto se hace ya que es una relacion muchos a muchos donde el varios productos puede ser ofrecidos por varios proveedores,
///  y un proveedor puede ofrecer varios productos con el precio que este considere, 
/// de ahi el porque el precio es definido en esta clase y no en la clase producto.
/// </summary>


public class ProductosOfertados
{
    public ProductosOfertados(
        int proveedorId, // foreign key del proveedor en la base de datos
        Producto producto, // en la BD se usara el idProducto como foreign key
        decimal precio // representa el precio del producto ofrecido por el proveedor
    )
    {
        ProveedorId = proveedorId;
        Producto = producto;
        Precio = precio;
    }

    public int ProveedorId { get; private set; }
    public Producto Producto { get; private set; }
    public decimal Precio { get; private set; }
}