
using System.Text;

namespace Vercom.Services;

public interface IFileStorageService
{
    Task<(bool Success, string Message, string? RelativePath)> SaveFileAsync(
        IFormFile file,
        string subFolder,
        string? prefix = null);

    Task<(bool Success, string Message)> DeleteFileAsync(string relativePath);

    string GetPhysicalPath(string relativePath);
    string GetPublicUrl(string relativePath);
}



public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(
        IWebHostEnvironment env,
        IConfiguration config,
        ILogger<FileStorageService> logger)
    {
        _env = env;
        _config = config;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, string? RelativePath)> SaveFileAsync(
        IFormFile file,
        string subFolder,
        string? prefix = null)
    {
        if (file == null || file.Length == 0)
            return (false, "El archivo está vacío.", null);

        // Validar tamaño
        var maxSizeMB = _config.GetValue<int>("FileStorage:MaxFileSizeMB", 10);
        if (file.Length > maxSizeMB * 1024 * 1024)
            return (false, $"El archivo excede el tamaño máximo de {maxSizeMB} MB.", null);

        // Validar extensión
        var allowedExtensions = _config.GetSection("FileStorage:AllowedExtensions")
            .Get<string[]>() ?? new[] { ".pdf" };

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return (false, $"Extensión no permitida. Permitidas: {string.Join(", ", allowedExtensions)}", null);

        try
        {
            // ✅ Obtener WebRootPath con seguridad
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
            }

            // Normalizar a minúsculas para evitar problemas de case-sensitivity en Linux
            var basePath = (_config["FileStorage:BasePath"] ?? "uploads").ToLowerInvariant();
            subFolder = subFolder.ToLowerInvariant();

            var folderPath = Path.Combine(webRoot, basePath, subFolder);

            // ✅ Intento de creación con log detallado
            if (!Directory.Exists(folderPath))
            {
                _logger.LogInformation("Creando directorio de subidas: {Path}", folderPath);
                Directory.CreateDirectory(folderPath);

                if (!OperatingSystem.IsWindows())
                {
                    try
                    {
                        File.SetUnixFileMode(folderPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead | UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("No se pudo establecer permisos en carpeta: {Msg}", ex.Message);
                    }
                }
            }

            // ✅ Nombre único basado en GUID (sin nombre original)
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var shortGuid = Guid.NewGuid().ToString("N").Substring(0, 8);
            var uniqueName = string.IsNullOrEmpty(prefix)
                ? $"{timestamp}_{shortGuid}{extension}"
                : $"{prefix.ToLowerInvariant()}_{timestamp}_{shortGuid}{extension}";

            var fullPath = Path.Combine(folderPath, uniqueName);

            // ✅ Guardar archivo con reporte de ruta en caso de fallo
            try
            {
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ioEx)
            {
                _logger.LogError(ioEx, "Fallo de E/S al escribir archivo en {Path}", fullPath);
                return (false, $"Error de escritura en el servidor. Verifique permisos en: {fullPath}", null);
            }

            // ✅ Establecer permisos Unix al archivo (644 - lectura para todos)
            if (!OperatingSystem.IsWindows())
            {
                try
                {
                    File.SetUnixFileMode(fullPath,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.GroupRead | UnixFileMode.OtherRead);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudieron establecer permisos Unix para el archivo {Path}", fullPath);
                }
            }

            // ✅ Ruta relativa uniforme para la web
            var relativePath = $"/{basePath}/{subFolder}/{uniqueName}".Replace("\\", "/");

            _logger.LogInformation("Archivo guardado exitosamente: {Path}", relativePath);

            return (true, "Archivo guardado correctamente.", relativePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Permisos denegados al guardar {File}", file.FileName);
            return (false, "No hay permisos para guardar el archivo. Contacte al administrador.", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar archivo {File}", file.FileName);
            return (false, $"Error al guardar: {ex.Message}", null);
        }
    }

    public Task<(bool Success, string Message)> DeleteFileAsync(string relativePath)
    {
        try
        {
            if (string.IsNullOrEmpty(relativePath))
                return Task.FromResult((false, "Ruta vacía."));

            var fullPath = GetPhysicalPath(relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Archivo eliminado: {Path}", relativePath);
                return Task.FromResult((true, "Archivo eliminado."));
            }

            return Task.FromResult((false, "El archivo no existe."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar archivo {Path}", relativePath);
            return Task.FromResult((false, $"Error: {ex.Message}"));
        }
    }

    public string GetPhysicalPath(string relativePath)
    {
        // Normalizar separadores y quitar la barra inicial si existe
        var cleanPath = relativePath.Replace("\\", "/").TrimStart('/');

        var webRoot = _env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
        }

        return Path.Combine(webRoot, cleanPath);
    }

    public string GetPublicUrl(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return string.Empty;

        // Asegurar que empiece con /
        var url = relativePath.Replace("\\", "/");
        if (!url.StartsWith("/")) url = "/" + url;

        return url;
    }

    /// <summary>
    /// Limpia el nombre del archivo de caracteres problemáticos
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return "archivo";

        var sb = new StringBuilder();
        foreach (var c in fileName.Normalize(NormalizationForm.FormD))
        {
            // Quitar diacríticos (tildes)
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                // Solo letras, números, guiones y guiones bajos
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
                    sb.Append(c);
                else
                    sb.Append('_');
            }
        }

        var result = sb.ToString().Normalize(NormalizationForm.FormC);

        // Limitar longitud
        if (result.Length > 50)
            result = result.Substring(0, 50);

        return result;
    }
}