namespace Vercom.Services;

public class ConexionRequest
{
    public string Servidor { get; set; } = "localhost";
    public string BaseDatos { get; set; } = "TIERRA_PROMETIDA";
    public string Usuario { get; set; } = "sa";
    public string Password { get; set; } = "sql2026*";
    public List<string>? Secciones { get; set; }
}
