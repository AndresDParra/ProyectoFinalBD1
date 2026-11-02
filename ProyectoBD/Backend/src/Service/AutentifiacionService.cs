using BCrypt.Net;

/// <summary>
/// Servicio de autenticación: verifica credenciales, gestiona el cambio de contraseña
/// y registra en la bitácora cuándo entra y sale cada usuario.
/// El hash de la contraseña nunca vive en la entidad Usuario; solo se lee del DAO
/// en el momento de verificarlo.
/// </summary>
public class AutentificacionService
{
    private readonly IUsuarioDAO _usuarioDao;
    private readonly IBitacoraDAO _bitacoraDao;

    // Hash falso para gastar el mismo tiempo cuando el correo no existe
    private static readonly string HashFalso =
        BCrypt.Net.BCrypt.HashPassword("contrasena-falsa-para-igualar-tiempos");

    public AutentificacionService(IUsuarioDAO usuarioDao, IBitacoraDAO bitacoraDao)
    {
        _usuarioDao = usuarioDao;
        _bitacoraDao = bitacoraDao;
    }

    /// <summary>
    /// Verifica las credenciales, registra la entrada en la bitácora y devuelve el usuario autenticado.
    /// Lanza UnauthorizedAccessException si son inválidas. Si no se puede registrar la entrada el inicio
    /// de sesión falla, para que no haya ingresos sin registrar.
    /// </summary>
    public Usuario Login(string correo, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrEmpty(contrasena))
            throw new ArgumentException("Correo y contraseña son obligatorios");

        var credenciales = _usuarioDao.ObtenerCredencialesPorCorreo(correo.Trim());

        // Siempre se ejecuta un Verify, exista o no el usuario
        string hash = credenciales?.Hash ?? HashFalso;
        bool valida = BCrypt.Net.BCrypt.Verify(contrasena, hash);

        if (credenciales == null || !valida)
            throw new UnauthorizedAccessException("Credenciales inválidas");

        Usuario usuario = credenciales.Value.Usuario;

        if (!_bitacoraDao.RegistrarEntrada(usuario.IdUsuario, DateTime.Now))
            throw new InvalidOperationException("No se pudo registrar la entrada en la bitácora.");

        return usuario;
    }

    /// <summary>
    /// Registra la salida del usuario en la bitácora, cerrando su sesión abierta más reciente.
    /// Devuelve false si no tenía ninguna sesión abierta.
    /// </summary>
    public bool Logout(int idUsuario)
    {
        if (idUsuario <= 0)
            throw new ArgumentException("El id del usuario debe ser mayor que cero.", nameof(idUsuario));

        return _bitacoraDao.RegistrarSalida(idUsuario, DateTime.Now);
    }

    /// <summary>
    /// Cambia la contraseña de un usuario, exigiendo la actual.
    /// </summary>
    public void CambiarContrasena(int idUsuario, string actual, string nueva)
    {
        if (idUsuario <= 0)
            throw new ArgumentOutOfRangeException(nameof(idUsuario));

        if (string.IsNullOrEmpty(actual))
            throw new ArgumentException("La contraseña actual es obligatoria");

        ValidarContrasenaNueva(nueva);

        string? hashActual = _usuarioDao.ObtenerHashPorId(idUsuario);

        // Verify siempre, exista o no, para igualar tiempos
        bool valida = BCrypt.Net.BCrypt.Verify(actual, hashActual ?? HashFalso);

        if (hashActual == null || !valida)
            throw new UnauthorizedAccessException("Credenciales inválidas");

        string nuevoHash = BCrypt.Net.BCrypt.HashPassword(nueva);
        _usuarioDao.ActualizarContrasena(idUsuario, nuevoHash);
    }

    /// <summary>
    /// Valida la contraseña en texto plano ANTES de hashearla.
    /// </summary>
    public static void ValidarContrasenaNueva(string contrasena)
    {
        if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 8)
            throw new ArgumentException("La contraseña debe tener al menos 8 caracteres");
    }
}