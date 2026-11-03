using System.Globalization;

/// <summary>
/// Genera los reportes en PDF. Cada reporte sigue los mismos pasos: valida los filtros que eligió el usuario,
/// pide los datos al DAO, los convierte a texto con el formato que se muestra en pantalla y se los pasa al
/// <see cref="GeneradorPdf"/>. Cualquier usuario puede generarlos, igual que las consultas, porque solo leen datos.
/// </summary>
public class ReporteService
{
    // los precios y las cifras se muestran con el formato colombiano (punto de miles)
    private static readonly CultureInfo Cultura = new("es-CO");

    private readonly IConsultaDAO _consultaDAO;
    private readonly GeneradorPdf _generadorPdf;

    public ReporteService(IConsultaDAO consultaDAO, GeneradorPdf generadorPdf)
    {
        _consultaDAO = consultaDAO;
        _generadorPdf = generadorPdf;
    }

    /// <summary>
    /// Reporte "Ofertas de los proveedores" (productos y proveedores).
    ///
    /// Para qué sirve en el proyecto: responde a "qué productos puede enviar cada proveedor" y a que un producto
    /// pueda adquirirse de varios proveedores. Muestra una fila por oferta con el proveedor, su empresa y ciudad,
    /// el producto, su categoría, el precio y el tiempo de entrega.
    ///
    /// Datos: es el catálogo de ofertas (JOIN de productosofertados, producto, categoria y usuario) que ya usa
    /// <see cref="ConsultaService"/>, con los mismos filtros más los de este reporte; así no hay un segundo SQL.
    /// El catálogo llega ordenado por precio, y aquí se reordena por proveedor para que el PDF agrupe sus ofertas.
    ///
    /// Filtros del usuario, todos opcionales (los vacíos no se aplican): proveedor (id), ciudad, empresa y nombre
    /// del producto (coincidencia parcial). Devuelve el PDF como bytes para que quien lo pida decida si lo guarda
    /// en un archivo o lo muestra.
    /// </summary>
    public byte[] GenerarReporteOfertasPorProveedor(
        int? idProveedor = null,
        string? ciudad = null,
        string? nombreEmpresa = null,
        string? nombreProducto = null)
    {
        if (idProveedor <= 0)
            throw new ArgumentException("El id del proveedor debe ser mayor que cero.", nameof(idProveedor));

        ciudad = ConsultaService.NormalizarFiltro(ciudad);
        nombreEmpresa = ConsultaService.NormalizarFiltro(nombreEmpresa);
        nombreProducto = ConsultaService.NormalizarFiltro(nombreProducto);

        List<OfertaDetalle> ofertas = _consultaDAO.ConsultarCatalogoOfertas(
                categoria: null, ciudad, precioMinimo: null, precioMaximo: null,
                idProveedor, nombreEmpresa, nombreProducto)
            .OrderBy(oferta => oferta.Proveedor)
            .ThenBy(oferta => oferta.ProveedorId)
            .ThenBy(oferta => oferta.Producto)
            .ToList();

        var columnas = new List<ColumnaReporte>
        {
            new("Proveedor", 2),
            new("Empresa", 2),
            new("Ciudad", 1.5f),
            new("Producto", 2.5f),
            new("Categoría", 1.5f),
            new("Precio", 1.5f),
            new("Entrega", 1),
        };

        var filas = ofertas
            .Select(oferta => new[]
            {
                oferta.Proveedor,
                oferta.NombreEmpresa ?? "-",
                oferta.Ciudad ?? "-",
                oferta.Producto,
                oferta.Categoria,
                FormatearPrecio(oferta.Precio),
                FormatearDias(oferta.TiempoEntrega),
            })
            .ToList();

        return _generadorPdf.GenerarTabla(
            "Reporte de ofertas por proveedor",
            DescribirFiltros(
                ("Proveedor (id)", idProveedor?.ToString()),
                ("Ciudad", ciudad),
                ("Empresa", nombreEmpresa),
                ("Producto contiene", nombreProducto)),
            columnas,
            filas);
    }

    /// <summary>
    /// Reporte "Proveedores y su portafolio" (productos y proveedores).
    ///
    /// Para qué sirve en el proyecto: el enunciado exige guardar los datos de los proveedores, que pueden estar en
    /// cualquier ciudad, y que un proveedor pueda proveer varios productos. Muestra una fila por proveedor con su
    /// empresa y ciudad, cuántos productos y categorías distintas ofrece, y su precio y tiempo de entrega promedio.
    ///
    /// Datos: ConsultarPortafolioProveedores (JOIN de usuario, productosofertados y producto con LEFT JOIN, así que
    /// los proveedores sin ofertas también aparecen, con ceros y promedios vacíos).
    ///
    /// Filtros del usuario, opcionales: ciudad y empresa.
    /// </summary>
    public byte[] GenerarReporteProveedoresPortafolio(string? ciudad = null, string? nombreEmpresa = null)
    {
        ciudad = ConsultaService.NormalizarFiltro(ciudad);
        nombreEmpresa = ConsultaService.NormalizarFiltro(nombreEmpresa);

        List<PortafolioProveedor> proveedores = _consultaDAO.ConsultarPortafolioProveedores(ciudad, nombreEmpresa);

        var columnas = new List<ColumnaReporte>
        {
            new("Proveedor", 2.5f),
            new("Empresa", 2),
            new("Ciudad", 1.5f),
            new("Productos", 1),
            new("Categorías", 1),
            new("Precio promedio", 1.5f),
            new("Entrega promedio", 1.5f),
        };

        var filas = proveedores
            .Select(proveedor => new[]
            {
                proveedor.Proveedor,
                proveedor.NombreEmpresa ?? "-",
                proveedor.Ciudad ?? "-",
                proveedor.Productos.ToString(Cultura),
                proveedor.Categorias.ToString(Cultura),
                FormatearPrecioOpcional(proveedor.PrecioPromedio),
                FormatearDiasPromedio(proveedor.EntregaPromedio),
            })
            .ToList();

        return _generadorPdf.GenerarTabla(
            "Reporte de proveedores y su portafolio",
            DescribirFiltros(("Ciudad", ciudad), ("Empresa", nombreEmpresa)),
            columnas,
            filas);
    }

    /// <summary>
    /// Reporte "Productos y su mejor proveedor" (productos y proveedores).
    ///
    /// Para qué sirve en el proyecto: el sistema existe para "determinar los proveedores a los que se pueden realizar
    /// los pedidos". Para cada producto indica qué proveedor lo ofrece al menor precio, con su empresa, ciudad y
    /// tiempo de entrega. Si dos proveedores empatan en el menor precio, salen los dos. Los productos que nadie
    /// ofrece no aparecen.
    ///
    /// Datos: ConsultarMejorOfertaPorProducto (JOIN de productosofertados, producto, categoria y usuario, con una
    /// subconsulta que calcula el precio mínimo de cada producto). Se ordena por nombre de producto.
    ///
    /// Filtros del usuario, opcionales: nombre del producto (coincidencia parcial) y ciudad. Con ciudad, el usuario
    /// decide cómo se interpreta mediante <paramref name="soloProveedoresDeLaCiudad"/>:
    /// - true (predeterminado): la "mejor oferta" se calcula entre los proveedores de esa ciudad, es decir, "el más
    ///   barato de Pereira" para cada producto que se ofrezca allí.
    /// - false: la mejor oferta se calcula entre todos los proveedores y la ciudad solo filtra, es decir, "los
    ///   productos cuyo proveedor más barato del país es de Pereira". Puede no salir ningún producto.
    /// Sin ciudad el parámetro no tiene efecto. El PDF indica cuál de las dos formas se usó.
    /// </summary>
    public byte[] GenerarReporteMejorProveedorPorProducto(
        string? nombreProducto = null,
        string? ciudad = null,
        bool soloProveedoresDeLaCiudad = true)
    {
        nombreProducto = ConsultaService.NormalizarFiltro(nombreProducto);
        ciudad = ConsultaService.NormalizarFiltro(ciudad);

        List<OfertaDetalle> ofertas = _consultaDAO.ConsultarMejorOfertaPorProducto(nombreProducto, ciudad, soloProveedoresDeLaCiudad)
            .OrderBy(oferta => oferta.Producto)
            .ThenBy(oferta => oferta.ProductoId)
            .ThenBy(oferta => oferta.Proveedor)
            .ToList();

        var columnas = new List<ColumnaReporte>
        {
            new("Producto", 2.5f),
            new("Categoría", 1.5f),
            new("Mejor proveedor", 2),
            new("Empresa", 2),
            new("Ciudad", 1.5f),
            new("Precio", 1.5f),
            new("Entrega", 1),
        };

        var filas = ofertas
            .Select(oferta => new[]
            {
                oferta.Producto,
                oferta.Categoria,
                oferta.Proveedor,
                oferta.NombreEmpresa ?? "-",
                oferta.Ciudad ?? "-",
                FormatearPrecio(oferta.Precio),
                FormatearDias(oferta.TiempoEntrega),
            })
            .ToList();

        return _generadorPdf.GenerarTabla(
            "Reporte de productos y su mejor proveedor",
            DescribirFiltros(
                ("Producto contiene", nombreProducto),
                ("Ciudad", ciudad is null ? null : $"{ciudad} ({DescribirAlcance(soloProveedoresDeLaCiudad)})")),
            columnas,
            filas);
    }

    /// <summary>
    /// Reporte "Productos por categoría" (productos con sus categorías).
    ///
    /// Para qué sirve en el proyecto: el enunciado dice que las categorías identifican las "familias de productos".
    /// Lista los productos agrupados por su categoría, con su descripción, para ver qué hay en cada familia.
    ///
    /// Datos: ConsultarProductosPorCategoria (JOIN de producto y categoria). Sale ordenado por categoría y nombre.
    /// Muestra todos los productos, estén o no ofertados por algún proveedor.
    ///
    /// Filtros del usuario, opcionales: categoría y nombre del producto (coincidencia parcial).
    /// </summary>
    public byte[] GenerarReporteProductosPorCategoria(string? categoria = null, string? nombreProducto = null)
    {
        categoria = ConsultaService.NormalizarFiltro(categoria);
        nombreProducto = ConsultaService.NormalizarFiltro(nombreProducto);

        List<ProductoCategoria> productos = _consultaDAO.ConsultarProductosPorCategoria(categoria, nombreProducto);

        var columnas = new List<ColumnaReporte>
        {
            new("Categoría", 1.5f),
            new("Producto", 2.5f),
            new("Descripción", 4),
        };

        var filas = productos
            .Select(producto => new[]
            {
                producto.Categoria,
                producto.Producto,
                producto.Descripcion ?? "-",
            })
            .ToList();

        return _generadorPdf.GenerarTabla(
            "Reporte de productos por categoría",
            DescribirFiltros(("Categoría", categoria), ("Producto contiene", nombreProducto)),
            columnas,
            filas);
    }

    /// <summary>
    /// Reporte "Resumen por categoría" (productos con sus categorías).
    ///
    /// Para qué sirve en el proyecto: el enunciado también pide guardar los datos básicos de las categorías. Da una
    /// vista de cada familia de productos: cuántos productos tiene, cuántas ofertas de proveedores y en qué rango
    /// de precios (mínimo, máximo y promedio). Las categorías sin ofertas aparecen con ceros y precios vacíos.
    ///
    /// Datos: ConsultarResumenPorCategoria (JOIN de categoria, producto y productosofertados con LEFT JOIN).
    ///
    /// Filtro del usuario, opcional: categoría.
    /// </summary>
    public byte[] GenerarReporteResumenPorCategoria(string? categoria = null)
    {
        categoria = ConsultaService.NormalizarFiltro(categoria);

        List<ResumenCategoria> resumenes = _consultaDAO.ConsultarResumenPorCategoria(categoria);

        var columnas = new List<ColumnaReporte>
        {
            new("Categoría", 2),
            new("Productos", 1),
            new("Ofertas", 1),
            new("Precio mínimo", 1.5f),
            new("Precio máximo", 1.5f),
            new("Precio promedio", 1.5f),
        };

        var filas = resumenes
            .Select(resumen => new[]
            {
                resumen.Categoria,
                resumen.Productos.ToString(Cultura),
                resumen.Ofertas.ToString(Cultura),
                FormatearPrecioOpcional(resumen.PrecioMinimo),
                FormatearPrecioOpcional(resumen.PrecioMaximo),
                FormatearPrecioOpcional(resumen.PrecioPromedio),
            })
            .ToList();

        return _generadorPdf.GenerarTabla(
            "Reporte resumen por categoría",
            DescribirFiltros(("Categoría", categoria)),
            columnas,
            filas);
    }

    /// <summary>
    /// Arma el texto "Ciudad: Pereira · Empresa: D1" con los filtros que sí se aplicaron,
    /// o "Sin filtros" si el usuario no eligió ninguno, para que el PDF diga qué datos contiene.
    /// </summary>
    private static string DescribirFiltros(params (string Nombre, string? Valor)[] filtros)
    {
        var aplicados = filtros
            .Where(filtro => filtro.Valor is not null)
            .Select(filtro => $"{filtro.Nombre}: {filtro.Valor}")
            .ToList();

        return aplicados.Count == 0 ? "Sin filtros" : string.Join(" · ", aplicados);
    }

    /// <summary>Texto que el PDF muestra junto a la ciudad para que se sepa cómo se calculó la mejor oferta.</summary>
    private static string DescribirAlcance(bool soloProveedoresDeLaCiudad) =>
        soloProveedoresDeLaCiudad ? "mejor oferta entre los proveedores de la ciudad" : "mejor oferta entre todos los proveedores";

    internal static string FormatearPrecio(decimal precio) => $"$ {precio.ToString("N0", Cultura)}";

    internal static string FormatearDias(int dias) => dias == 1 ? "1 día" : $"{dias} días";

    /// <summary>
    /// Los precios de MIN, MAX y AVG son null cuando no hay ofertas; en el PDF se muestra un guion.
    /// Los formateadores son internos porque <see cref="Demostracion"/> muestra los mismos datos en consola con el mismo formato.
    /// </summary>
    internal static string FormatearPrecioOpcional(decimal? precio) => precio is null ? "-" : FormatearPrecio(precio.Value);

    internal static string FormatearDiasPromedio(decimal? dias) => dias is null ? "-" : $"{dias.Value.ToString("N1", Cultura)} días";
}
