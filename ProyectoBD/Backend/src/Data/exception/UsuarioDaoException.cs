public class UsuarioDaoException : Exception
{
    public UsuarioDaoException(string message)
        : base(message)
    {
    }

    public UsuarioDaoException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
