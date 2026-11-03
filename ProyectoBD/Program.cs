// Sin argumentos el programa se comporta como siempre. Con argumentos entra en el modo de demostración
// (ver Demostracion), que existe hasta que haya una interfaz gráfica para ver consultas y reportes.
if (args.Length == 0)
{
    Console.WriteLine("Hello, World!");
    Console.WriteLine();
    Console.WriteLine(Demostracion.Ayuda);
    return 0;
}

return await Demostracion.EjecutarAsync(args);
