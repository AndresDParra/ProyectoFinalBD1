using System.Collections.Generic;

/// <summary>
/// Interfaz para las consultas que combinan varias tablas (JOIN) o usan subconsultas.
/// No pertenecen a una sola entidad, por eso están separadas de los DAO de cada tabla.
/// </summary>
public interface IConsultaDAO
{
    // consultas con JOIN
    List<OfertaDetalle> ConsultarCatalogoOfertas(string? categoria, string? ciudad, decimal? precioMinimo, decimal? precioMaximo);
    List<ResumenCategoria> ConsultarResumenPorCategoria();
    List<PortafolioProveedor> ConsultarPortafolioProveedores();

    // consultas con subconsulta
    List<OfertaDetalle> ConsultarMejorOfertaPorProducto();
    List<Producto> ConsultarProductosSinOferta();
}
