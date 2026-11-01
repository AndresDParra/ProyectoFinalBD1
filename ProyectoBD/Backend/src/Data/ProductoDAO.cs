using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

/// <summary>
/// Clase ProductoDAO que implementa la interfaz IProductoDAO para realizar operaciones CRUD en la entidad Producto.
/// en cada metodo se define la logica para interactuar con la base de datos, como insertar,
/// consultar, actualizar y eliminar registros de productos.
/// </summary>

public class ProductoDAO : IProductoDAO
{
    /// <summary>
    /// La tabla categoria numera sus códigos desde 1 (Cacodigo), mientras que el enum
    /// CategoriaProducto empieza en 0, por eso se suma o resta 1 al guardar o leer.
    /// </summary>
    internal static int CategoriaACodigo(CategoriaProducto categoria) => (int)categoria + 1;

    internal static CategoriaProducto CodigoACategoria(int codigo) => (CategoriaProducto)(codigo - 1);

    public bool Crear(Producto producto)
    {
        ArgumentNullException.ThrowIfNull(producto);

        const string sql = """
            INSERT INTO producto (Prid, nombre, descripcion, categoria)
            VALUES (@idProducto, @nombre, @descripcion, @categoria);
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);

        comando.Parameters.AddWithValue("@idProducto", producto.IdProducto);
        comando.Parameters.AddWithValue("@nombre", producto.Nombre);
        comando.Parameters.AddWithValue("@descripcion", producto.Descripcion);
        ///<summary>
        /// Se convierte el valor del enum CategoriaProducto a su representación entera para almacenarlo en la base de datos.
        /// </summary>
        comando.Parameters.AddWithValue("@categoria", CategoriaACodigo(producto.Categoria));

        conexion.Open();
        return comando.ExecuteNonQuery() == 1;
    }

    public Producto ConsultarId(int idProducto)
    {
        const string sql = """
            SELECT Prid, nombre, descripcion, categoria
            FROM producto
            WHERE Prid = @idProducto;
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idProducto", idProducto);

        conexion.Open();
        using var lector = comando.ExecuteReader();

        if (!lector.Read())
        {
            throw new KeyNotFoundException($"No existe un producto con el ID {idProducto}.");
        }

        return new Producto(
            lector.GetInt32("Prid"),
            lector.GetString("nombre"),
            lector.GetString("descripcion"),
            CodigoACategoria(lector.GetInt32("categoria")));
    }

    public bool Actualizar(Producto producto)
    {
        ArgumentNullException.ThrowIfNull(producto);

        const string sql = """
            UPDATE producto
            SET nombre = @nombre,
                descripcion = @descripcion,
                categoria = @categoria
            WHERE Prid = @idProducto;
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);

        comando.Parameters.AddWithValue("@idProducto", producto.IdProducto);
        comando.Parameters.AddWithValue("@nombre", producto.Nombre);
        comando.Parameters.AddWithValue("@descripcion", producto.Descripcion);
        comando.Parameters.AddWithValue("@categoria", CategoriaACodigo(producto.Categoria));

        conexion.Open();
        return comando.ExecuteNonQuery() == 1;
    }

    public bool Eliminar(int idProducto)
    {
        const string sql = "DELETE FROM producto WHERE Prid = @idProducto;";

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idProducto", idProducto);

        conexion.Open();
        return comando.ExecuteNonQuery() == 1;
    }

    public bool Existe(int idProducto)
    {
        const string sql = "SELECT EXISTS(SELECT 1 FROM producto WHERE Prid = @idProducto);";

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idProducto", idProducto);

        conexion.Open();
        return Convert.ToInt32(comando.ExecuteScalar()) == 1;
    }
}