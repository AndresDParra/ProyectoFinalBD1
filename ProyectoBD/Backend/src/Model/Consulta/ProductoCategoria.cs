/// <summary>
/// Un producto junto con el nombre de su categoría. Solo transporta datos, no valida nada.
///
/// La devuelve ConsultarProductosPorCategoria de <see cref="ConsultaDAO"/> (JOIN de producto y categoria): sirve
/// para ver los productos agrupados por su "familia" y es la fuente del reporte "Productos por categoría"
/// (<see cref="ReporteService"/>). Se devuelve el nombre de la categoría y no el enum <see cref="CategoriaProducto"/>
/// porque quien lee el listado quiere el texto, y así no depende de que el enum esté al día con la tabla.
/// Descripcion es null si el producto no la tiene. Sale ordenada por categoría y luego por nombre del producto.
/// </summary>
public record ProductoCategoria(
    int ProductoId,
    string Producto,
    string? Descripcion,
    string Categoria);
