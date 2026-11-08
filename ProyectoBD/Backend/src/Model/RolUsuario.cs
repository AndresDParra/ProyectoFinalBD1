/// <summary>
/// en esta clase es encuentran los diferentes roles que pude tener un usuario dentro del sistema
/// aqui se definen su rol y se le asigna un valor que representa su id 
/// </summary>

public enum RolUsuario
{
    Administrador = 1, // representa al usario con maxima autirdad sobre la base de datos
    Proveedor = 2, // representa al usuario tradicional 
    Vendedor = 3, // representa al usario esporadico
}