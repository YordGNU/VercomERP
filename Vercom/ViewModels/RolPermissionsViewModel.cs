using Vercom.Models;

namespace Vercom.ViewModels;

public class RolPermissionsViewModel
{
    public Rol Rol { get; set; } = null!;
    public List<PermissionGroup> Modules { get; set; } = new();
    public string Title { get; set; } = "Gestión de Permisos por Rol";
}

public class PermissionGroup
{
    public string ModuleName { get; set; } = null!;
    public List<PermissionItem> Permissions { get; set; } = new();
}

public class PermissionItem
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public bool IsSelected { get; set; }
}
