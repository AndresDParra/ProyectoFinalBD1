/// <summary>
/// Clase para manejar la conexión a la base de datos MySQL.
/// 
/// Esta clase centraliza la configuración necesaria para acceder a la base de datos
/// y permite crear objetos de conexión reutilizables en el resto de la aplicación.
/// 
/// La idea es que otras clases no tengan que escribir la cadena de conexión cada vez
/// que necesiten consultar o modificar datos. En su lugar, llaman al método Obtener()
/// para recibir una instancia de MySqlConnection lista para usarse.
/// 
/// La cadena puede configurarse mediante MYSQL_CONNECTION_STRING o mediante las
/// variables MYSQL_HOST, MYSQL_PORT, MYSQL_DATABASE, MYSQL_USER y MYSQL_PASSWORD.
/// </summary>

using MySql.Data.MySqlClient;

public static class Conexion
{
    private static readonly string cadena = ConstruirCadena();

    /// <summary>
    /// Crea y devuelve una nueva conexión a la base de datos MySQL usando la cadena
    /// de conexión configurada en esta clase.
    /// </summary>
    /// <returns>Un objeto MySqlConnection listo para abrir y utiliavailablezar.</returns>

    public static MySqlConnection Obtener()
    {
        return new MySqlConnection(cadena);
    }

    /// <summary>
    /// Abre una conexión para comprobar que la configuración y el servidor son válidos.
    /// </summary>
    public static async Task ProbarAsync(CancellationToken cancellationToken = default)
    {
        await using var conexion = Obtener();
        await conexion.OpenAsync(cancellationToken);
    }

    /// <summary>
    /// Construye la cadena de conexión que utilizarán los DAO para acceder a MySQL.
    /// Primero busca una cadena completa en MYSQL_CONNECTION_STRING. Si no existe,
    /// crea una cadena con los valores de MYSQL_HOST, MYSQL_PORT, MYSQL_DATABASE,
    /// MYSQL_USER y MYSQL_PASSWORD. La base de datos predeterminada es "proyecto"
    /// y los valores configurables tienen valores locales para facilitar el desarrollo.
    /// </summary>
    private static string ConstruirCadena()
    {
        var cadenaConfigurada = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(cadenaConfigurada))
        {
            return cadenaConfigurada;
        }

        var builder = new MySqlConnectionStringBuilder
        {
            Server = Variable("MYSQL_HOST", "localhost"),
            Database = Variable("MYSQL_DATABASE", "proyecto"),
            UserID = Variable("MYSQL_USER", "root"),
            Password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD") ?? string.Empty,
            SslMode = MySqlSslMode.Preferred,
            ConnectionTimeout = 5
        };

        if (uint.TryParse(Environment.GetEnvironmentVariable("MYSQL_PORT"), out var port))
        {
            builder.Port = port;
        }

        return builder.ConnectionString;
    }

    /// <summary>
    /// Obtiene una variable de entorno y, cuando no está definida o está vacía,
    /// devuelve el valor predeterminado recibido. Este método centraliza la lectura
    /// de configuración opcional y evita repetir la misma comprobación en cada campo.
    /// </summary>
    /// <param name="nombre">Nombre de la variable de entorno que se debe consultar.</param>
    /// <param name="valorPorDefecto">Valor utilizado si la variable no está configurada.</param>
    /// <returns>El valor configurado o el valor predeterminado.</returns>
    private static string Variable(string nombre, string valorPorDefecto)
    {
        return Environment.GetEnvironmentVariable(nombre) is { Length: > 0 } valor
            ? valor
            : valorPorDefecto;
    }
}

