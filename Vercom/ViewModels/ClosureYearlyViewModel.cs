namespace Vercom.ViewModels;

public class ClosureYearlyViewModel
{
    public short Year { get; set; }
    public decimal EstimatedProfit { get; set; }
    public bool CanClose { get; set; }
    public List<string> ValidationMessages { get; set; } = new();
}
