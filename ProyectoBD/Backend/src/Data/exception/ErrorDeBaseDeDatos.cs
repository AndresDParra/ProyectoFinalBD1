    using MySql.Data.MySqlClient;

    public class ErrorDeBaseDeDatos : UsuarioDaoException
    {
        public ErrorDeBaseDeDatos(MySqlException exception)
            : base("La operación de usuario no pudo completarse en MySQL.", exception)
    {
    }
    }