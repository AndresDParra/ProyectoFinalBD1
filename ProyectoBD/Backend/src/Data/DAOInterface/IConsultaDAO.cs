using System.Collections.Generic;

/// <summary>
/// Interfaz para las consultas que combinan varias tablas (JOIN) o usan subconsultas.
/// No pertenecen a una sola entidad, por eso están separadas de los DAO de cada tabla.
/// </summary>
public interface IConsultaDAO
{
    // consultas con JOIN
    List<OfertaDetalle> ConsultarCatalogoOfertas(
        string? categoria, string? ciudad, decimal? precioMinimo, decimal? precioMaximo,
        int? idProveedor, string? nombreEmpresa, string? nombreProducto);
    List<ResumenCategoria> ConsultarResumenPorCategoria(string? categoria);
    List<PortafolioProveedor> ConsultarPortafolioProveedores(string? ciudad, string? nombreEmpresa);
    List<ProductoCategoria> ConsultarProductosPorCategoria(string? categoria, string? nombreProducto);

    // consultas con subconsulta
    List<OfertaDetalle> ConsultarMejorOfertaPorProducto(string? nombreProducto, string? ciudad, bool mejorSoloEntreLaCiudad);
    List<Producto> ConsultarProductosSinOferta();
}
