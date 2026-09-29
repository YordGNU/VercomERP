namespace Vercom.ViewModels;

using Microsoft.AspNetCore.Html;

public class FormCardModel
{
    public string FormId { get; set; } = "formCard";
    public string Title { get; set; } = "";
    public string? Icon { get; set; }
    public string Controller { get; set; } = "";
    public string Action { get; set; } = "";
    public string ColumnClass { get; set; } = "col-lg-10";
    public bool ShowValidationSummary { get; set; } = true;
    public List<FormField> Fields { get; set; } = new();
    public List<FormField> FooterFields { get; set; } = new();
    public string? BackUrl { get; set; }
    public string SubmitText { get; set; } = "GUARDAR";
    public string SubmitIcon { get; set; } = "ti ti-device-floppy";
    public bool InitializeSelect2 { get; set; } = true;
}

public class FormField
{
    public string Expression { get; set; } = "";
    public string Label { get; set; } = "";
    public string ColumnClass { get; set; } = "col-md-4";
    public IHtmlContent InputHtml { get; set; } = new HtmlString("");
}