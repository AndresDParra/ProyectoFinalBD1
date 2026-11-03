using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

/// <summary>
/// Convierte una tabla de datos ya formateados en un PDF. Es la única clase que conoce QuestPDF y no sabe
/// nada de productos ni proveedores: todos los reportes comparten el mismo diseño y solo cambian las columnas
/// y las filas. Así, cambiar de biblioteca o de estilo se hace en un solo lugar.
/// </summary>
public class GeneradorPdf
{
    static GeneradorPdf()
    {
        // QuestPDF exige declarar la licencia antes de generar el primer documento
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Genera un PDF en horizontal con encabezado, tabla y pie de página.
    /// Cada fila debe tener tantas celdas como columnas. Si no hay filas, el PDF muestra un aviso
    /// en lugar de una tabla vacía.
    /// </summary>
    public byte[] GenerarTabla(
        string titulo,
        string filtrosAplicados,
        IReadOnlyList<ColumnaReporte> columnas,
        IReadOnlyList<string[]> filas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(titulo);
        ArgumentNullException.ThrowIfNull(columnas);
        ArgumentNullException.ThrowIfNull(filas);

        if (columnas.Count == 0)
            throw new ArgumentException("El reporte necesita al menos una columna.", nameof(columnas));

        if (filas.Any(fila => fila.Length != columnas.Count))
            throw new ArgumentException("Todas las filas deben tener una celda por cada columna.", nameof(filas));

        DateTime generado = DateTime.Now;

        return Document.Create(documento => documento.Page(pagina =>
        {
            pagina.Size(PageSizes.A4.Landscape());
            pagina.Margin(30);
            pagina.DefaultTextStyle(estilo => estilo.FontSize(9));

            pagina.Header().PaddingBottom(10).Column(encabezado =>
            {
                encabezado.Item().Text(titulo).FontSize(16).Bold();
                encabezado.Item().Text($"Generado el {generado:dd/MM/yyyy HH:mm}").FontColor(Colors.Grey.Darken1);
                encabezado.Item().Text($"Filtros: {filtrosAplicados}").FontColor(Colors.Grey.Darken1);
                encabezado.Item().Text($"Registros: {filas.Count}").FontColor(Colors.Grey.Darken1);
            });

            pagina.Content().Element(contenido =>
            {
                if (filas.Count == 0)
                {
                    contenido.PaddingTop(20).Text("No hay resultados con los filtros indicados.").Italic();
                    return;
                }

                contenido.Table(tabla =>
                {
                    tabla.ColumnsDefinition(definicion =>
                    {
                        foreach (var columna in columnas)
                            definicion.RelativeColumn(columna.Ancho);
                    });

                    // el encabezado de la tabla se repite en cada página
                    tabla.Header(cabecera =>
                    {
                        foreach (var columna in columnas)
                            cabecera.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(columna.Titulo).Bold();
                    });

                    foreach (var fila in filas)
                        foreach (var celda in fila)
                            tabla.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(4).Text(celda);
                });
            });

            pagina.Footer().AlignCenter().Text(texto =>
            {
                texto.Span("Página ");
                texto.CurrentPageNumber();
                texto.Span(" de ");
                texto.TotalPages();
            });
        })).GeneratePdf();
    }
}
