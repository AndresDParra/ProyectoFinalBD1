/// <summary>
/// el objetivo de esta clase es definir los diferentes tipos de autorizaciones que puede tener un usuario
/// esta clase sirve como apoyo de verifacion de rol dentro del sistema, ayudando a definir las 
/// capacidades de cada tipo de usuario
/// </summary>

public class AutorizacionService
{
    private readonly IUsuarioDAO _usuarioDao;

    public AutorizacionService(IUsuarioDAO usuarioDao)
    {
        _usuarioDao = usuarioDao;
    }

    /// <summary>
    /// Verifica que el solicitante tenga alguno de los roles permitidos.
    /// Lanza UnauthorizedAccessException si no existe o su rol no está en la lista.
    /// </summary>
    public void ValidarRol(int idSolicitante, params RolUsuario[] rolesPermitidos)
    {
        RolUsuario? rol = _usuarioDao.ObtenerRolUsuarioPorId(idSolicitante);

        if (rol is null || !rolesPermitidos.Contains(rol.Value))
            throw new UnauthorizedAccessException("No tienes permiso para esta acción");
    }
}