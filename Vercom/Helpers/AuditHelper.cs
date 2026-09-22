namespace Vercom.Helpers
{
    public static class AuditHelper
    {

        public static string GetActionBadgeClass(string accion)
        {
            return accion switch
            {
                "INSERT" => "bg-success",
                "UPDATE" => "bg-warning text-dark",
                "DELETE" => "bg-danger",
                "LOGIN_FALLIDO" => "bg-danger",
                _ => "bg-info text-dark"
            };
        }


        public static string GetActionIcon(string accion)
        {
            return accion switch
            {
                "INSERT" => "plus",
                "UPDATE" => "pencil",
                "DELETE" => "trash",
                "LOGIN_FALLIDO" => "alert-triangle",
                _ => "circle-check"
            };
        }

        public static string GetChannelIcon(string canal)
        {
            return canal?.ToUpperInvariant() switch
            {
                "ERP" => "globe",
                "POS" => "device-desktop",
                "API" => "api",
                "MOVIL" => "device-mobile",
                _ => "question-mark"
            };
        }
    }
}