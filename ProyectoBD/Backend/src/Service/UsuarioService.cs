/// </summary>
/// El objetivo de esta clase es permitir el acceso de los diferentes tipos de usuarios a la base de datos,
/// cada tipo de usuario tiene diferentes capacidades de acceso a la base de datos, por lo que se definen diferentes servicios para cada tipo de usuario
/// para eso se usa de apoyon la clase AutorizacionService la cual contiene la logica de autentificacion de roles de usuario
/// con el fin limitar la interacion con el sistema segun el usuario que la quiera hacer
/// </summary>

public class UsuarioService
{
    private readonly AutorizacionService _autorizacionService;
    private readonly UsuarioDAO _usuarioDAO;

    public UsuarioService(AutorizacionService autorizacionService, UsuarioDAO usuarioDAO)
    {
        _autorizacionService = autorizacionService;
        _usuarioDAO = usuarioDAO;
    }

    /// <summary>
    /// metodos para realizar CRUDs completos en el sistema estos solo pueden ser realizados por el administrador
    /// </summary>

    public bool CrearUsuario(Usuario usuario, int idSolicitante)
    {
        // Validar que el solicitante tenga rol de Administrador
        _autorizacionService.ValidarRol(idSolicitante, RolUsuario.Administrador);

        if (_usuarioDAO.ExisteUsuario(usuario.IdUsuario))
        {
            return false; // El usuario ya existe
        }

        if (usuario.Rol == RolUsuario.Administrador)
        {
            throw new UnauthorizedAccessException("No se puede crear un usuario con rol de Administrador");
        }

        return _usuarioDAO.CrearUsuario(usuario);
    }

    public bool EliminarUsuario(int idUsuario, int idSolicitante)
    {
        // Validar que el solicitante tenga rol de Administrador
        _autorizacionService.ValidarRol(idSolicitante, RolUsuario.Administrador);

        if (!_usuarioDAO.ExisteUsuario(idUsuario))
        {
            return false; // El usuario no existe
        }

        if (_usuarioDAO.ObtenerRolUsuarioPorId(idUsuario) == RolUsuario.Administrador)
        {
            throw new UnauthorizedAccessException("No se puede eliminar un usuario con rol de Administrador");
        }

        return _usuarioDAO.EliminarUsuario(idUsuario);
    }

    public bool ActualizarUsuario(Usuario usuario, int idSolicitante)
    {
        // Validar que el solicitante tenga rol de Administrador
        _autorizacionService.ValidarRol(idSolicitante, RolUsuario.Administrador);

        if (!_usuarioDAO.ExisteUsuario(usuario.IdUsuario))
        {
            return false; // El usuario no existe
        }

        if (usuario.Rol == RolUsuario.Administrador)
        {
            throw new UnauthorizedAccessException("No se puede actualizar un usuario con rol de Administrador");
        }

        return _usuarioDAO.ActualizarUsuario(usuario);
    }

    /// <summary>
    /// metodos para realizar consultas a la base de datos, estos pueden ser realizados por cualquier usuario
    /// </summary>
    
    /// metodos que solo retornan un usuario
    
}