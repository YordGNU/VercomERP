namespace Vercom.ViewModels;

public class DataTableCardModel
{
    public string TableId { get; set; } = "dataTable";
    public string? Title { get; set; }
    public string ColumnClass { get; set; } = "col-xxl-12";
    public ActionLinkModel? CreateAction { get; set; }
    public List<DataTableFilter> Filters { get; set; } = new();
    public bool ShowClearButton { get; set; } = true;
    public string? ClearUrl { get; set; }
    public List<DataTableColumn> Columns { get; set; } = new();
    public List<DataTableRow> Rows { get; set; } = new();
    public string EmptyMessage { get; set; } = "Sin registros";
    public int SortColumnIndex { get; set; } = 0;
    public string SortDirection { get; set; } = "asc";
}

public class DataTableFilter
{
    public string Name { get; set; } = "";
    public string Label { get; set; } = "";
    public string ColumnClass { get; set; } = "col-md-3";
    public string Type { get; set; } = "text";
    public string? Value { get; set; }
    public string? Placeholder { get; set; }
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? Options { get; set; }
    public bool AutoSubmit { get; set; } = false;
    public object? Attributes { get; set; }
}

public class DataTableColumn
{
    public string Key { get; set; } = "";
    public string Header { get; set; } = "";
    public string? Width { get; set; }
    public Func<object, DataTableRow, object>? Template { get; set; }
    public object? TdAttributes { get; set; }
}

public class DataTableRow
{
    public Dictionary<string, object> Values { get; set; } = new();
    public object? Attributes { get; set; }

    public object GetValue(string key) => Values.TryGetValue(key, out var v) ? v : "";
}