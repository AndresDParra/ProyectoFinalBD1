public class OperacionUsuarioNoPermitidaException : UsuarioDaoException
{
    public OperacionUsuarioNoPermitidaException(string message)
        : base(message)
    {
    }
}
