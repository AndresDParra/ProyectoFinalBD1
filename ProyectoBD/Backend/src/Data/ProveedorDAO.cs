using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

/// <summary>
/// Clase ProveedorDAO que implementa la interfaz IProveedorDAO para realizar operaciones CRUD en la entidad Proveedor.
/// en cada metodo se define la logica para interactuar con la base de datos, como insertar,
/// consultar, actualizar y eliminar registros de proveedores.
/// </summary>

public class ProveedorDAO : IProveedorDAO
{
    public bool Crear(Proveedor proveedor)
    {
        ArgumentNullException.ThrowIfNull(proveedor);

        const string sql = """
            INSERT INTO proveedor (
                Pcedula, Pnombre, Pciudad, PnombreUsuario, Pdireccion,
                Ptelefono, PcedulaUsuario, Pcorreo, PfechaNacimiento, Pgenero)
            VALUES (
                @idProveedor, @nombreEmpresa, @ciudad, @nombre, @direccion,
                @telefono, @cedula, @correo, @fechaNacimiento, @genero);
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);

        comando.Parameters.AddWithValue("@idProveedor", proveedor.IdProveedor);
        comando.Parameters.AddWithValue("@nombreEmpresa", proveedor.NombreEmpresa);
        comando.Parameters.AddWithValue("@ciudad", proveedor.Ciudad);
        comando.Parameters.AddWithValue("@nombre", proveedor.Nombre);
        comando.Parameters.AddWithValue("@direccion", proveedor.Direccion);
        comando.Parameters.AddWithValue("@telefono", proveedor.Telefono);
        comando.Parameters.AddWithValue("@cedula", proveedor.Cedula);
        comando.Parameters.AddWithValue("@correo", proveedor.Correo);
        comando.Parameters.AddWithValue("@fechaNacimiento", proveedor.FechaNacimiento.ToDateTime(TimeOnly.MinValue));
        comando.Parameters.AddWithValue("@genero", proveedor.Genero);

        conexion.Open();
        return comando.ExecuteNonQuery() == 1;
    }

    public Proveedor ConsultarId(int id)
    {
        const string sql = """
            SELECT Pcedula, Pnombre, Pciudad
            FROM proveedor
            WHERE Pcedula = @idProveedor;
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idProveedor", id);

        conexion.Open();
        using var lector = comando.ExecuteReader();

        if (!lector.Read())
        {
            throw new KeyNotFoundException($"No existe un proveedor con el ID {id}.");
        }

        var idProveedor = lector.GetInt32("Pcedula");
        var nombre = lector.GetString("Pnombre");
        var ciudad = lector.GetString("Pciudad");

            return new Proveedor(
                idProveedor,
                nombre,
                ciudad,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                DateOnly.MinValue,
                string.Empty);
    }

    public bool Actualizar(Proveedor proveedor)
    {
        ArgumentNullException.ThrowIfNull(proveedor);

        const string sql = """
            UPDATE proveedor
            SET Pnombre = @nombre,
                Pciudad = @ciudad
            WHERE Pcedula = @idProveedor;
            """;

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);

        comando.Parameters.AddWithValue("@idProveedor", proveedor.IdProveedor);
        comando.Parameters.AddWithValue("@nombre", proveedor.NombreEmpresa);
        comando.Parameters.AddWithValue("@ciudad", proveedor.Ciudad);

        conexion.Open();
        return comando.ExecuteNonQuery() == 1;
    }

    public bool Eliminar(int id)
    {
        const string sql = "DELETE FROM proveedor WHERE Pcedula = @idProveedor;";

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idProveedor", id);

        conexion.Open();
        return comando.ExecuteNonQuery() == 1;
    }

    public bool Existe(int idProveedor)
    {
        const string sql = "SELECT EXISTS(SELECT 1 FROM proveedor WHERE Pcedula = @idProveedor);";

        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@idProveedor", idProveedor);

        conexion.Open();
        return Convert.ToInt32(comando.ExecuteScalar()) == 1;
    }
}