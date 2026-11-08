using System.Collections.Generic;
using MySql.Data.MySqlClient;

public class ProductoOfertadoDAO : IProductoOfertadoDAO
{

    /// <summary>
    /// metodos auxiliares que facilitan la gestion de productos ofertados y validan su estado antes de realizar operaciones CRUD
    /// </summary>

    /// <summary>
    /// Valida los datos obligatorios de una oferta antes de guardarla.
    /// </summary>
    private static void ValidarOferta(ProductoOfertado ofertado)
    {
        ArgumentNullException.ThrowIfNull(ofertado);
        ArgumentNullException.ThrowIfNull(ofertado.Producto);

        if (ofertado.ProveedorId <= 0)
            throw new OfertaInvalidaException("El identificador del proveedor debe ser mayor que cero.");

        if (ofertado.Precio < 0)
            throw new OfertaInvalidaException("El precio no puede ser negativo.");

        if (ofertado.TiempoEntrega < 0)
            throw new OfertaInvalidaException("El tiempo de entrega no puede ser negativo.");
    }

    /// <summary>
    /// Valida que el precio de una oferta no sea negativo.
    /// </summary>
    private static void ValidarPrecio(decimal precio)
    {
        if (precio < 0)
            throw new OfertaInvalidaException("El precio no puede ser negativo.");
    }

    /// <summary>
    /// Comprueba que existan el proveedor y el producto relacionados con una oferta.
    /// </summary>
    private static void ValidarProveedorYProducto(
        MySqlConnection conexion,
        MySqlTransaction transaccion,
        int idProveedor,
        int idProducto)
    {
        const string sql = """
            SELECT
                EXISTS (SELECT 1 FROM proveedor WHERE idproveedor = @idProveedor),
                EXISTS (SELECT 1 FROM producto WHERE Prid = @idProducto);
            """;

        using var comando = new MySqlCommand(sql, conexion, transaccion);
        comando.Parameters.AddWithValue("@idProveedor", idProveedor);
        comando.Parameters.AddWithValue("@idProducto", idProducto);

        using var lector = comando.ExecuteReader();
        lector.Read();

        if (!lector.GetBoolean(0))
            throw new ProveedorOfertaNoEncontradoException(idProveedor);

        if (!lector.GetBoolean(1))
            throw new ProductoOfertaNoEncontradoException(idProducto);
    }

    /// <summary>
    /// Construye un producto con los datos recuperados del lector de MySQL.
    /// </summary>
    private static Producto LeerProducto(MySqlDataReader lector)
    {
        return new Producto(
            lector.GetInt32("Prid"),
            lector.GetString("Prnombre"),
            lector.GetString("Prdecripcion"),
            (CategoriaProducto)lector.GetInt32("Prcategoria"));
    }

    /// <summary>
    /// Registra una oferta nueva en la base de datos.
    /// </summary>
    public bool Crear(ProductoOfertado ofertado)
    {
        ValidarOferta(ofertado);

        const string sql = """
            INSERT INTO productosofertados
                (proveedorid, producto, precio, tiempoentrega)
            VALUES
                (@idProveedor, @idProducto, @precio, @tiempoEntrega);
            """;

        using var conexion = Conexion.Obtener();
        conexion.Open();
        using var transaccion = conexion.BeginTransaction();

        ValidarProveedorYProducto(conexion, transaccion, ofertado.ProveedorId, ofertado.Producto.IdProducto);

        using var comando = new MySqlCommand(sql, conexion, transaccion);
        comando.Parameters.AddWithValue("@idProveedor", ofertado.ProveedorId);
        comando.Parameters.AddWithValue("@idProducto", ofertado.Producto.IdProducto);
        comando.Parameters.AddWithValue("@precio", ofertado.Precio);
        comando.Parameters.AddWithValue("@tiempoEntrega", ofertado.TiempoEntrega);

        if (comando.ExecuteNonQuery() != 1)
        {
            transaccion.Rollback();
            return false;
        }

        transaccion.Commit();
        return true;
    }

    /// <summary>
    /// Actualiza únicamente el precio de una oferta existente.
    /// </summary>
    public bool ActualizarPrecio(int idProveedor, int idProducto, decimal nuevoPrecio)
    {
        ValidarPrecio(nuevoPrecio);

        const string sql = """
            UPDATE productosofertados
            SET precio = @precio
            WHERE proveedorid = @idProveedor
              AND producto = @idProducto;
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idProveedor", idProveedor);
        comando.Parameters.AddWithValue("@idProducto", idProducto);
        comando.Parameters.AddWithValue("@precio", nuevoPrecio);

        conexion.Open();
        return comando.ExecuteNonQuery() == 1;
    }

    /// <summary>
    /// Comprueba si un producto tiene al menos una oferta registrada.
    /// </summary>
    public bool ExistePorProducto(int idProducto)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM productosofertados
                WHERE producto = @idProducto
            );
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idProducto", idProducto);

        conexion.Open();
        return Convert.ToBoolean(comando.ExecuteScalar());
    }

    /// <summary>
    /// Obtiene todas las ofertas registradas para un proveedor.
    /// </summary>
    public List<ProductoOfertado> ConsultarIdProveedor(int idProveedor)
    {
        const string sql = """
            SELECT po.proveedorid,
                   po.precio,
                   po.tiempoentrega,
                   p.Prid,
                   p.Prnombre,
                   p.Prdecripcion,
                   p.Prcategoria
            FROM productosofertados po
            INNER JOIN producto p ON p.Prid = po.producto
            WHERE po.proveedorid = @idProveedor
            ORDER BY po.producto;
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idProveedor", idProveedor);

        conexion.Open();
        using var lector = comando.ExecuteReader();
        var ofertas = new List<ProductoOfertado>();

        while (lector.Read())
        {
            ofertas.Add(new ProductoOfertado(
                lector.GetInt32("proveedorid"),
                LeerProducto(lector),
                lector.GetDecimal("precio"),
                lector.GetInt32("tiempoentrega")));
        }

        return ofertas;
    }

    /// <summary>
    /// Obtiene la oferta única de un producto o informa si existen varias.
    /// </summary>
    public ProductoOfertado ConsultarIdProducto(int idProducto)
    {
        const string sql = """
            SELECT po.proveedorid,
                   po.precio,
                   po.tiempoentrega,
                   p.Prid,
                   p.Prnombre,
                   p.Prdecripcion,
                   p.Prcategoria
            FROM productosofertados po
            INNER JOIN producto p ON p.Prid = po.producto
            WHERE po.producto = @idProducto
            ORDER BY po.proveedorid;
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idProducto", idProducto);

        conexion.Open();
        using var lector = comando.ExecuteReader();

        if (!lector.Read())
            throw new ProductoOfertaNoEncontradoException(idProducto);

        var oferta = new ProductoOfertado(
            lector.GetInt32("proveedorid"),
            LeerProducto(lector),
            lector.GetDecimal("precio"),
            lector.GetInt32("tiempoentrega"));

        if (lector.Read())
            throw new MultiplesOfertasException(idProducto);

        return oferta;
    }

    /// <summary>
    /// Elimina una oferta por la combinación de proveedor y producto.
    /// </summary>
    public bool Eliminar(int idProveedor, int idProducto)
    {
        const string sql = """
            DELETE FROM productosofertados
            WHERE proveedorid = @idProveedor
              AND producto = @idProducto;
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idProveedor", idProveedor);
        comando.Parameters.AddWithValue("@idProducto", idProducto);

        conexion.Open();
        return comando.ExecuteNonQuery() == 1;
    }
}
