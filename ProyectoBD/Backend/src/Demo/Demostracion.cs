using System.Text;

/// <summary>
/// Modo de demostración para ver el backend funcionando mientras no exista la interfaz gráfica: muestra las
/// consultas en la consola y genera los reportes en PDF. No tiene lógica de negocio: solo arma los servicios, los
/// llama con algunos filtros de ejemplo e imprime el resultado. Se invoca desde Program.cs con
/// <c>dotnet run -- consultas</c> o <c>dotnet run -- reportes [carpeta]</c>.
/// </summary>
public static class Demostracion
{
    public const string Ayuda = """
        Modos de demostración (hasta que exista la interfaz gráfica):
          dotnet run -- consultas            Muestra en consola las consultas con JOIN y con subconsultas
          dotnet run -- reportes [carpeta]   Genera los reportes en PDF (por defecto en la carpeta temporal del sistema)
        """;

    /// <summary>
    /// Ejecuta el modo pedido. Devuelve 0 si todo salió bien y 1 si hubo un problema, para que el código de salida
    /// sirva también en scripts. Antes de empezar comprueba la conexión, porque un error de configuración
    /// (servidor apagado, base inexistente o contraseña incorrecta) es lo más común y merece un mensaje claro.
    /// </summary>
    public static async Task<int> EjecutarAsync(string[] args)
    {
        string modo = args[0].ToLowerInvariant();

        if (modo is not ("consultas" or "reportes"))
        {
            Console.Error.WriteLine($"Modo desconocido: {args[0]}");
            Console.Error.WriteLine(Ayuda);
            return 1;
        }

        try
        {
            await Conexion.ProbarAsync();
        }
        catch (Exception excepcion)
        {
            Console.Error.WriteLine($"No se pudo conectar a MySQL: {excepcion.Message}");
            Console.Error.WriteLine("Revisa que el servidor esté activo, que la base exista (BDproyecto.sql) y las variables MYSQL_*.");
            return 1;
        }

        try
        {
            var consultaDao = new ConsultaDAO();

            if (modo == "consultas")
                MostrarConsultas(new ConsultaService(consultaDao));
            else
                GenerarReportes(new ReporteService(consultaDao, new GeneradorPdf()), args.Length > 1 ? args[1] : null);

            return 0;
        }
        catch (ErrorDeBaseDeDatos excepcion)
        {
            Console.Error.WriteLine($"Error de base de datos: {excepcion.InnerException?.Message ?? excepcion.Message}");
            Console.Error.WriteLine("¿Se cargó BDproyecto.sql completo en la base configurada?");
            return 1;
        }
    }

    /// <summary>
    /// Imprime las cinco consultas que pide el enunciado (tres con JOIN y dos con subconsulta) y el listado
    /// de productos por categoría, cada una con las tablas que usa, para poder explicarlas al mostrarlas.
    /// </summary>
    private static void MostrarConsultas(ConsultaService servicio)
    {
        var ofertas = servicio.ConsultarCatalogoOfertas();
        AvisarSiNoHayOfertas(ofertas.Count);

        Imprimir(
            "1. Catálogo de ofertas  [JOIN de 4 tablas: productosofertados, producto, categoria, usuario]",
            ["Proveedor", "Ciudad", "Producto", "Categoría", "Precio", "Entrega"],
            ofertas.Select(o => new[]
            {
                o.Proveedor, o.Ciudad ?? "-", o.Producto, o.Categoria,
                ReporteService.FormatearPrecio(o.Precio), ReporteService.FormatearDias(o.TiempoEntrega),
            }));

        Imprimir(
            "2. Resumen por categoría  [JOIN de 3 tablas: categoria, producto, productosofertados]",
            ["Categoría", "Productos", "Ofertas", "Precio mínimo", "Precio máximo", "Precio promedio"],
            servicio.ConsultarResumenPorCategoria().Select(r => new[]
            {
                r.Categoria, r.Productos.ToString(), r.Ofertas.ToString(),
                ReporteService.FormatearPrecioOpcional(r.PrecioMinimo),
                ReporteService.FormatearPrecioOpcional(r.PrecioMaximo),
                ReporteService.FormatearPrecioOpcional(r.PrecioPromedio),
            }));

        Imprimir(
            "3. Portafolio de proveedores  [JOIN de 3 tablas: usuario, productosofertados, producto]",
            ["Proveedor", "Empresa", "Ciudad", "Productos", "Categorías", "Precio promedio", "Entrega promedio"],
            servicio.ConsultarPortafolioProveedores().Select(p => new[]
            {
                p.Proveedor, p.NombreEmpresa ?? "-", p.Ciudad ?? "-", p.Productos.ToString(), p.Categorias.ToString(),
                ReporteService.FormatearPrecioOpcional(p.PrecioPromedio),
                ReporteService.FormatearDiasPromedio(p.EntregaPromedio),
            }));

        Imprimir(
            "4. Mejor oferta por producto  [subconsulta MIN(precio): productosofertados con producto y usuario]",
            ["Producto", "Categoría", "Mejor proveedor", "Ciudad", "Precio", "Entrega"],
            servicio.ConsultarMejorOfertaPorProducto().Select(o => new[]
            {
                o.Producto, o.Categoria, o.Proveedor, o.Ciudad ?? "-",
                ReporteService.FormatearPrecio(o.Precio), ReporteService.FormatearDias(o.TiempoEntrega),
            }));

        Imprimir(
            "5. Productos sin ninguna oferta  [subconsulta NOT EXISTS: producto y productosofertados]",
            ["Id", "Producto", "Descripción"],
            servicio.ConsultarProductosSinOferta().Select(p => new[]
            {
                p.IdProducto.ToString(), p.Nombre, p.Descripcion,
            }));

        Imprimir(
            "Extra. Productos de la categoría Libros  [JOIN de producto y categoria, con filtro]",
            ["Id", "Categoría", "Producto", "Descripción"],
            servicio.ConsultarProductosPorCategoria(categoria: "Libros").Select(p => new[]
            {
                p.ProductoId.ToString(), p.Categoria, p.Producto, p.Descripcion ?? "-",
            }));
    }

    /// <summary>
    /// Genera los cinco reportes, varios con y sin filtros para ver que estos cambian el resultado, y escribe
    /// cada PDF en una carpeta. El servicio solo devuelve bytes; guardarlos en archivos es trabajo de quien lo llama.
    /// </summary>
    private static void GenerarReportes(ReporteService servicio, string? carpetaIndicada)
    {
        string carpeta = carpetaIndicada ?? Path.Combine(Path.GetTempPath(), "reportes_prueba");
        Directory.CreateDirectory(carpeta);

        var reportes = new (string Nombre, Func<byte[]> Generar)[]
        {
            ("1_proveedores_sin_filtros", () => servicio.GenerarReporteProveedoresPortafolio()),
            ("1_proveedores_ciudad_armenia", () => servicio.GenerarReporteProveedoresPortafolio(ciudad: "armenia")),
            ("2_ofertas_sin_filtros", () => servicio.GenerarReporteOfertasPorProveedor()),
            ("2_ofertas_producto_libro", () => servicio.GenerarReporteOfertasPorProveedor(nombreProducto: "libro")),
            ("3_mejor_proveedor_sin_filtros", () => servicio.GenerarReporteMejorProveedorPorProducto()),
            ("3_mejor_proveedor_armenia_en_la_ciudad", () => servicio.GenerarReporteMejorProveedorPorProducto(ciudad: "armenia")),
            ("3_mejor_proveedor_armenia_global", () => servicio.GenerarReporteMejorProveedorPorProducto(ciudad: "armenia", soloProveedoresDeLaCiudad: false)),
            ("4_productos_por_categoria", () => servicio.GenerarReporteProductosPorCategoria()),
            ("4_productos_categoria_libros", () => servicio.GenerarReporteProductosPorCategoria(categoria: "Libros")),
            ("5_resumen_sin_filtros", () => servicio.GenerarReporteResumenPorCategoria()),
            ("5_resumen_electronica", () => servicio.GenerarReporteResumenPorCategoria(categoria: "Electronica")),
        };

        Console.WriteLine($"Generando reportes en: {Path.GetFullPath(carpeta)}");

        foreach (var (nombre, generar) in reportes)
        {
            byte[] pdf = generar();
            string ruta = Path.Combine(carpeta, nombre + ".pdf");
            File.WriteAllBytes(ruta, pdf);
            Console.WriteLine($"  {nombre}.pdf  ({pdf.Length / 1024} KB)");
        }

        Console.WriteLine();
        Console.WriteLine("Ábrelos con el visor de PDF del sistema, por ejemplo: xdg-open " + Path.GetFullPath(carpeta));
        Console.WriteLine("Si salen vacíos, falta cargar ofertas en la tabla productosofertados.");
    }

    /// <summary>
    /// La tabla productosofertados no trae datos en BDproyecto.sql, y sin ofertas casi todas las consultas
    /// salen vacías; el aviso evita que parezca un error.
    /// </summary>
    private static void AvisarSiNoHayOfertas(int cantidadOfertas)
    {
        if (cantidadOfertas == 0)
            Console.WriteLine("AVISO: no hay ofertas en la tabla productosofertados; la mayoría de las consultas saldrán vacías.\n");
    }

    /// <summary>
    /// Imprime una tabla de texto con las columnas alineadas. El ancho de cada columna se calcula con su contenido
    /// para no tener que fijar anchos a mano, y las filas vacías se indican con un mensaje en vez de dejar solo el título.
    /// </summary>
    private static void Imprimir(string titulo, string[] columnas, IEnumerable<string[]> filas)
    {
        var lista = filas.ToList();
        int[] anchos = columnas
            .Select((columna, i) => Math.Max(columna.Length, lista.Count == 0 ? 0 : lista.Max(fila => fila[i].Length)))
            .ToArray();

        string Linea(string[] celdas) => string.Join("  ", celdas.Select((celda, i) => celda.PadRight(anchos[i]))).TrimEnd();

        Console.WriteLine(titulo);
        Console.WriteLine(Linea(columnas));
        Console.WriteLine(new string('-', anchos.Sum() + 2 * (anchos.Length - 1)));

        foreach (var fila in lista)
            Console.WriteLine(Linea(fila));

        Console.WriteLine(lista.Count == 0 ? "(sin resultados)" : $"({lista.Count} filas)");
        Console.WriteLine();
    }
}
