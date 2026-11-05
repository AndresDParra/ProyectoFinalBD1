/// <summary>
/// Una sesión de un usuario en el sistema: cuándo entró y cuándo salió. Solo transporta datos, no valida nada.
///
/// FechaSalida es null mientras la sesión sigue abierta o cuando el programa se cerró sin registrar la salida;
/// <see cref="SalidaRegistrada"/> permite distinguirlo sin comparar con null. IdUsuario y NombreUsuario son null
/// cuando el usuario fue eliminado: la bitácora conserva el registro aunque ya no se sepa de quién era.
/// Es la fuente de <see cref="BitacoraService"/> y del reporte de bitácora de <see cref="ReporteService"/>,
/// ambos solo para el Administrador.
/// </summary>
public record RegistroBitacora(
    int IdBitacora,
    int? IdUsuario,
    string? NombreUsuario,
    DateTime FechaEntrada,
    DateTime? FechaSalida)
{
    public bool SalidaRegistrada => FechaSalida is not null;
}
