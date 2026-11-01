/// <summary>
/// Consulta de la bitácora de entrada y salida. Es información de auditoría,
/// por eso solo la puede ver el Administrador.
/// </summary>
public class BitacoraService
{
    private readonly AutorizacionService _autorizacionService;
    private readonly IBitacoraDAO _bitacoraDAO;

    public BitacoraService(AutorizacionService autorizacionService, IBitacoraDAO bitacoraDAO)
    {
        _autorizacionService = autorizacionService;
        _bitacoraDAO = bitacoraDAO;
    }

    /// <summary>
    /// Sesiones registradas, de la más reciente a la más antigua. Todos los filtros son opcionales:
    /// usuario, y rango de fechas aplicado a la fecha de entrada.
    /// </summary>
    public List<RegistroBitacora> ConsultarBitacora(
        int idSolicitante,
        int? idUsuario = null,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        _autorizacionService.ValidarRol(idSolicitante, RolUsuario.Administrador);

        if (idUsuario <= 0)
            throw new ArgumentException("El id del usuario debe ser mayor que cero.", nameof(idUsuario));

        if (desde > hasta)
            throw new ArgumentException("La fecha inicial no puede ser posterior a la final.");

        return _bitacoraDAO.Consultar(idUsuario, desde, hasta);
    }
}
