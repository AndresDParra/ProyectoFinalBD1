using System.Collections.Generic;

/// <summary>
/// Interfaz para el acceso a la bitácora de entrada y salida de los usuarios al sistema.
/// </summary>
public interface IBitacoraDAO
{
    bool RegistrarEntrada(int idUsuario, DateTime fechaEntrada);
    bool RegistrarSalida(int idUsuario, DateTime fechaSalida);
    List<RegistroBitacora> Consultar(int? idUsuario, DateTime? desde, DateTime? hasta);
}
