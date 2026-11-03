/// <summary>
/// Servicio de consultas que combinan varias tablas. Cualquier usuario puede ejecutarlas,
/// porque solo leen datos, por lo que no se valida el rol del solicitante.
/// </summary>
public class ConsultaService
{
    private readonly IConsultaDAO _consultaDAO;

    public ConsultaService(IConsultaDAO consultaDAO)
    {
        _consultaDAO = consultaDAO;
    }

    /// <summary>
    /// Un texto vacío o con solo espacios significa "sin filtro". Es interno porque
    /// <see cref="ReporteService"/> aplica la misma regla a los filtros de los reportes.
    /// </summary>
    internal static string? NormalizarFiltro(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }

    /// <summary>
    /// Ofertas con su producto, categoría y proveedor. Todos los filtros son opcionales.
    /// </summary>
    public List<OfertaDetalle> ConsultarCatalogoOfertas(
        string? categoria = null,
        string? ciudad = null,
        decimal? precioMinimo = null,
        decimal? precioMaximo = null)
    {
        if (precioMinimo < 0 || precioMaximo < 0)
            throw new ArgumentException("Los precios del filtro no pueden ser negativos.");

        if (precioMinimo > precioMaximo)
            throw new ArgumentException("El precio mínimo no puede ser mayor que el precio máximo.");

        return _consultaDAO.ConsultarCatalogoOfertas(
            NormalizarFiltro(categoria), NormalizarFiltro(ciudad), precioMinimo, precioMaximo,
            idProveedor: null, nombreEmpresa: null, nombreProducto: null);
    }

    public List<ResumenCategoria> ConsultarResumenPorCategoria()
    {
        return _consultaDAO.ConsultarResumenPorCategoria(categoria: null);
    }

    public List<PortafolioProveedor> ConsultarPortafolioProveedores()
    {
        return _consultaDAO.ConsultarPortafolioProveedores(ciudad: null, nombreEmpresa: null);
    }

    /// <summary>
    /// Productos con el nombre de su categoría, para ver las "familias" de productos o buscar un producto
    /// por nombre. Los dos filtros son opcionales.
    /// </summary>
    public List<ProductoCategoria> ConsultarProductosPorCategoria(string? categoria = null, string? nombreProducto = null)
    {
        return _consultaDAO.ConsultarProductosPorCategoria(NormalizarFiltro(categoria), NormalizarFiltro(nombreProducto));
    }

    public List<OfertaDetalle> ConsultarMejorOfertaPorProducto()
    {
        return _consultaDAO.ConsultarMejorOfertaPorProducto(
            nombreProducto: null, ciudad: null, mejorSoloEntreLaCiudad: true);
    }

    public List<Producto> ConsultarProductosSinOferta()
    {
        return _consultaDAO.ConsultarProductosSinOferta();
    }
}
