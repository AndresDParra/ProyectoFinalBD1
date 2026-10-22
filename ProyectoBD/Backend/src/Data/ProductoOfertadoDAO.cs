using System.Collections.Generic;
using MySql.Data.MySqlClient;

public class ProductoOfertadoDAO : IProductoOfertadoDAO
{
    public bool Crear(ProductoOfertado productoOfertado)
    {
        ArgumentNullException.ThrowIfNull(productoOfertado);
        ArgumentNullException.ThrowIfNull(productoOfertado.Producto);

        const string sql = """
            INSERT INTO productoofertado (Prfproveedor, Prfproducto, Prfprecio)
            VALUES (@idProveedor, @idProducto, @precio);
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);

        comando.Parameters.AddWithValue("@idProveedor", productoOfertado.ProveedorId);
        comando.Parameters.AddWithValue("@idProducto", productoOfertado.Producto.IdProducto);
        comando.Parameters.AddWithValue("@precio", productoOfertado.Precio);

        conexion.Open();
        return comando.ExecuteNonQuery() == 1;
    }

    public bool ActualizarPrecio(int idProveedor, int idProducto, decimal nuevoPrecio)
    {
        if (nuevoPrecio <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(nuevoPrecio), "El precio debe ser mayor que cero.");
        }

        const string sql = """
            UPDATE productoofertado
            SET Prfprecio = @precio
            WHERE Prfproveedor = @idProveedor
              AND Prfproducto = @idProducto;
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);

        comando.Parameters.AddWithValue("@idProveedor", idProveedor);
        comando.Parameters.AddWithValue("@idProducto", idProducto);
        comando.Parameters.AddWithValue("@precio", nuevoPrecio);

        conexion.Open();
        return comando.ExecuteNonQuery() == 1;
    }

    public bool ExistePorProducto(int idProducto)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM productoofertado
                WHERE Prfproducto = @idProducto
            );
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idProducto", idProducto);

        conexion.Open();
        return Convert.ToBoolean(comando.ExecuteScalar());
    }

    public List<ProductoOfertado> ConsultarIdProveedor(int idProveedor)
    {
        const string sql = """
            SELECT po.Prfproveedor,
                   po.Prfprecio,
                   p.Prid,
                   p.Prnombre,
                   p.Prdecripcion,
                   p.Prcategoria
            FROM productoofertado po
            INNER JOIN producto p ON p.Prid = po.Prfproducto
            WHERE po.Prfproveedor = @idProveedor
            ORDER BY po.Prfproducto;
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idProveedor", idProveedor);

        conexion.Open();
        using var lector = comando.ExecuteReader();

        var productosOfertados = new List<ProductoOfertado>();

        while (lector.Read())
        {
            var producto = new Producto(
                lector.GetInt32("Prid"),
                lector.GetString("Prnombre"),
                lector.GetString("Prdecripcion"),
                (CategoriaProducto)lector.GetInt32("Prcategoria"));

            productosOfertados.Add(new ProductoOfertado(
                lector.GetInt32("Prfproveedor"),
                producto,
                lector.GetDecimal("Prfprecio")));
        }

        return productosOfertados;
    }

    public ProductoOfertado ConsultarIdProducto(int idProducto)
    {
        const string sql = """
            SELECT po.Prfproveedor,
                   po.Prfprecio,
                   p.Prid,
                   p.Prnombre,
                   p.Prdecripcion,
                   p.Prcategoria
            FROM productoofertado po
            INNER JOIN producto p ON p.Prid = po.Prfproducto
            WHERE po.Prfproducto = @idProducto
            ORDER BY po.Prfproveedor
            LIMIT 1;
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idProducto", idProducto);

        conexion.Open();
        using var lector = comando.ExecuteReader();

        if (!lector.Read())
        {
            throw new KeyNotFoundException($"El producto {idProducto} no está ofertado.");
        }

        var producto = new Producto(
            lector.GetInt32("Prid"),
            lector.GetString("Prnombre"),
            lector.GetString("Prdecripcion"),
            (CategoriaProducto)lector.GetInt32("Prcategoria"));

        return new ProductoOfertado(
            lector.GetInt32("Prfproveedor"),
            producto,
            lector.GetDecimal("Prfprecio"));
    }

    public bool Eliminar(int idProveedor, int idProducto)
    {
        const string sql = """
            DELETE FROM productoofertado
            WHERE Prfproveedor = @idProveedor
              AND Prfproducto = @idProducto;
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);

        comando.Parameters.AddWithValue("@idProveedor", idProveedor);
        comando.Parameters.AddWithValue("@idProducto", idProducto);

        conexion.Open();
        return comando.ExecuteNonQuery() == 1;
    }

}