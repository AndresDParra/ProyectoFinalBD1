public class UsuarioNoEncontradoException : UsuarioDaoException
{
    public UsuarioNoEncontradoException(int idUsuario)
        : base($"No existe un usuario con el ID {idUsuario}.")
    {
    }
}
