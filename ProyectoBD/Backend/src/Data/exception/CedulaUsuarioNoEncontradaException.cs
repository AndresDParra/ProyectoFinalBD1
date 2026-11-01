public class CedulaUsuarioNoEncontradaException : UsuarioDaoException
{
    public CedulaUsuarioNoEncontradaException(string cedula)
        : base($"No existe un usuario con la cédula {cedula}.")
    {
    }
}
