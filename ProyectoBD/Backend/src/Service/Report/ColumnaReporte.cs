/// <summary>
/// Describe una columna de la tabla de un reporte: su título y qué tanto espacio ocupa respecto a las demás.
/// Ancho es relativo: una columna con Ancho 2 ocupa el doble que una con Ancho 1.
/// </summary>
public record ColumnaReporte(string Titulo, float Ancho = 1);
