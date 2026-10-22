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
            INSERT INTO vendedor (
                VidVendedor, Vnombre, Vdireccion, Vtelefono,
                Vcedula, Vcorreo, VfechaNacimiento, Vgenero)
            VALUES (
                @idVendedor, @nombre, @direccion, @telefono,
                @cedula, @correo, @fechaNacimiento, @genero);
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idVendedor", vendedor.IdVendedor);
        comando.Parameters.AddWithValue("@nombre", vendedor.Nombre);
        comando.Parameters.AddWithValue("@direccion", vendedor.Direccion);
        comando.Parameters.AddWithValue("@telefono", vendedor.Telefono);
        comando.Parameters.AddWithValue("@cedula", vendedor.Cedula);
        comando.Parameters.AddWithValue("@correo", vendedor.Correo);
        comando.Parameters.AddWithValue("@fechaNacimiento", vendedor.FechaNacimiento.ToDateTime(TimeOnly.MinValue));
        comando.Parameters.AddWithValue("@genero", vendedor.Genero);

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

    public bool Existe(int idVendedor)
    {
        const string sql = "SELECT EXISTS(SELECT 1 FROM vendedor WHERE VidVendedor = @idVendedor);";

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idVendedor", idVendedor);

        conexion.Open();
        return Convert.ToInt32(comando.ExecuteScalar()) == 1;
    }
}