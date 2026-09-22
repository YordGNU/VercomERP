namespace Vercom.Helpers
{
    public static class AuditConstants
    {
        public const string CanalErp = "ERP";
        public const string CanalApi = "API";
        public const string CanalPos = "POS";
        public const string CanalDefault = CanalErp;

        public static readonly HashSet<string> CanalesValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        CanalErp, CanalApi, CanalPos
    };

        public static string NormalizeCanal(string? canal)
        {
            if (string.IsNullOrWhiteSpace(canal))
                return CanalDefault;

            var limpio = canal.Trim().ToUpperInvariant();

            return CanalesValidos.Contains(limpio) ? limpio : CanalDefault;
        }
    }
}
