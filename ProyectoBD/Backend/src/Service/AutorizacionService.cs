/// <summary>
/// el objetivo de esta clase es definir los diferentes tipos de autorizaciones que puede tener un usuario
/// esta clase sirve como apoyo de verifacion de rol dentro del sistema, ayudando a definir las 
/// capacidades de cada tipo de usuario
/// </summary>

public class AutorizacionService
{
    private readonly UsuarioDAO _usuarioDao;

    public AutorizacionService(UsuarioDAO usuarioDao)
    {
        _usuarioDao = usuarioDao;
    }

    public void ValidarRol(int idSolicitante, RolUsuario rolRequerido)
    {
        RolUsuario? rol = _usuarioDao.ObtenerRolUsuarioPorId(idSolicitante);

        if (rol != rolRequerido)
            throw new UnauthorizedAccessException("No tienes permiso para esta acción");
    }
}