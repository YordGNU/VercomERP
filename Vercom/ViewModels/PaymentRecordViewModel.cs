using System.ComponentModel.DataAnnotations;

namespace Vercom.ViewModels;

public class PaymentRecordViewModel
{
    [Required]
    public Guid ItemId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
    public decimal Amount { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = "EFECTIVO";

    public string? Reference { get; set; }
}
