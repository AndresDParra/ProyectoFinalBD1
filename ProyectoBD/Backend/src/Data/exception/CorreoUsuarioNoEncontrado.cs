public class CorreoUsuarioNoEncontradoException : UsuarioDaoException
{
    public CorreoUsuarioNoEncontradoException(string correo)
        : base($"No existe un usuario con el correo {correo}.")
    {
    }
}