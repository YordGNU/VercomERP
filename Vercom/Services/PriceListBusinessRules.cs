using Vercom.Models;

namespace Vercom.Services;

public static class PriceListBusinessRules
{
    public sealed record ValidationResult(bool Succeeded, string Message)
    {
        public static ValidationResult Success() => new(true, string.Empty);
        public static ValidationResult Failure(string message) => new(false, message);
    }

    public static ValidationResult ValidatePriceListDetails(IEnumerable<ListaPrecioDetalle>? details)
    {
        if (details == null || !details.Any())
            return ValidationResult.Failure("La lista de precios debe tener al menos un producto.");

        var detalles = details.ToList();

        if (detalles.Any(d => d.ProductoId == Guid.Empty))
            return ValidationResult.Failure("Cada producto de la lista debe ser válido.");

        if (detalles.GroupBy(d => d.ProductoId).Any(g => g.Count() > 1))
            return ValidationResult.Failure("No se puede incluir un producto duplicado dentro de la misma lista de precios.");

        if (detalles.Any(d => d.Precio <= 0m))
            return ValidationResult.Failure("El precio de cada producto debe ser mayor que cero.");

        return ValidationResult.Success();
    }
}
