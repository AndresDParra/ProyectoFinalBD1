/// <summary>
/// Resumen de una categoría de productos. Solo transporta datos, no valida nada.
///
/// La devuelve ConsultarResumenPorCategoria de <see cref="ConsultaDAO"/> (JOIN de categoria, producto y
/// productosofertados): sirve para ver cómo está cada "familia de productos", cuántos productos tiene, cuántas
/// ofertas y qué rango de precios. Como usa LEFT JOIN, siempre aparecen todas las categorías, aunque no tengan
/// productos u ofertas; en ese caso los tres precios son null y Ofertas vale 0. Sale ordenada por código de categoría.
/// </summary>
public record ResumenCategoria(
    string Categoria,
    int Productos,
    int Ofertas,
    decimal? PrecioMinimo,
    decimal? PrecioMaximo,
    decimal? PrecioPromedio);
