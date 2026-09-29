namespace Vercom.Services;

public interface IEntidadDisplayService
{
    string Name { get; }
}

public class EntidadDisplayService : IEntidadDisplayService
{
    public string Name => "yordanisvc@gmail.com";
}
