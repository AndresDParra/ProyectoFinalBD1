/// <summary>
/// Una oferta junto con los datos de su producto, su categoría y su proveedor. Solo transporta datos, no valida nada.
///
/// La devuelven dos consultas de <see cref="ConsultaDAO"/>:
///
/// - ConsultarCatalogoOfertas (JOIN de productosofertados, producto, categoria y usuario): sirve para que un usuario
///   vea qué proveedor ofrece cada producto, a qué precio y en cuántos días, y pueda elegir. Sus filtros (categoría,
///   ciudad, precio mínimo y máximo, proveedor, empresa y nombre del producto) son opcionales: un filtro vacío no se
///   aplica y los demás se combinan con AND. Sale ordenada de menor a mayor precio. Si nada coincide devuelve una
///   lista vacía, nunca null. Es también la fuente del reporte "Ofertas de los proveedores" (<see cref="ReporteService"/>).
///
/// - ConsultarMejorOfertaPorProducto (subconsulta con MIN(precio) por producto): responde a qué proveedor conviene
///   pedirle cada producto. Si dos proveedores empatan en el precio mínimo salen ambos, y los productos sin
///   ofertas no aparecen. Sus filtros opcionales son el nombre del producto y la ciudad. Con ciudad se puede elegir
///   si la mejor oferta se calcula solo entre los proveedores de esa ciudad o entre todos los proveedores (y la ciudad
///   solo filtra el resultado). Es la fuente del reporte "Productos y su mejor proveedor" (<see cref="ReporteService"/>).
/// </summary>
public record OfertaDetalle(
    int ProveedorId,
    string Proveedor,
    string? NombreEmpresa,
    string? Ciudad,
    int ProductoId,
    string Producto,
    string Categoria,
    decimal Precio,
    int TiempoEntrega);
