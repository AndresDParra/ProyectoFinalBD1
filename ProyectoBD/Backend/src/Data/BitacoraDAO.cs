using System.Collections.Generic;
using MySql.Data.MySqlClient;

/// <summary>
/// Guarda y consulta la bitácora: una fila por sesión, creada al entrar y completada al salir.
///
/// Espera esta tabla en la base de datos (idusuario admite NULL y se pone en NULL al borrar el usuario,
/// para conservar el historial de quien ya no existe):
///
///   CREATE TABLE `bitacora` (
///     `idbitacora` int NOT NULL AUTO_INCREMENT,
///     `idusuario` int DEFAULT NULL,
///     `fechaentrada` datetime NOT NULL,
///     `fechasalida` datetime DEFAULT NULL,
///     PRIMARY KEY (`idbitacora`),
///     KEY `idusuario_bitacora_idx` (`idusuario`),
///     CONSTRAINT `idusuario_bitacora` FOREIGN KEY (`idusuario`)
///       REFERENCES `usuario` (`idusuario`) ON DELETE SET NULL
///   ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/// </summary>
public class BitacoraDAO : IBitacoraDAO
{
    public bool RegistrarEntrada(int idUsuario, DateTime fechaEntrada)
    {
        const string sql = """
            INSERT INTO bitacora (idusuario, fechaentrada)
            VALUES (@idUsuario, @fechaEntrada);
            """;

        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@idUsuario", idUsuario);
            comando.Parameters.AddWithValue("@fechaEntrada", fechaEntrada);

            return comando.ExecuteNonQuery() == 1;
        }
        catch (MySqlException exception)
        {
            throw new ErrorDeBaseDeDatos(exception);
        }
    }

    /// <summary>
    /// Cierra la sesión abierta más reciente del usuario. Se hace así, y no con el id de la fila,
    /// para que quien cierra sesión solo necesite saber quién es. Devuelve false si no tenía ninguna abierta.
    /// </summary>
    public bool RegistrarSalida(int idUsuario, DateTime fechaSalida)
    {
        const string sql = """
            UPDATE bitacora
            SET fechasalida = @fechaSalida
            WHERE idusuario = @idUsuario
              AND fechasalida IS NULL
            ORDER BY fechaentrada DESC
            LIMIT 1;
            """;

        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@idUsuario", idUsuario);
            comando.Parameters.AddWithValue("@fechaSalida", fechaSalida);

            return comando.ExecuteNonQuery() == 1;
        }
        catch (MySqlException exception)
        {
            throw new ErrorDeBaseDeDatos(exception);
        }
    }

    /// <summary>
    /// Devuelve las sesiones, de la más reciente a la más antigua. Los filtros son opcionales y se combinan:
    /// el rango de fechas se aplica a la fecha de entrada. Usa LEFT JOIN para que sigan apareciendo las sesiones
    /// de usuarios eliminados, que quedaron con idusuario en NULL.
    /// </summary>
    public List<RegistroBitacora> Consultar(int? idUsuario, DateTime? desde, DateTime? hasta)
    {
        const string sql = """
            SELECT b.idbitacora,
                   b.idusuario,
                   u.nombre,
                   b.fechaentrada,
                   b.fechasalida
            FROM bitacora b
            LEFT JOIN usuario u ON u.idusuario = b.idusuario
            WHERE (@idUsuario IS NULL OR b.idusuario = @idUsuario)
              AND (@desde IS NULL OR b.fechaentrada >= @desde)
              AND (@hasta IS NULL OR b.fechaentrada <= @hasta)
            ORDER BY b.fechaentrada DESC, b.idbitacora DESC;
            """;

        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@idUsuario", (object?)idUsuario ?? DBNull.Value);
            comando.Parameters.AddWithValue("@desde", (object?)desde ?? DBNull.Value);
            comando.Parameters.AddWithValue("@hasta", (object?)hasta ?? DBNull.Value);

            using var lector = comando.ExecuteReader();
            var registros = new List<RegistroBitacora>();

            while (lector.Read())
            {
                int posUsuario = lector.GetOrdinal("idusuario");
                int posNombre = lector.GetOrdinal("nombre");
                int posSalida = lector.GetOrdinal("fechasalida");

                registros.Add(new RegistroBitacora(
                    lector.GetInt32("idbitacora"),
                    lector.IsDBNull(posUsuario) ? null : lector.GetInt32(posUsuario),
                    lector.IsDBNull(posNombre) ? null : lector.GetString(posNombre),
                    lector.GetDateTime("fechaentrada"),
                    lector.IsDBNull(posSalida) ? null : lector.GetDateTime(posSalida)));
            }

            return registros;
        }
        catch (MySqlException exception)
        {
            throw new ErrorDeBaseDeDatos(exception);
        }
    }
}
