public class RolUsuarioInvalidoException : UsuarioDaoException
{
    public RolUsuarioInvalidoException(int idUsuario, int rol)
        : base($"El usuario {idUsuario} tiene un rol no valido en la base de datos: {rol}.")
    {
    }
}
