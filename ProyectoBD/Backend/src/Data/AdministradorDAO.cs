using System.Data;
using MySql.Data.MySqlClient;

public class AdministradorDAO : IAdministradorDAO
{
	public bool Crear(Administrador administrador)
	{
		ArgumentNullException.ThrowIfNull(administrador);

		const string verificarSql = "SELECT COUNT(*) FROM administrador;";
		const string insertarSql = """
			INSERT INTO administrador (
				AidAdministrador, Anombre, Adireccion, Atelefono,
				Acedula, Acorreo, AfechaNacimiento, Agenero)
			VALUES (
				@idAdministrador, @nombre, @direccion, @telefono,
				@cedula, @correo, @fechaNacimiento, @genero);
			""";

		using var conexion = Conexion.Obtener();
		conexion.Open();

		using var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);
		using var verificar = new MySqlCommand(verificarSql, conexion, transaccion);

		if (Convert.ToInt32(verificar.ExecuteScalar()) > 0)
		{
			return false;
		}

		using var insertar = new MySqlCommand(insertarSql, conexion, transaccion);
		insertar.Parameters.AddWithValue("@idAdministrador", administrador.IdAdministrador);
		insertar.Parameters.AddWithValue("@nombre", administrador.Nombre);
		insertar.Parameters.AddWithValue("@direccion", administrador.Direccion);
		insertar.Parameters.AddWithValue("@telefono", administrador.Telefono);
		insertar.Parameters.AddWithValue("@cedula", administrador.Cedula);
		insertar.Parameters.AddWithValue("@correo", administrador.Correo);
		insertar.Parameters.AddWithValue("@fechaNacimiento", administrador.FechaNacimiento.ToDateTime(TimeOnly.MinValue));
		insertar.Parameters.AddWithValue("@genero", administrador.Genero);

		var insertado = insertar.ExecuteNonQuery() == 1;
		transaccion.Commit();
		return insertado;
	}

	public Administrador ConsultarId(int idAdministrador)
	{
		const string sql = """
			SELECT AidAdministrador
			FROM administrador
			WHERE AidAdministrador = @idAdministrador;
			""";

		using var conexion = Conexion.Obtener();
		using var comando = new MySqlCommand(sql, conexion);
		comando.Parameters.AddWithValue("@idAdministrador", idAdministrador);

		conexion.Open();
		using var lector = comando.ExecuteReader();

		if (!lector.Read())
		{
			throw new KeyNotFoundException($"No existe un administrador con el ID {idAdministrador}.");
		}

		return new Administrador(
			lector.GetInt32("AidAdministrador"),
			string.Empty,
			string.Empty,
			string.Empty,
			string.Empty,
			string.Empty,
			DateOnly.MinValue,
			string.Empty);
	}

	public bool Actualizar(Administrador administrador)
	{
		ArgumentNullException.ThrowIfNull(administrador);

		const string sql = """
			UPDATE administrador
			SET AidAdministrador = @idAdministrador
			WHERE AidAdministrador = @idAdministrador;
			""";

		using var conexion = Conexion.Obtener();
		using var comando = new MySqlCommand(sql, conexion);
		comando.Parameters.AddWithValue("@idAdministrador", administrador.IdAdministrador);

		conexion.Open();
		return comando.ExecuteNonQuery() == 1;
	}

	public bool Existe(int idAdministrador)
	{
		const string sql = """
			SELECT COUNT(*)
			FROM administrador
			WHERE AidAdministrador = @idAdministrador;
			""";

		using var conexion = Conexion.Obtener();
		using var comando = new MySqlCommand(sql, conexion);
		comando.Parameters.AddWithValue("@idAdministrador", idAdministrador);

		conexion.Open();
		return Convert.ToInt32(comando.ExecuteScalar()) > 0;
	}
}