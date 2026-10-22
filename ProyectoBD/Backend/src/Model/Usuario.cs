/// <summary>
/// Representa la información común a cualquier usuario del sistema.
/// </summary>
public abstract class Usuario
{
    protected Usuario(
        string nombre,
        string direccion,
        string telefono,
        string cedula,
        string correo,
        DateOnly fechaNacimiento,
        string genero)
    {
        Nombre = nombre;
        Direccion = direccion;
        Telefono = telefono;
        Cedula = cedula;
        Correo = correo;
        FechaNacimiento = fechaNacimiento;
        Genero = genero;
    }

    public string Nombre { get; protected set; }
    public string Direccion { get; protected set; }
    public string Telefono { get; protected set; }
    public string Cedula { get; }
    public string Correo { get; protected set; }
    public DateOnly FechaNacimiento { get; }
    public string Genero { get; }
}