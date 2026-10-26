/// <summary>
/// Representa la información común a cualquier usuario del sistema.
/// ademas aqui se define su el Rol que tendran las clases hijas, cada clase que herede de esta clase tendra un rol diferente
/// el cual va a limitar su capacidad en el sistema
/// </summary>
public class Usuario
{
    protected Usuario(
        int idUsuario, // primary key del usuario en la base de datos
        string nombre,
        string direccion,
        string telefono,
        string cedula,
        string correo,
        DateOnly fechaNacimiento,
        string genero,
        RolUsuario rol,
        string? nombreEmpresa = null,
        string? ciudad = null
        )
    {
        if (idUsuario <= 0)
            throw new ArgumentException("IdUsuario debe ser mayor que cero.", nameof(idUsuario));

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("Nombre no puede estar vacío.", nameof(nombre));
        if (string.IsNullOrWhiteSpace(direccion))
            throw new ArgumentException("Dirección no puede estar vacía.", nameof(direccion));

        if (string.IsNullOrWhiteSpace(telefono))
            throw new ArgumentException("Teléfono no puede estar vacío.", nameof(telefono));

        if (string.IsNullOrWhiteSpace(cedula))
            throw new ArgumentException("Cédula no puede estar vacía.", nameof(cedula));

        if (string.IsNullOrWhiteSpace(correo))
            throw new ArgumentException("Correo no puede estar vacío.", nameof(correo));

        if (fechaNacimiento == default)
            throw new ArgumentException("Fecha de nacimiento no puede estar vacía.", nameof(fechaNacimiento));

        if (string.IsNullOrWhiteSpace(genero))
            throw new ArgumentException("Género no puede estar vacío.", nameof(genero));

        Nombre = nombre;
        Direccion = direccion;
        Telefono = telefono;
        Cedula = cedula;
        Correo = correo;
        FechaNacimiento = fechaNacimiento;
        Genero = genero;
        Rol = rol;
        IdUsuario = idUsuario;

        (NombreEmpresa, Ciudad) = ValidarDatosEspecificos(rol, nombreEmpresa, ciudad);
    }

    public int IdUsuario { get; }
    public string Nombre { get; set; }
    public string Direccion { get; set; }
    public string Telefono { get; set; }
    public string Cedula { get; }
    public string Correo { get; set; }
    public DateOnly FechaNacimiento { get; }
    public string Genero { get; }

    /// <summary>
    /// atributos para representar los diferentes roles de usuario que pueden existir en el sistema
    /// esto ayudara a diferenciarlos en la base de datos
    /// </summary>
    public RolUsuario Rol { get; }
    public int IdTipoUsuario => (int)Rol;
    public string TipoUsuario => Rol.ToString();

    /// <summary>
    /// atributos extra que representan al proveedor y que solo seran utilizados por los usuarios
    /// con tipo de rol proveedor, estos atributos no seran utilizados por los usuarios con rol vendedor o administrador
    /// </summary>

    public string? NombreEmpresa { get; private set; }
    public string? Ciudad { get; private set; }

    internal void ValidarEstado()
    {
        ValidarDatosEspecificos(Rol, NombreEmpresa, Ciudad);
    }

    private static (string? NombreEmpresa, string? Ciudad) ValidarDatosEspecificos(
        RolUsuario rol,
        string? nombreEmpresa,
        string? ciudad)
    {
        if (rol == RolUsuario.Proveedor)
        {
            if (string.IsNullOrWhiteSpace(nombreEmpresa))
                throw new ArgumentException("Empresa es obligatoria para un proveedor.", nameof(nombreEmpresa));

            if (string.IsNullOrWhiteSpace(ciudad))
                throw new ArgumentException("Ciudad es obligatoria para un proveedor.", nameof(ciudad));

            return (nombreEmpresa.Trim(), ciudad.Trim());
        }

        if (rol != RolUsuario.Administrador && rol != RolUsuario.Vendedor)
            throw new ArgumentOutOfRangeException(nameof(rol), rol, "El rol de usuario no es válido.");

        if (nombreEmpresa is not null || ciudad is not null)
            throw new ArgumentException("Empresa y ciudad solo pueden existir para un proveedor.");

        return (null, null);
    }
}