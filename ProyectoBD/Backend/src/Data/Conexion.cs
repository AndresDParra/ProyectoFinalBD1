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
/// Actualmente la conexión está en desarrollo y se planea utilizar MySql.Data como
/// dependencia de NuGet para interactuar con MySQL desde C#. La librería aún debe
/// estar instalada y configurada correctamente en el proyecto.
/// </summary>

// using MySql.Data.MySqlClient; referencia a la libreria de MySQL para C# (NuGet)
public static class Conexion
{
    private static readonly string cadena =
        "Server=localhost;Database=proveedores_db;Uid=root;Pwd=tu_clave;";
  
    /// <summary>
    /// Crea y devuelve una nueva conexión a la base de datos MySQL usando la cadena
    /// de conexión configurada en esta clase.
    /// </summary>
    /// <returns>Un objeto MySqlConnection listo para abrir y utilizar.</returns>
    
    public static MySqlConnection Obtener()
    {
        return new MySqlConnection(cadena);
    }
}

