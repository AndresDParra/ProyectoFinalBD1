using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

/// <summary>
/// Clase VendedorDAO que implementa la interfaz IVendedorDAO para realizar operaciones CRUD en la entidad Vendedor.
/// en cada metodo se define la logica para interactuar con la base de datos, como insertar,
/// consultar, actualizar y eliminar registros de vendedores.
/// </summary>

public class VendedorDAO : IVendedorDAO
{
    public bool Crear(Vendedor vendedor)
    {
        ArgumentNullException.ThrowIfNull(vendedor);

        const string sql = """
            INSERT INTO vendedor (VidVendedor)
            VALUES (@idVendedor);
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idVendedor", vendedor.IdVendedor);

        conexion.Open();
        return comando.ExecuteNonQuery() == 1;
    }

    public Vendedor ConsultarId(int idVendedor)
    {
        const string sql = """
            SELECT VidVendedor
            FROM vendedor
            WHERE VidVendedor = @idVendedor;
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idVendedor", idVendedor);

        conexion.Open();
        using var lector = comando.ExecuteReader();

        if (!lector.Read())
        {
            throw new KeyNotFoundException($"No existe un vendedor con el ID {idVendedor}.");
        }

        return new Vendedor(
            lector.GetInt32("VidVendedor"),
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            DateOnly.MinValue,
            string.Empty);
    }

    public bool Actualizar(Vendedor vendedor)
    {
        ArgumentNullException.ThrowIfNull(vendedor);

        const string sql = """
            UPDATE vendedor
            SET VidVendedor = @nuevoIdVendedor
            WHERE VidVendedor = @idVendedor;
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@nuevoIdVendedor", vendedor.IdVendedor);
        comando.Parameters.AddWithValue("@idVendedor", vendedor.IdVendedor);

        conexion.Open();
        return comando.ExecuteNonQuery() == 1;
    }

    public bool Eliminar(int idVendedor)
    {
        const string sql = "DELETE FROM vendedor WHERE VidVendedor = @idVendedor;";

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idVendedor", idVendedor);

        conexion.Open();
        return comando.ExecuteNonQuery() == 1;
    }

    public bool Existe(string idVendedor)
    {
        if (!int.TryParse(idVendedor, out var id))
        {
            return false;
        }

        const string sql = "SELECT EXISTS(SELECT 1 FROM vendedor WHERE VidVendedor = @idVendedor);";

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idVendedor", id);

        conexion.Open();
        return Convert.ToInt32(comando.ExecuteScalar()) == 1;
    }
}