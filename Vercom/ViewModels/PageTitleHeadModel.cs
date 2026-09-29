namespace Vercom.ViewModels;

public class PageTitleHeadModel
{
    public string Title { get; set; } = "";
    public string? Subtitle { get; set; }
    public bool ShowBreadcrumb { get; set; } = true;
    public bool Padding { get; set; } = true;
    public List<BreadcrumbItem> BreadcrumbItems { get; set; } = new();
    public ActionLinkModel? PrimaryAction { get; set; }
}

public class BreadcrumbItem
{
    public string Text { get; set; } = "";
    public string? Url { get; set; }
    public bool IsActive { get; set; }
}

public class ActionLinkModel
{
    public string Controller { get; set; } = "";
    public string Action { get; set; } = "";
    public string Text { get; set; } = "";
    public string? Icon { get; set; }
    public string? Id { get; set; }
}