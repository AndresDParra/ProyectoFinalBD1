public class CorreoUsuarioDuplicadoException : UsuarioServiceException
{
    public CorreoUsuarioDuplicadoException(string correo)
        : base($"Ya existe un usuario con el correo {correo}.")
    {
    }
}
