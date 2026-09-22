using System.Text;

namespace Vercom.Services;

public interface IExportService
{
    byte[] ExportToCsv<T>(IEnumerable<T> data);
}

public class ExportService : IExportService
{
    public byte[] ExportToCsv<T>(IEnumerable<T> data)
    {
        if (data == null || !data.Any()) return Array.Empty<byte>();

        var sb = new StringBuilder();
        var properties = typeof(T).GetProperties();

        // Header
        sb.AppendLine(string.Join(",", properties.Select(p => p.Name)));

        // Rows
        foreach (var item in data)
        {
            var values = properties.Select(p => {
                var val = p.GetValue(item);
                if (val == null) return "";
                var str = val.ToString();
                if (str!.Contains(",") || str.Contains("\""))
                    return $"\"{str.Replace("\"", "\"\"")}\"";
                return str;
            });
            sb.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }
}
