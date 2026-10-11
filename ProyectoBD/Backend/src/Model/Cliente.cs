/// <summary>
/// Representa un cliente del sistema, que hereda de la clase Usuario y agrega un identificador único para cada cliente.
/// este cliente tiene la capacidad realizar compras y revisar catalogos de productos
/// </summary>


public class Cliente : Usuario
{
    public Cliente(
        int idCliente, // primary key del cliente en la base de datos
        string nombre,
        string direccion,
        string telefono,
        string cedula,
        string correo,
        DateOnly fechaNacimiento,
        string genero)
        : base(nombre, direccion, telefono, cedula, correo, fechaNacimiento, genero)
    {
        IdCliente = idCliente;
    }

    public int IdCliente { get; private set; }
}