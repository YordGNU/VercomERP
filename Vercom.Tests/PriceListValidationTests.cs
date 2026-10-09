using Vercom.Models;
using Vercom.Services;
using Xunit;

namespace Vercom.Tests;

public class PriceListValidationTests
{
    [Fact]
    public void ValidatePriceListDetails_ShouldRejectEmptyList()
    {
        var result = PriceListBusinessRules.ValidatePriceListDetails(new List<ListaPrecioDetalle>());

        Assert.False(result.Succeeded);
        Assert.Contains("al menos un producto", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidatePriceListDetails_ShouldRejectDuplicateProducts()
    {
        var productId = Guid.NewGuid();
        var details = new List<ListaPrecioDetalle>
        {
            new() { ProductoId = productId, Precio = 10m },
            new() { ProductoId = productId, Precio = 12m }
        };

        var result = PriceListBusinessRules.ValidatePriceListDetails(details);

        Assert.False(result.Succeeded);
        Assert.Contains("duplicado", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidatePriceListDetails_ShouldRejectNonPositivePrices()
    {
        var details = new List<ListaPrecioDetalle>
        {
            new() { ProductoId = Guid.NewGuid(), Precio = 0m },
        };

        var result = PriceListBusinessRules.ValidatePriceListDetails(details);

        Assert.False(result.Succeeded);
        Assert.Contains("mayor que cero", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
