/// <summary>
/// Portafolio de un proveedor. Solo transporta datos, no valida nada.
///
/// La devuelve ConsultarPortafolioProveedores de <see cref="ConsultaDAO"/> (JOIN de usuario, productosofertados y
/// producto): sirve para ver la capacidad de cada proveedor, o sea cuántos productos y categorías distintas ofrece y
/// con qué precio y tiempo de entrega promedio. Solo incluye usuarios con rol Proveedor. Como usa LEFT JOIN, un
/// proveedor sin ofertas también aparece, con Productos y Categorias en 0 y los promedios en null. Sale ordenada
/// de más a menos productos ofrecidos. Sus filtros opcionales son la ciudad y la empresa. Es también la fuente del
/// reporte "Proveedores y su portafolio" (<see cref="ReporteService"/>).
/// </summary>
public record PortafolioProveedor(
    int ProveedorId,
    string Proveedor,
    string? NombreEmpresa,
    string? Ciudad,
    int Productos,
    int Categorias,
    decimal? PrecioPromedio,
    decimal? EntregaPromedio);
