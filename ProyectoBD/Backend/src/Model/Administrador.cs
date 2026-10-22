/// <summary>
/// Representa un administrador del sistema, que hereda de la clase Usuario y agrega un identificador único para el administrador.
/// este administrador representa el rol maximo del sistema, tiene la capacidad
/// de gestionar clientes, vendedores y productos, ademas de tener la capacidad de generar reportes
/// </summary>



public class Administrador : Usuario
{
    public Administrador(
        int idAdministrador, // primary key del administrador en la base de datos


        string nombre,
        string direccion,
        string telefono,
        string cedula,
        string correo,
        DateOnly fechaNacimiento,
        string genero)
        : base(nombre, direccion, telefono, cedula, correo, fechaNacimiento, genero)
    {
        IdAdministrador = idAdministrador;
    }

    public int IdAdministrador { get; private set; }
}