using System.Collections.Generic;
using MySql.Data.MySqlClient;

/// <summary>
/// Consultas de lectura que combinan varias tablas. Tres usan JOIN (catálogo de ofertas,
/// resumen por categoría y portafolio de proveedores) y dos usan subconsultas
/// (mejor oferta por producto y productos sin oferta).
/// </summary>
public class ConsultaDAO : IConsultaDAO
{
    /// <summary>
    /// Se separa en una constante porque el catálogo de ofertas y la mejor oferta por producto devuelven lo mismo
    /// y solo cambian en el WHERE y el ORDER BY; así el SELECT y los JOIN se escriben una sola vez.
    /// </summary>
    private const string SelectOfertaDetalle = """
        SELECT po.proveedorid,
               u.nombre AS proveedor,
               u.nombreempresa,
               u.ciudad,
               p.Prid,
               p.nombre AS producto,
               c.nombre AS categoria,
               po.precio,
               po.tiempoentrega
        FROM productosofertados po
        INNER JOIN producto p ON p.Prid = po.producto
        INNER JOIN categoria c ON c.Cacodigo = p.categoria
        INNER JOIN usuario u ON u.idusuario = po.proveedorid
        """;

    /// <summary>
    /// Existe para no repetir en cada consulta el mismo código de abrir la conexión, ejecutar y recorrer las filas.
    /// Cada consulta solo aporta su SQL, sus parámetros y cómo convertir una fila; los errores de MySQL se
    /// envuelven en <see cref="ErrorDeBaseDeDatos"/> en un solo lugar.
    /// </summary>
    private static List<T> Consultar<T>(string sql, Action<MySqlCommand> agregarParametros, Func<MySqlDataReader, T> mapear)
    {
        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();
            using var comando = new MySqlCommand(sql, conexion);
            agregarParametros(comando);

            using var lector = comando.ExecuteReader();
            var resultado = new List<T>();
            while (lector.Read())
                resultado.Add(mapear(lector));

            return resultado;
        }
        catch (MySqlException exception)
        {
            throw new ErrorDeBaseDeDatos(exception);
        }
    }

    /// <summary>
    /// Se separa porque dos consultas devuelven una <see cref="OfertaDetalle"/> y deben leerla igual.
    /// Empresa y ciudad se revisan con <c>IsDBNull</c> porque pueden ser NULL en la base.
    /// </summary>
    private static OfertaDetalle MapearOfertaDetalle(MySqlDataReader lector)
    {
        int posEmpresa = lector.GetOrdinal("nombreempresa");
        int posCiudad = lector.GetOrdinal("ciudad");

        return new OfertaDetalle(
            lector.GetInt32("proveedorid"),
            lector.GetString("proveedor"),
            lector.IsDBNull(posEmpresa) ? null : lector.GetString(posEmpresa),
            lector.IsDBNull(posCiudad) ? null : lector.GetString(posCiudad),
            lector.GetInt32("Prid"),
            lector.GetString("producto"),
            lector.GetString("categoria"),
            lector.GetDecimal("precio"),
            lector.GetInt32("tiempoentrega"));
    }

    /// <summary>
    /// <c>MIN</c>, <c>MAX</c> y <c>AVG</c> devuelven NULL cuando una categoría o un proveedor no tiene ofertas
    /// (por el LEFT JOIN), y <c>GetDecimal</c> lanzaría una excepción con ese NULL.
    /// </summary>
    private static decimal? LeerDecimalNulo(MySqlDataReader lector, string columna)
    {
        int pos = lector.GetOrdinal(columna);
        return lector.IsDBNull(pos) ? null : lector.GetDecimal(pos);
    }

    /// <summary>
    /// JOIN de cuatro tablas (productosofertados, producto, categoria y usuario).
    /// Todos los filtros son opcionales: un filtro null no se aplica.
    /// </summary>
    public List<OfertaDetalle> ConsultarCatalogoOfertas(
        string? categoria, string? ciudad, decimal? precioMinimo, decimal? precioMaximo)
    {
        const string sql = $"""
            {SelectOfertaDetalle}
            WHERE (@categoria IS NULL OR c.nombre = @categoria)
              AND (@ciudad IS NULL OR u.ciudad = @ciudad)
              AND (@precioMinimo IS NULL OR po.precio >= @precioMinimo)
              AND (@precioMaximo IS NULL OR po.precio <= @precioMaximo)
            ORDER BY po.precio, po.proveedorid, p.Prid;
            """;

        return Consultar(sql,
            comando =>
            {
                comando.Parameters.AddWithValue("@categoria", (object?)categoria ?? DBNull.Value);
                comando.Parameters.AddWithValue("@ciudad", (object?)ciudad ?? DBNull.Value);
                comando.Parameters.AddWithValue("@precioMinimo", (object?)precioMinimo ?? DBNull.Value);
                comando.Parameters.AddWithValue("@precioMaximo", (object?)precioMaximo ?? DBNull.Value);
            },
            MapearOfertaDetalle);
    }

    /// <summary>
    /// JOIN de tres tablas (categoria, producto y productosofertados). Los LEFT JOIN hacen que
    /// también aparezcan las categorías sin productos y los productos sin ofertas.
    /// </summary>
    public List<ResumenCategoria> ConsultarResumenPorCategoria()
    {
        const string sql = """
            SELECT c.nombre AS categoria,
                   COUNT(DISTINCT p.Prid) AS productos,
                   COUNT(po.producto) AS ofertas,
                   MIN(po.precio) AS preciominimo,
                   MAX(po.precio) AS preciomaximo,
                   AVG(po.precio) AS preciopromedio
            FROM categoria c
            LEFT JOIN producto p ON p.categoria = c.Cacodigo
            LEFT JOIN productosofertados po ON po.producto = p.Prid
            GROUP BY c.Cacodigo, c.nombre
            ORDER BY c.Cacodigo;
            """;

        // COUNT devuelve BIGINT en MySQL, por eso se lee con GetInt64 y se convierte a int
        return Consultar(sql, _ => { }, lector => new ResumenCategoria(
            lector.GetString("categoria"),
            (int)lector.GetInt64("productos"),
            (int)lector.GetInt64("ofertas"),
            LeerDecimalNulo(lector, "preciominimo"),
            LeerDecimalNulo(lector, "preciomaximo"),
            LeerDecimalNulo(lector, "preciopromedio")));
    }

    /// <summary>
    /// JOIN de tres tablas (usuario, productosofertados y producto) con los proveedores
    /// y sus ofertas. El LEFT JOIN conserva a los proveedores que todavía no ofrecen nada.
    /// </summary>
    public List<PortafolioProveedor> ConsultarPortafolioProveedores()
    {
        const string sql = """
            SELECT u.idusuario,
                   u.nombre AS proveedor,
                   u.nombreempresa,
                   u.ciudad,
                   COUNT(DISTINCT p.Prid) AS productos,
                   COUNT(DISTINCT p.categoria) AS categorias,
                   AVG(po.precio) AS preciopromedio,
                   AVG(po.tiempoentrega) AS entregapromedio
            FROM usuario u
            LEFT JOIN productosofertados po ON po.proveedorid = u.idusuario
            LEFT JOIN producto p ON p.Prid = po.producto
            WHERE u.rolusuario = @rolProveedor
            GROUP BY u.idusuario, u.nombre, u.nombreempresa, u.ciudad
            ORDER BY productos DESC, u.idusuario;
            """;

        return Consultar(sql,
            comando => comando.Parameters.AddWithValue("@rolProveedor", (int)RolUsuario.Proveedor),
            lector =>
            {
                int posEmpresa = lector.GetOrdinal("nombreempresa");
                int posCiudad = lector.GetOrdinal("ciudad");

                return new PortafolioProveedor(
                    lector.GetInt32("idusuario"),
                    lector.GetString("proveedor"),
                    lector.IsDBNull(posEmpresa) ? null : lector.GetString(posEmpresa),
                    lector.IsDBNull(posCiudad) ? null : lector.GetString(posCiudad),
                    (int)lector.GetInt64("productos"),
                    (int)lector.GetInt64("categorias"),
                    LeerDecimalNulo(lector, "preciopromedio"),
                    LeerDecimalNulo(lector, "entregapromedio"));
            });
    }

    /// <summary>
    /// Subconsulta correlacionada: de cada producto devuelve la oferta (o las ofertas, si hay empate)
    /// cuyo precio es igual al mínimo precio ofrecido para ese mismo producto.
    /// </summary>
    public List<OfertaDetalle> ConsultarMejorOfertaPorProducto()
    {
        const string sql = $"""
            {SelectOfertaDetalle}
            WHERE po.precio = (
                SELECT MIN(menor.precio)
                FROM productosofertados menor
                WHERE menor.producto = po.producto
            )
            ORDER BY p.Prid, po.proveedorid;
            """;

        return Consultar(sql, _ => { }, MapearOfertaDetalle);
    }

    /// <summary>
    /// Subconsulta con NOT EXISTS: productos que ningún proveedor ofrece todavía.
    /// Son también los únicos que <c>ProductoService.EliminarProducto</c> permite borrar.
    /// </summary>
    public List<Producto> ConsultarProductosSinOferta()
    {
        const string sql = """
            SELECT p.Prid, p.nombre, p.descripcion, p.categoria
            FROM producto p
            WHERE NOT EXISTS (
                SELECT 1
                FROM productosofertados po
                WHERE po.producto = p.Prid
            )
            ORDER BY p.Prid;
            """;

        return Consultar(sql, _ => { }, lector => new Producto(
            lector.GetInt32("Prid"),
            lector.GetString("nombre"),
            lector.GetString("descripcion"),
            ProductoDAO.CodigoACategoria(lector.GetInt32("categoria"))));
    }
}
