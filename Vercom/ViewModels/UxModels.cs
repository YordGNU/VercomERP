using Microsoft.AspNetCore.Html;

namespace Vercom.ViewModels;

public class PageHeaderModel
{
    public string? Eyebrow { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Icon { get; set; }
    public string Tone { get; set; } = "primary";
    public string? BadgeText { get; set; }
    public string? BadgeTone { get; set; }
    public IHtmlContent? ActionsHtml { get; set; }
}

public class KpiCardModel
{
    public string? Label { get; set; }
    public string? Value { get; set; }
    public string? Icon { get; set; }
    public string Tone { get; set; } = "primary";
    public string? Hint { get; set; }
    public string? ValueSuffix { get; set; }
    public bool Small { get; set; }
    public string? Href { get; set; }
}

public class EmptyStateModel
{
    public string? Icon { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string Tone { get; set; } = "light";
    public IHtmlContent? ActionHtml { get; set; }
    public bool InTable { get; set; }
    public int Colspan { get; set; } = 1;
}
