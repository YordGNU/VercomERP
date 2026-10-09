using Vercom.Services;
using Xunit;

namespace Vercom.Tests;

public class PriceResolutionServiceTests
{
    [Fact]
    public void Resolve_UsesListPrice_WhenActiveAndWithinValidity()
    {
        var result = PriceResolutionEngine.Resolve(
            basePrice: 100m,
            listPrice: 85m,
            hasActiveList: true,
            isInsideValidityWindow: true,
            listName: "Mayorista",
            listId: Guid.NewGuid());

        Assert.Equal(85m, result.Precio);
        Assert.Equal(PriceSource.ListaPrecio, result.Source);
        Assert.Equal("LISTA_CLIENTE", result.RuleName);
    }

    [Fact]
    public void Resolve_FallsBackToBase_WhenListIsNotAvailable()
    {
        var result = PriceResolutionEngine.Resolve(
            basePrice: 100m,
            listPrice: null,
            hasActiveList: false,
            isInsideValidityWindow: true,
            listName: null,
            listId: null);

        Assert.Equal(100m, result.Precio);
        Assert.Equal(PriceSource.Plano, result.Source);
        Assert.Equal("BASE", result.RuleName);
    }

    [Fact]
    public void Resolve_FallsBackToBase_WhenListIsExpired()
    {
        var result = PriceResolutionEngine.Resolve(
            basePrice: 120m,
            listPrice: 90m,
            hasActiveList: true,
            isInsideValidityWindow: false,
            listName: "Mayorista",
            listId: Guid.NewGuid());

        Assert.Equal(120m, result.Precio);
        Assert.Equal(PriceSource.Plano, result.Source);
        Assert.Equal("BASE", result.RuleName);
    }
}
