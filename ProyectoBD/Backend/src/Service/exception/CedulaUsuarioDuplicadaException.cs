public class CedulaUsuarioDuplicadaException : UsuarioServiceException
{
    public CedulaUsuarioDuplicadaException(string cedula)
        : base($"Ya existe un usuario con la cédula {cedula}.")
    {
    }
}
